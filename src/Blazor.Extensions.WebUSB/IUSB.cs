using System;
using System.Threading.Tasks;

namespace Blazor.Extensions.WebUSB
{
    public interface IUSB
    {
        event Action<USBDevice> OnDisconnect;
        event Action<USBDevice> OnConnect;
        Task Initialize();
        Task<USBDevice[]> GetDevices();
        Task<USBDevice> RequestDevice(USBDeviceRequestOptions options = null);
    }
}