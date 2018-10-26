namespace Blazor.Extensions.WebUSB
{
    public static class USBTransferStatus
    {
        public const string OK = "ok";
        public const string Stall = "stall";
        public const string Babble = "babble";
    }

    public abstract class USBTransferResult
    {
        public string Status { get; protected set; }
    }

    public class USBInTransferResult : USBTransferResult
    {
        public byte[] Data { get; private set; }
    }

    public class USBOutTransferResult : USBTransferResult
    {
        public long BytesWritten { get; private set; }
    }
}