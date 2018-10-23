using Microsoft.Extensions.DependencyInjection;

namespace Blazor.Extensions.WebUSB
{
    public static class WebUSBExtensions
    {
        public static IServiceCollection UseWebUSB(this IServiceCollection services) => services.AddSingleton<IUSB, USB>();
    }
}