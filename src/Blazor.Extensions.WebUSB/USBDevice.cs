using System;
using System.Collections.Generic;
using Microsoft.JSInterop;

namespace Blazor.Extensions.WebUSB
{
    public class USBDevice
    {
        public byte USBVersionMajor { get; set; }
        public byte USBVersionMinor { get; set; }
        public byte USBVersionSubminor { get; set; }
        public byte DeviceClass { get; set; }
        public byte DeviceSubclass { get; set; }
        public byte DeviceProtocol { get; set; }
        public short VendorId { get; private set; }
        public short ProductId { get; private set; }
        public byte DeviceVersionMajor { get; set; }
        public byte DeviceVersionMinor { get; set; }
        public byte DeviceVersionSubminor { get; set; }
        public string ManufacturerName { get; set; }
        public string ProductName { get; set; }
        public string SerialNumber { get; set; }
        public USBConfiguration Configuration { get; set; }
        public List<USBConfiguration> Configurations { get; set; }
        public bool Opened { get; set; }
    }
}