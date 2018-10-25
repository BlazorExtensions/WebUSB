using System.Collections.Generic;

namespace Blazor.Extensions.WebUSB
{
    public class USBAlternateInterface
    {
        public byte AlternateSetting { get; private set; }
        public byte InterfaceClass { get; private set; }
        public byte InterfaceSubclass { get; private set; }
        public byte InterfaceProtocol { get; private set; }
        public string InterfaceName { get; private set; }
        public List<USBEndpoint> Endpoints { get; private set; }
    }
}