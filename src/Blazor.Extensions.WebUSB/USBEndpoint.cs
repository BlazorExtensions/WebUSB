using System.Text.Json.Serialization;

namespace Blazor.Extensions.WebUSB
{
    public static class USBDirection
    {
        public const string In = "in";
        public const string Out = "out";
    }

    public static class USBEndpointType
    {
        public const string Bulk = "bulk";
        public const string Interrupt = "interrupt";
        public const string Isochronous = "isochronous";
    }

    public class USBEndpoint
    {
        [JsonPropertyName("endpointNumber")]
        public byte EndpointNumber { get; set; }

        [JsonPropertyName("direction")]
        public string Direction { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("packetSize")]
        public long PacketSize { get; set; }
    }
}