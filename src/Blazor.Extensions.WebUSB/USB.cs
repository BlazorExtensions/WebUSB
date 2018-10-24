using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Blazor.Extensions.WebUSB
{
    internal class USB : IUSB
    {
        private static readonly List<USBDeviceFilter> _emptyFilters = new List<USBDeviceFilter>();
        private const string REQUEST_DEVICE_METHOD = "BlazorExtensions.WebUSB.RequestDevice";
        private const string GET_DEVICE_METHOD = "BlazorExtensions.WebUSB.GetDevices";
        private const string ADD_ON_CONNECT_METHOD = "BlazorExtensions.WebUSB.AddOnConnectHandler";
        // public event EventHandler OnDisconnect;

        public Task<USBDevice[]> GetDevices() => JSRuntime.Current.InvokeAsync<USBDevice[]>(GET_DEVICE_METHOD);

        public async Task<USBDevice> RequestDevice(USBDeviceRequestOptions options = null)
        {
            try
            {
                if (options == null)
                    options = new USBDeviceRequestOptions { Filters = _emptyFilters };
                return await JSRuntime.Current.InvokeAsync<USBDevice>(REQUEST_DEVICE_METHOD, options);
            }
            catch (JSException)
            {
                return null;
            }
        }

        public async Task<IDisposable> OnConnect(Func<USBDevice, Task> callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            var callbackReference = new OnConnectCallback(Guid.NewGuid().ToString(), callback);
            await JSRuntime.Current.InvokeAsync<object>(ADD_ON_CONNECT_METHOD, new DotNetObjectRef(callbackReference));
            return callbackReference;
        }
    }
}