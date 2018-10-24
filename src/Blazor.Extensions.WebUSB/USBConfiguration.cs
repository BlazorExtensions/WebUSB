using System.Collections.Generic;

namespace Blazor.Extensions.WebUSB
{
    public class USBConfiguration
    {
        public byte ConfigurationValue { get; set; }
        public string ConfigurationName { get; set; }
        public List<USBInterface> Interfaces { get; set; }
    }
}