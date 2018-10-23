namespace Blazor.Extensions.WebUSB
{
    public class USBDeviceFilter 
    {
        public ushort VendorId { get; set; }
        public ushort ProductId { get; set; }
        public byte ClassCode { get; set; }
        public byte SubClassCode { get; set; }
        public byte ProtocolCode { get; set; }
        public string SerialNumber { get; set; }
    }

    internal class _USBDeviceFilter 
    {
        public ushort vendorId;
        public ushort productId;
        public byte classCode;
        public byte subClassCode;
        public byte protocolCode;
        public string serialNumber;
    }
}   