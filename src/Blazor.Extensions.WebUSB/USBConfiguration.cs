using System.Collections.Generic;

namespace Blazor.Extensions.WebUSB
{
    public class USBConfiguration
    {
        public byte ConfigurationValue { get; internal set; }
        public string ConfigurationName { get; private set; }
        public List<USBInterface> Interfaces { get; private set; }
    }
}