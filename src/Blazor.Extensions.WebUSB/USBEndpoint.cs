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
        public byte EndpointNumber { get; private set; }
        public string Direction { get; private set; }
        public string Type { get; private set; }
        public long PacketSize { get; private set; }
    }
}