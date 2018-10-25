namespace Blazor.Extensions.WebUSB
{
    public class USBDeviceFilter 
    {
        public int? VendorId { get; set; }
        public int? ProductId { get; set; }
        public byte? ClassCode { get; set; }
        public byte? SubClassCode { get; set; }
        public byte? ProtocolCode { get; set; }
        public string SerialNumber { get; set; }
    }
}   