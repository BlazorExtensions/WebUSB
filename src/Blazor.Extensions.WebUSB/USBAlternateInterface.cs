using System.Text.Json.Serialization;

namespace Blazor.Extensions.WebUSB
{
    public class USBAlternateInterface
    {
        [JsonPropertyName("alternateSetting")]
        public byte AlternateSetting { get; set; }

        [JsonPropertyName("interfaceClass")]
        public byte InterfaceClass { get; set; }

        [JsonPropertyName("interfaceSubclass")]
        public byte InterfaceSubclass { get; set; }

        [JsonPropertyName("interfaceProtocol")]
        public byte InterfaceProtocol { get; set; }

        [JsonPropertyName("interfaceName")]
        public string InterfaceName { get; set; }

        [JsonPropertyName("endpoints")]
        public USBEndpoint[] Endpoints { get; set; }
    }
}