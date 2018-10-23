using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Blazor.Extensions.WebUSB
{
    internal class USB : IUSB
    {
        private static readonly List<_USBDeviceFilter> _emptyFilters = new List<_USBDeviceFilter>();
        // private const string REQUEST_DEVICE_METHOD = "navigator.usb.requestDevice";
        // private const string GET_DEVICE_METHOD = "navigator.usb.getDevices";
        private const string REQUEST_DEVICE_METHOD = "BlazorExtensions.WebUSB.RequestDevice";
        private const string GET_DEVICE_METHOD = "BlazorExtensions.WebUSB.GetDevices";
        // public event EventHandler OnConnect;
        // public event EventHandler OnDisconnect;

        public async Task<USBDevice[]> GetDevices() => (await JSRuntime.Current.InvokeAsync<_USBDevice[]>(GET_DEVICE_METHOD)).Select(d => new USBDevice(d)).ToArray();

        public async Task<USBDevice> RequestDevice(USBDeviceRequestOptions options = null)
        {
            var internalOptions = new _USBDeviceRequestOptions();
            if (options != null)
            {
                internalOptions.filters =
                    options.Filters.Select(f =>
                        new _USBDeviceFilter
                        {
                            classCode = f.ClassCode,
                            productId = f.ProductId,
                            protocolCode = f.ProtocolCode,
                            serialNumber = f.SerialNumber,
                            subClassCode = f.SubClassCode,
                            vendorId = f.VendorId
                        }).ToList();
            }
            else
            {
                internalOptions.filters = _emptyFilters;
            }

            var device = await JSRuntime.Current.InvokeAsync<_USBDevice>(REQUEST_DEVICE_METHOD, internalOptions);
            if (device == null) return null;
            return new USBDevice(device);
        }
    }
}