using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Blazor.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Blazor.Extensions.WebUSB.Test
{
    public class TestPage : ComponentBase
    {
        [Inject] private IUSB _usb { get; set; }
        [Inject] private ILogger<TestPage> _logger { get; set; }
        protected string productId;
        protected string vendorId;

        private bool _initialized = false;

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            if (!_initialized)
            {
                this._usb.OnConnect += this.OnConnect;
                this._usb.OnDisconnect += this.OnDisconnect;
                this._initialized = true;
                return this._usb.Initialize();
            }
            return Task.CompletedTask;
        }

        private void OnConnect(USBDevice device)
        {
            this._logger.LogInformation("OnConnect:");
            this._logger.LogInformation(device);
        }

        private void OnDisconnect(USBDevice device)
        {
            this._logger.LogInformation("OnDisconnect:");
            this._logger.LogInformation(device);
        }

        protected async Task RequestDevices()
        {
            USBDevice device = null;

            if (string.IsNullOrEmpty(this.productId) && string.IsNullOrEmpty(this.vendorId))
            {
                device = await this._usb.RequestDevice();
            }
            else
            {
                device = await this._usb.RequestDevice(new USBDeviceRequestOptions
                {
                    Filters = new List<USBDeviceFilter>
                    {
                        new USBDeviceFilter {VendorId = 0x079b, ProductId = 0x0028},
                        new USBDeviceFilter {VendorId = 0x1753, ProductId = 0xC902}
                        // new USBDeviceFilter { VendorId = Convert.ToUInt16(this.vendorId, 16) , ProductId = Convert.ToUInt16(this.productId, 16) }
                    }
                });
            }

            if (device != null)
            {
                device = await device.Open();
                device = await device.SelectConfiguration(device.Configuration);
                // device = await device.SelectConfiguration(1);
                device = await device.ClaimBulkInterface();
                this._logger.LogInformation(device);

                var outResult = await device.TransferOut(2, new byte[] { 1, 2, 3 });
                this._logger.LogInformation("Write response:");
                this._logger.LogInformation(outResult);
                var inResult = await device.TransferIn(1, 3);
                this._logger.LogInformation("Read response:");
                this._logger.LogInformation(inResult);
            }
        }

        protected async Task ListDevices()
        {
            var devices = await this._usb.GetDevices();
            this._logger.LogInformation(devices);
        }
    }
}