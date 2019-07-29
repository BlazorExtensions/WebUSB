using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Blazor.Extensions.Logging;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Blazor.Extensions.WebUSB.Test
{
    public class BCTestComponent : ComponentBase
    {
        [Inject] private IUSB _usb { get; set; }
        [Inject] private ILogger<BCTestComponent> _logger { get; set; }
        [Inject] private IJSRuntime _runtime { get; set; }
        private bool _initialized = false;

        private static readonly List<USBDeviceFilter> _filters = new List<USBDeviceFilter>
        {
            new USBDeviceFilter {VendorId = 0x079b, ProductId = 0x0028}, // Ingenico
            new USBDeviceFilter {VendorId = 0x1753, ProductId = 0xC902} // Gertec
        };

        protected override Task OnAfterRenderAsync()
        {
            if (!_initialized)
            {
                this._usb.OnConnect += OnConnect;
                this._usb.OnDisconnect += OnDisconnect;
                this._initialized = true;
                return this._usb.Initialize();
            }
            return Task.CompletedTask;
        }

        private async Task<USBDevice> GetDevice()
        {
            USBDevice device = await this._usb.RequestDevice(new USBDeviceRequestOptions
            {
                Filters = _filters
            });
            return device;
        }

        protected async Task Run()
        {
            USBDevice device = await this.GetDevice();

            if (device != null)
            {
                try
                {
                    device = await device.Open();
                    device = await device.SelectConfiguration(device.Configuration);
                    device = await device.ClaimBulkInterface();
                    this._logger.LogInformation("PinPad selected:");
                    this._logger.LogInformation("PinPad information:");
                    var info = await this.Open(device);
                    if (info == null)
                    {
                        this._logger.LogWarning("Unable to read device info...");
                    }
                    else
                    {
                        this._logger.LogInformation(info);
                    }

                }
                catch (Exception exc)
                {
                    this._logger.LogError(exc);
                }
                finally
                {
                    await Close(device);
                    await device.ReleaseBulkInterface();
                    await device.Close();
                }
            }
            else
            {
                this._logger.LogWarning("Fail to get device.");
            }
        }

        internal async Task<DeviceInfo> Open(USBDevice device)
        {
            var readed = await WritePacket(device, GetCommandData(CommandTypes.Open));
            this._logger.LogInformation(readed);
            var ret = GetReturnCode(Encoding.ASCII.GetString(readed));
            if (ret != ReturnCodes.Ok && ret != ReturnCodes.AlreadyOpen)
            {
                this._logger.LogError("Unable to open PinPad.");
                return null;
            }
            await Cancel(device);
            readed = await WritePacket(device, GetCommandData(CommandTypes.Open));
            ret = GetReturnCode(Encoding.ASCII.GetString(readed));
            if (ret != ReturnCodes.Ok && ret != ReturnCodes.AlreadyOpen)
            {
                this._logger.LogError("Unable to open PinPad.");
                return null;
            }

            await DisplayMessage(device, "Getting device info...");

            readed = await WritePacket(device, GetCommandData(CommandTypes.GetInfo, ((short)0).ToString().PadLeft(2, '0')));
            ret = GetReturnCode(Encoding.ASCII.GetString(readed));
            if (ret != ReturnCodes.Ok)
            {
                this._logger.LogError("Unable to read device info.");
                return null;
            }
            return new DeviceInfo(readed);
        }

        internal async Task Close(USBDevice device)
        {
            var result = await WritePacket(device, GetCommandData(CommandTypes.Close, "Done!"));
            var ret = GetReturnCode(Encoding.ASCII.GetString(result));
            if (ret != ReturnCodes.Ok) throw new InvalidOperationException("Unable to access display.");
        }

        public async Task DisplayMessage(USBDevice device, string text)
        {
            var result = await WritePacket(device, GetCommandData(CommandTypes.DisplayEx, $"{text.Length.ToString().PadLeft(3, '0')}{text}"));
            var ret = GetReturnCode(Encoding.ASCII.GetString(result));
            if (ret != ReturnCodes.Ok) throw new InvalidOperationException("Unable to access display.");
        }

        public async Task Cancel(USBDevice device)
        {
            await device.TransferOut(2, new byte[] { PinPadConstants.CAN });
            await device.TransferIn(1, 1);
        }

        private ReturnCodes GetReturnCode(string raw) => (ReturnCodes)Enum.Parse(typeof(ReturnCodes), raw.Substring(3, 3));

        private byte[] GetCommandData(string type, params string[] parameters)
        {
            var cmdBuilder = new StringBuilder();
            cmdBuilder.Append(type);

            var paramBuilder = new StringBuilder();
            foreach (var param in parameters)
            {
                paramBuilder.Append(param.Length.ToString().PadLeft(3, '0'));
                paramBuilder.Append(param);
            }

            if (paramBuilder.Length > 0)
            {
                cmdBuilder.Append(paramBuilder.ToString());
            }

            return Encoding.ASCII.GetBytes(cmdBuilder.ToString());
        }

        private async Task<byte[]> WritePacket(USBDevice device, byte[] data)
        {
            var toCRC = new MemoryStream();
            await toCRC.WriteAsync(data, 0, data.Length);
            toCRC.WriteByte(PinPadConstants.ETB);
            var crc = toCRC.ToArray().ComputeCRC16();

            // <SYN>[MESSAGE]<ETB>{CRC}
            var output = new MemoryStream();
            output.WriteByte(PinPadConstants.SYN); //<SYN>
            await output.WriteAsync(data, 0, data.Length); //[MESSAGE]
            output.WriteByte(PinPadConstants.ETB); //<ETB>
            await output.WriteAsync(crc, 0, crc.Length); //{CRC}

            var outputData = output.ToArray();
            this._logger.LogInformation($"{PinPadConstants.APP_TO_PINPAD} {PacketToString(outputData)}");

            var trxResult = await device.TransferOut(2, outputData);

            if (trxResult.Status != USBTransferStatus.OK)
            {
                this._logger.LogWarning("NOT OK!!!");
                this._logger.LogWarning(trxResult);
            }
            this._logger.LogInformation(trxResult);
            int sendFailures = 0;
            var readResult = await device.TransferIn(1, 64);
            this._logger.LogInformation(readResult);
            var read = readResult.Data[0];

            while (read == PinPadConstants.NAK && sendFailures < 3)
            {
                this._logger.LogInformation($"{PinPadConstants.PINPAD_TO_APP} {PinPadConstants.NAK_STR}");
                trxResult = await device.TransferOut(2, outputData);
                if (trxResult.Status != USBTransferStatus.OK)
                {
                    this._logger.LogWarning("WRITE NOT OK!!!");
                    this._logger.LogWarning(trxResult);
                }
                readResult = await device.TransferIn(1, 64);
                if (readResult.Status != USBTransferStatus.OK)
                {
                    this._logger.LogWarning("READ NOT OK!!!");
                    this._logger.LogWarning(readResult);
                }
                read = readResult.Data[0];
                sendFailures++;
                this._logger.LogInformation($"Failures: {sendFailures}");
            }

            if (read != PinPadConstants.ACK) throw new InvalidOperationException(PinPadConstants.NAK_STR);

            this._logger.LogInformation($"{PinPadConstants.PINPAD_TO_APP} {PinPadConstants.ACK_STR}");

            var response = new MemoryStream();

            if (readResult.Data.Length > 1 && readResult.Data.Length < 64)
            {
                await response.WriteAsync(readResult.Data, 0, readResult.Data.Length);
            }
            else
            {
                do
                {
                    this._logger.LogInformation($"Reading mode data...");
                    readResult = await device.TransferIn(1, 64);
                    await response.WriteAsync(readResult.Data, 0, readResult.Data.Length);
                } while (readResult.Data.Length == 64);
            }

            this._logger.LogInformation($"Data length: {response.Length}");

            return await ReadPacket(device, response.ToArray());
        }

        internal async Task<byte[]> ReadPacket(USBDevice device, byte[] packet)
        {
            var input = new MemoryStream();

            for (int i = 0; i < packet.Length; i++)
            {
                var readedByte = packet[i];
                if (readedByte == PinPadConstants.ACK || readedByte == PinPadConstants.NAK) continue;
                input.WriteByte(readedByte);

                if (readedByte == PinPadConstants.SYN) continue;

                if (readedByte == PinPadConstants.ETB)
                {
                    var data = input.ToArray().Skip(1).ToArray();
                    await device.TransferOut(2, new byte[] { PinPadConstants.ACK });
                    return data.Take(data.Length - 1).ToArray();
                }
            }
            return null;
        }

        private string PacketToString(byte[] ba)
        {
            var withoutSYN = ba.Skip(1).ToArray();
            var data = withoutSYN.Take(withoutSYN.Length - 3).ToArray();
            string ascii = Encoding.ASCII.GetString(data);
            var sb = new StringBuilder();
            sb.Append(PinPadConstants.SYN_STR);
            sb.Append(ascii);
            sb.Append(PinPadConstants.ETB_STR);
            var lrc = withoutSYN[withoutSYN.Length - 1];
            sb.AppendFormat("{0:X2}", lrc);
            return sb.ToString();
        }

        private void OnConnect(USBDevice device)
        {
            if (_filters.Any(f => f.ProductId == device.ProductId && f.VendorId == device.VendorId))
            {
                this._logger.LogInformation("PinPad connected:");
                this._logger.LogInformation(device);
            }
        }

        private void OnDisconnect(USBDevice device)
        {
            if (_filters.Any(f => f.ProductId == device.ProductId && f.VendorId == device.VendorId))
            {
                this._logger.LogInformation("PinPad disconnected:");
                this._logger.LogInformation(device);
            }
        }
    }

    internal static class CommandTypes
    {
        public const string Open = "OPN";
        public const string Close = "CLO";
        public const string DisplayEx = "DEX";
        public const string GetInfo = "GIN";
    }

    internal static class PinPadConstants
    {
        public const string APP_TO_PINPAD = "[APP->PINPAD]";
        public const string PINPAD_TO_APP = "[PINPAD->APP]";
        public const string ACK_STR = "<ACK>";
        public const string NAK_STR = "<NAK>";
        public const string SYN_STR = "<SYN>";
        public const string ETB_STR = "<ETB>";
        public const string EOT_STR = "<EOT>";
        public const string CAN_STR = "<CAN>";
        public const byte SYN = 0x16;
        public const byte ACK = 0x06;
        public const byte NAK = 0x15;
        public const byte CAN = 0x18;
        public const byte ETB = 0x17;
        public const byte EOT = 0x04;
    }

    internal enum ReturnCodes : short
    {
        Ok = 0,
        Processing = 1,
        AlreadyOpen = 14,
        NotOpen = 15,
    }

    internal sealed class DeviceInfo
    {
        public string Maker { get; private set; }
        public string Model { get; private set; }
        public bool IsContactlessSupported { get; private set; }
        public string Firmware { get; private set; }
        public string BCVersion { get; private set; }
        public string ApplicationBaseVersion { get; private set; }
        public string SerialNumber { get; private set; }

        internal DeviceInfo(byte[] data)
        {
            var str = Encoding.ASCII.GetString(data.Skip(9).ToArray());
            Maker = str.Substring(0, 20).Trim();
            Model = str.Substring(20, 19).Trim();
            var ctlsSupport = str.Substring(39, 1);
            IsContactlessSupported = ctlsSupport == "C" ? true : false;
            Firmware = str.Substring(40, 20);
            BCVersion = str.Substring(60, 4);
            ApplicationBaseVersion = str.Substring(64, 16).Trim();
            SerialNumber = str.Substring(80).Trim();
        }
    }

    internal static class CRC16
    {
        public static byte[] ComputeCRC16(this byte[] data)
        {
            return BitConverter.GetBytes(data.Compute()).Reverse().ToArray();
        }

        private static ushort Compute(this byte[] data)
        {
            ushort wCRC = 0;
            for (int i = 0; i < data.Length; i++)
            {
                wCRC ^= (ushort)(data[i] << 8);
                for (int j = 0; j < 8; j++)
                {
                    if ((wCRC & 0x8000) != 0)
                        wCRC = (ushort)((wCRC << 1) ^ 0x1021);
                    else
                        wCRC <<= 1;
                }
            }
            return wCRC;
        }
    }
}