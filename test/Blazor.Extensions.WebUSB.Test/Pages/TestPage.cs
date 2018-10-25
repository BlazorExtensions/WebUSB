using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.Extensions.Logging;
using Blazor.Extensions.Logging;
using System.Collections.Generic;

namespace Blazor.Extensions.WebUSB.Test
{
    public class TestPage : BlazorComponent
    {
        [Inject] private IUSB _usb { get; set; }
        [Inject] private ILogger<TestPage> _logger { get; set; }
        protected string productId;
        protected string vendorId;

        protected override Task OnInitAsync()
        {
            this._usb.OnConnect += OnConnect;
            this._usb.OnDisconnect += OnDisconnect;
            return this._usb.Initialize();
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
                device = await device.ClaimBulkInterface();
                this._logger.LogInformation(device);
            }
        }

        protected async Task ListDevices()
        {
            var devices = await this._usb.GetDevices();
            this._logger.LogInformation(devices);
        }
    }
}