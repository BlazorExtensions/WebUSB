namespace Blazor.Extensions.WebUSB
{
    public static class USBRequestType
    {
        public const string Standard = "standard";
        public const string Class = "class";
        public const string Vendor = "vendor";
    }

    public static class USBRecipient
    {
        public const string Device = "device";
        public const string Interface = "interface";
        public const string Endpoint = "endpoint";
        public const string Other = "other";
    }

    public class USBControlTransferParameters
    {
        public string RequestType { get; private set; }
        public string Recipient { get; private set; }
        public byte Request { get; private set; }
        public int Value { get; private set; }
        public int Index { get; private set; }
    }
}