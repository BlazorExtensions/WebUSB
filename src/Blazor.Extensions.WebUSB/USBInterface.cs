using System.Collections.Generic;

namespace Blazor.Extensions.WebUSB
{
    public class USBInterface
    {
        public byte InterfaceNumber { get; set; }
        public USBAlternateInterface Alternate { get; set; }
        public List<USBAlternateInterface> Alternates { get; set; }
        public bool Claimed { get; set; }
    }
}