using System.Collections.Generic;

namespace Blazor.Extensions.WebUSB
{
    public class USBAlternateInterface
    {
        public byte AlternateSetting { get; set; }
        public byte InterfaceClass { get; set; }
        public byte InterfaceSubclass { get; set; }
        public byte InterfaceProtocol { get; set; }
        public string InterfaceName { get; set; }
        public List<USBEndpoint> Endpoints { get; set; }
    }
}