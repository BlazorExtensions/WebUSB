using System;
using System.Threading.Tasks;

namespace Blazor.Extensions.WebUSB
{
    public interface IUSB
    {
        // event EventHandler OnConnect;
        // event EventHandler OnDisconnect;
        Task<USBDevice[]> GetDevices();
        Task<USBDevice> RequestDevice(USBDeviceRequestOptions options = null);
    }
}