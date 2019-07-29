using System.Text.Json.Serialization;

namespace Blazor.Extensions.WebUSB
{
    public class USBDevice
    {
        internal USB USB { get; set; }

        [JsonPropertyName("usbVersionMajor")]
        public byte USBVersionMajor { get; set; }

        [JsonPropertyName("usbVersionMinor")]
        public byte USBVersionMinor { get; set; }

        [JsonPropertyName("usbVersionSubminor")]
        public byte USBVersionSubminor { get; set; }

        [JsonPropertyName("deviceClass")]
        public byte DeviceClass { get; set; }

        [JsonPropertyName("deviceSubclass")]
        public byte DeviceSubclass { get; set; }

        [JsonPropertyName("deviceProtocol")]
        public byte DeviceProtocol { get; set; }

        [JsonPropertyName("vendorId")]
        public int VendorId { get; set; }

        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("deviceVersionMajor")]
        public byte DeviceVersionMajor { get; set; }

        [JsonPropertyName("deviceVersionMinor")]
        public byte DeviceVersionMinor { get; set; }

        [JsonPropertyName("deviceVersionSubminor")]
        public byte DeviceVersionSubminor { get; set; }

        [JsonPropertyName("manufacturerName")]
        public string ManufacturerName { get; set; }

        [JsonPropertyName("productName")]
        public string ProductName { get; set; }

        [JsonPropertyName("serialNumber")]
        public string SerialNumber { get; set; }

        [JsonPropertyName("configuration")]
        public USBConfiguration Configuration { get; set; }

        [JsonPropertyName("configurations")]
        public USBConfiguration[] Configurations { get; set; }

        [JsonPropertyName("opened")]
        public bool Opened { get; set; }
    }
}