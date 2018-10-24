using System;
using System.Threading.Tasks;

namespace Blazor.Extensions.WebUSB
{
    public interface IUSB
    {
        Task<USBDevice[]> GetDevices();
        Task<USBDevice> RequestDevice(USBDeviceRequestOptions options = null);
        Task<IDisposable> OnConnect(Func<USBDevice, Task> callback);
    }
}