using System.Collections.Generic;

namespace Blazor.Extensions.WebUSB
{
    public class USBInterface
    {
        public byte InterfaceNumber { get; private set; }
        public USBAlternateInterface Alternate { get; private set; }
        public List<USBAlternateInterface> Alternates { get; private set; }
        public bool Claimed { get; private set; }
    }
}