using System.Text.Json.Serialization;

namespace Blazor.Extensions.WebUSB
{
    public class USBInterface
    {
        [JsonPropertyName("interfaceNumber")]
        public byte InterfaceNumber { get; set; }

        [JsonPropertyName("alternate")]
        public USBAlternateInterface Alternate { get; set; }

        [JsonPropertyName("alternates")]
        public USBAlternateInterface[] Alternates { get; set; }

        [JsonPropertyName("claimed")]
        public bool Claimed { get; set; }
    }
}