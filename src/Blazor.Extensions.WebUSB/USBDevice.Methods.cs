using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Blazor.Extensions.WebUSB
{
    public static class USBDeviceMethods
    {
        private const string OPEN_DEVICE_METHOD = "BlazorExtensions.WebUSB.OpenDevice";
        private const string SELECT_CONFIGURATION_METHOD = "BlazorExtensions.WebUSB.SelectConfiguration";
        private const string CLAIM_INTERFACE_METHOD = "BlazorExtensions.WebUSB.ClaimInterface";

        internal static void AttachUSB(this USBDevice device, USB usb) => device.USB = usb;
        public static Task<USBDevice> Open(this USBDevice device) => JSRuntime.Current.InvokeAsync<USBDevice>(OPEN_DEVICE_METHOD, device);
        public static Task<USBDevice> SelectConfiguration(this USBDevice device, USBConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            return JSRuntime.Current.InvokeAsync<USBDevice>(SELECT_CONFIGURATION_METHOD, device, configuration);
        }

        public static Task<USBDevice> ClaimInterface(this USBDevice device, USBInterface usbInterface)
        {
            if (usbInterface == null) throw new ArgumentNullException(nameof(usbInterface));

            return JSRuntime.Current.InvokeAsync<USBDevice>(CLAIM_INTERFACE_METHOD, device, usbInterface);
        }

        public static Task<USBDevice> ClaimBulkInterface(this USBDevice device)
        {
            var bulkInterface = device.Configuration.Interfaces.FirstOrDefault(i => i.Alternates.Any(a => a.Endpoints.Any(e => e.Type == USBEndpointType.Bulk)));
            if (bulkInterface == null) throw new InvalidOperationException("This devices doesn't have a Bulk interface");
            return device.ClaimInterface(bulkInterface);
        }
    }
}