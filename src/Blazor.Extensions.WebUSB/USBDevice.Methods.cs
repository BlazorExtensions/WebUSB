using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Blazor.Extensions.WebUSB
{
    public static class USBDeviceMethods
    {
        private const string OPEN_DEVICE_METHOD = "BlazorExtensions.WebUSB.OpenDevice";
        private const string CLOSE_DEVICE_METHOD = "BlazorExtensions.WebUSB.CloseDevice";
        private const string RESET_DEVICE_METHOD = "BlazorExtensions.WebUSB.ResetDevice";
        private const string SELECT_CONFIGURATION_METHOD = "BlazorExtensions.WebUSB.SelectConfiguration";
        private const string CLAIM_INTERFACE_METHOD = "BlazorExtensions.WebUSB.ClaimInterface";
        private const string CLEAR_HALT_METHOD = "BlazorExtensions.WebUSB.ClearHalt";
        private const string RELEASE_INTERFACE_METHOD = "BlazorExtensions.WebUSB.ReleaseInterface";
        private const string SELECT_ALTERNATE_INTERFACE_METHOD = "BlazorExtensions.WebUSB.SelectAlternateInterface";
        private const string TRANSFER_OUT_METHOD = "BlazorExtensions.WebUSB.TransferOut";
        private const string TRANSFER_IN_METHOD = "BlazorExtensions.WebUSB.TransferIn";
        private const string CONTROL_TRANSFER_OUT_METHOD = "BlazorExtensions.WebUSB.ControlTransferOut";
        private const string CONTROL_TRANSFER_IN_METHOD = "BlazorExtensions.WebUSB.ControlTransferIn";

        public static async Task<USBDevice> Open(this USBDevice device)
        {
            var updatedDevice = await device.USB.JSRuntime.InvokeAsync<USBDevice>(OPEN_DEVICE_METHOD, device);
            updatedDevice.AttachUSB(device.USB);
            return updatedDevice;
        }
        public static async Task<USBDevice> Close(this USBDevice device)
        {
            var updatedDevice = await device.USB.JSRuntime.InvokeAsync<USBDevice>(CLOSE_DEVICE_METHOD, device);
            updatedDevice.AttachUSB(device.USB);
            return updatedDevice;
        }
        public static async Task<USBDevice> Reset(this USBDevice device)
        {
            var updatedDevice = await device.USB.JSRuntime.InvokeAsync<USBDevice>(RESET_DEVICE_METHOD, device);
            updatedDevice.AttachUSB(device.USB);
            return updatedDevice;
        }

        internal static void AttachUSB(this USBDevice device, USB usb)
        {
            device.USB = usb;
        }

        public static Task<USBDevice> SelectConfiguration(this USBDevice device, USBConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            return device.SelectConfiguration(configuration.ConfigurationValue);
        }

        public static async Task<USBDevice> SelectConfiguration(this USBDevice device, byte configuration)
        {
            var updatedDevice = await device.USB.JSRuntime.InvokeAsync<USBDevice>(SELECT_CONFIGURATION_METHOD, device, configuration);
            updatedDevice.AttachUSB(device.USB);
            return updatedDevice;
        }

        public static Task<USBDevice> ClaimInterface(this USBDevice device, USBInterface usbInterface)
        {
            if (usbInterface == null) throw new ArgumentNullException(nameof(usbInterface));

            return device.ClaimInterface(usbInterface.InterfaceNumber);
        }

        public static async Task<USBDevice> ClaimInterface(this USBDevice device, byte interfaceNumber)
        {
            var updatedDevice = await device.USB.JSRuntime.InvokeAsync<USBDevice>(CLAIM_INTERFACE_METHOD, device, interfaceNumber);
            updatedDevice.AttachUSB(device.USB);
            return updatedDevice;
        }

        public static Task<USBDevice> ClaimBulkInterface(this USBDevice device)
        {
            var bulkInterface = device.Configuration.Interfaces.FirstOrDefault(i => i.Alternates.Any(a => a.Endpoints.Any(e => e.Type == USBEndpointType.Bulk)));
            if (bulkInterface == null) throw new InvalidOperationException("This devices doesn't have a Bulk interface");
            return device.ClaimInterface(bulkInterface.InterfaceNumber);
        }

        public static Task<USBDevice> ReleaseBulkInterface(this USBDevice device)
        {
            var bulkInterface = device.Configuration.Interfaces.FirstOrDefault(i => i.Alternates.Any(a => a.Endpoints.Any(e => e.Type == USBEndpointType.Bulk)));
            if (bulkInterface == null) throw new InvalidOperationException("This devices doesn't have a Bulk interface");
            return device.ReleaseInterface(bulkInterface.InterfaceNumber);
        }

        public static Task<USBDevice> ReleaseInterface(this USBDevice device, USBInterface usbInterface)
        {
            if (usbInterface == null) throw new ArgumentNullException(nameof(usbInterface));

            return device.ReleaseInterface(usbInterface.InterfaceNumber);
        }

        public static async Task<USBDevice> ReleaseInterface(this USBDevice device, byte interfaceNumber)
        {
            var updatedDevice = await device.USB.JSRuntime.InvokeAsync<USBDevice>(RELEASE_INTERFACE_METHOD, device, interfaceNumber);
            updatedDevice.AttachUSB(device.USB);
            return updatedDevice;
        }

        public static Task<USBDevice> SelectAlternateInterface(this USBDevice device, USBInterface usbInterface, USBAlternateInterface usbAlternateInterface)
        {
            if (usbInterface == null) throw new ArgumentNullException(nameof(usbInterface));
            if (usbAlternateInterface == null) throw new ArgumentNullException(nameof(usbAlternateInterface));

            return device.SelectAlternateInterface(usbInterface.InterfaceNumber, usbAlternateInterface.AlternateSetting);
        }

        public static async Task<USBDevice> SelectAlternateInterface(this USBDevice device, byte interfaceNumber, byte alternateSetting)
        {
            var updatedDevice = await device.USB.JSRuntime.InvokeAsync<USBDevice>(SELECT_ALTERNATE_INTERFACE_METHOD, device, interfaceNumber, alternateSetting);
            updatedDevice.AttachUSB(device.USB);
            return updatedDevice;
        }

        public static Task<USBDevice> ClearHalt(this USBDevice device, USBEndpoint endpoint)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            return device.ClearHalt(endpoint.Direction, endpoint.EndpointNumber);
        }

        public static async Task<USBDevice> ClearHalt(this USBDevice device, string direction, byte endpointNumber)
        {
            var updatedDevice = await device.USB.JSRuntime.InvokeAsync<USBDevice>(CLEAR_HALT_METHOD, device, direction, endpointNumber);
            updatedDevice.AttachUSB(device.USB);
            return updatedDevice;
        }

        public static Task<USBInTransferResult> TransferIn(this USBDevice device, USBEndpoint endpoint, long length)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            return device.TransferIn(endpoint.EndpointNumber, length);
        }

        public static Task<USBInTransferResult> TransferIn(this USBDevice device, byte endpointNumber, long length)
        {
            Console.WriteLine(device.USB != null);
            Console.WriteLine(device.USB?.JSRuntime != null);
            Console.WriteLine(endpointNumber);
            Console.WriteLine(length);
            return device.USB.JSRuntime.InvokeAsync<USBInTransferResult>(TRANSFER_IN_METHOD, device, endpointNumber, length);
        }

        public static Task<USBOutTransferResult> TransferOut(this USBDevice device, USBEndpoint endpoint, byte[] data)
        {
            return device.TransferOut(endpoint.EndpointNumber, data);
        }

        public static Task<USBOutTransferResult> TransferOut(this USBDevice device, byte endpointNumber, byte[] data)
        {
            return device.USB.JSRuntime.InvokeAsync<USBOutTransferResult>(TRANSFER_OUT_METHOD, device, endpointNumber, data);
        }

        public static Task<USBInTransferResult> ControlTransferIn(this USBDevice device, USBControlTransferParameters setup, long length)
        {
            return device.USB.JSRuntime.InvokeAsync<USBInTransferResult>(CONTROL_TRANSFER_IN_METHOD, device, setup, length);
        }

        public static Task<USBOutTransferResult> ControlTransferOut(this USBDevice device, USBControlTransferParameters setup, byte[] data)
        {
            return device.USB.JSRuntime.InvokeAsync<USBOutTransferResult>(CONTROL_TRANSFER_OUT_METHOD, device, setup, data);
        }
    }
}