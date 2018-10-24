namespace Blazor.Extensions.WebUSB
{
    public class USBDeviceFilter 
    {
        public short? VendorId { get; set; }
        public short? ProductId { get; set; }
        public byte? ClassCode { get; set; }
        public byte? SubClassCode { get; set; }
        public byte? ProtocolCode { get; set; }
        public string SerialNumber { get; set; }
    }
}   