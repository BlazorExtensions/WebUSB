using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.Extensions.Logging;
using Blazor.Extensions.Logging;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using System.Text;

namespace Blazor.Extensions.WebUSB.Test
{
    public class BCTestComponent : BlazorComponent
    {
        [Inject] private IUSB _usb { get; set; }
        [Inject] private ILogger<BCTestComponent> _logger { get; set; }
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

        protected async Task Run()
        {
            USBDevice device = await this._usb.RequestDevice(new USBDeviceRequestOptions
            {
                Filters = _filters
            });

            if (device != null)
            {
                try
                {
                    device = await device.Open();
                    device = await device.SelectConfiguration(device.Configuration);
                    // device = await device.SelectConfiguration(1);
                    device = await device.ClaimBulkInterface();
                    this._logger.LogInformation("PinPad selected:");
                    this._logger.LogInformation(device);
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
        }

        internal async Task<DeviceInfo> Open(USBDevice device)
        {
            var readed = await WritePacket(device, GetCommandData(CommandTypes.Open));
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

            int sendFailures = 0;
            var readResult = await device.TransferIn(1, 64);
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
        public const string Abort = "CAN";
        public const string Close = "CLO";
        public const string Display = "DSP";
        public const string DisplayEx = "DEX";
        public const string GetKey = "GKY";
        public const string GetPIN = "GPN";
        public const string RemoveCard = "RMC";
        public const string Generic = "GEN";
        public const string CheckEvent = "CKE";
        public const string GetCard = "GCR";
        public const string GoOnChip = "GOC";
        public const string FinishChip = "FNC";
        public const string ChipDirect = "CHP";
        public const string ChangeParameter = "CNG";
        public const string GetInfo = "GIN";
        public const string EncryptBuffer = "ENB";
        public const string TableLoadInit = "TLI";
        public const string TableLoadRec = "TLR";
        public const string TableLoadEnd = "TLE";
        public const string GetDUKPT = "GDU";
        public const string GetTimeStamp = "GTS";
        public const string DefineWKPAN = "DWK";
        public const string Notification = "NTM";
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
        Notify = 2,
        F1 = 4,
        F2 = 5,
        F3 = 6,
        F4 = 7,
        Backspace = 8,
        /* Basic errors 10 -29 */
        InvalidCall = 10,
        InvalidParameter = 11,
        Timeout = 12,
        Cancel = 13,
        AlreadyOpen = 14,
        NotOpen = 15,
        ExecutionError = 16,
        InvalidModel = 17,
        NoFunction = 18,
        TableExpired = 20,
        TableError = 21,
        NoApplication = 22,
        ErrorRFU23 = 23,
        ErrorRFU24 = 24,
        ErrorRFU25 = 25,
        ErrorRFU26 = 26,
        ErrorRFU27 = 27,
        ErrorRFU28 = 28,
        ErrorRFU29 = 29,
        /* Communication/Protocol Errors 30 - 39 */
        PortError = 30,
        CommunicationError = 31,
        UnknownStatus = 32,
        ResponseError = 33,
        CommunicationTimeout = 34,
        ErrorRFU35 = 35,
        ErrorRFU36 = 36,
        ErrorRFU37 = 37,
        ErrorRFU38 = 38,
        ErrorRFU39 = 39,
        /* Pinpad basic errors 40 - 49 */
        InternalError = 40,
        MagStripeDataError = 41,
        PINError = 42,
        NoCard = 43,
        PINBusy = 44,
        ErrorRFU45 = 45,
        ErrorRFU46 = 46,
        ErrorRFU47 = 47,
        ErrorRFU48 = 48,
        ErrorRFU49 = 49,
        /* Errors processing cards with CHIP (SAM) 50 - 59 */
        SAMError = 50,
        NoSAM = 51,
        InvalidSAM = 52,
        ErrorRFU53 = 53,
        ErrorRFU54 = 54,
        ErrorRFU55 = 55,
        ErrorRFU56 = 56,
        ErrorRFU57 = 57,
        ErrorRFU58 = 58,
        ErrorRFU59 = 59,
        /* Errors procesing contact chip cards 60 - 79 */
        DumbCard = 60,
        CardError = 61,
        InvalidCard = 62,
        CardBlocked = 63,
        CardNotAuthorized = 64,
        CardExpired = 65,
        CardStructureError = 66,
        CardInvalidated = 67,
        CardProblem = 68,
        CardInvalidData = 69,
        CardApplicationNotAvailable = 70,
        CardApplicationNotAuthorized = 71,
        NoBalance = 72,
        LimitExceeded = 73,
        CardNotEffective = 74,
        VisaCashInvalidCurrency = 75,
        FallbackError = 76,
        ErrorRFU77 = 77,
        ErrorRFU78 = 78,
        ErrorRFU79 = 79,
        /* Errors processing contactless cards 80 - 89 */
        MultipleCTLS = 80,
        CommunicationErrorCTLS = 81,
        CardInvalidatedCTLS = 82,
        CardProblemCTLS = 83,
        CardApplicationNotAvailableCTLS = 84,
        CarApplicationNotAuthorizedCTLS = 85,
        ErrorRFU86 = 86,
        ErrorRFU87 = 87,
        ErrorRFU88 = 88,
        ErrorRFU89 = 89
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