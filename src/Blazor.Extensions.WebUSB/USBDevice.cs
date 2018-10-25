using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Blazor.Extensions.WebUSB
{
    public class USBDevice
    {
        internal USB USB;
        public byte USBVersionMajor { get; private set; }
        public byte USBVersionMinor { get; private set; }
        public byte USBVersionSubminor { get; private set; }
        public byte DeviceClass { get; private set; }
        public byte DeviceSubclass { get; private set; }
        public byte DeviceProtocol { get; private set; }
        public int VendorId { get; private set; }
        public int ProductId { get; private set; }
        public byte DeviceVersionMajor { get; private set; }
        public byte DeviceVersionMinor { get; private set; }
        public byte DeviceVersionSubminor { get; private set; }
        public string ManufacturerName { get; private set; }
        public string ProductName { get; private set; }
        public string SerialNumber { get; private set; }
        public USBConfiguration Configuration { get; private set; }
        public List<USBConfiguration> Configurations { get; private set; }
        public bool Opened { get; private set; }
    }
}