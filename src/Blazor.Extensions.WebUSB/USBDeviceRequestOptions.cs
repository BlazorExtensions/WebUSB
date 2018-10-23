using System.Collections.Generic;

namespace Blazor.Extensions.WebUSB
{
    public class USBDeviceRequestOptions
    {
        public List<USBDeviceFilter> Filters { get; set; }
    }

    internal class _USBDeviceRequestOptions
    {
        public List<_USBDeviceFilter> filters;
    }
}