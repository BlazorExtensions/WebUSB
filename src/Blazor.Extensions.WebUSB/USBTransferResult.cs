using System.Text.Json.Serialization;

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
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    public class USBInTransferResult : USBTransferResult
    {
        [JsonPropertyName("data")]
        public byte[] Data { get; set; }
    }

    public class USBOutTransferResult : USBTransferResult
    {
        [JsonPropertyName("bytesWritten")]
        public long BytesWritten { get; set; }
    }
}