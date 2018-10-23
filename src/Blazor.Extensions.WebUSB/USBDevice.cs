using System;
using Microsoft.JSInterop;

namespace Blazor.Extensions.WebUSB
{
    public class USBDevice
    {
        public uint VendorId { get; private set; }
        public uint ProductId { get; private set; }

        internal USBDevice(_USBDevice internalDevice)
        {
            this.VendorId = internalDevice.vendorId;
            this.ProductId = internalDevice.productId;
        }
    }

    internal class _USBDevice
    {
        public uint vendorId { get; set; }
        public uint productId { get; set; }
    }
}