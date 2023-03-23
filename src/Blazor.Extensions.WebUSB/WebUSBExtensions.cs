using Microsoft.Extensions.DependencyInjection;

namespace Blazor.Extensions.WebUSB
{
    /// <summary>
    /// The web usb extensions.
    /// </summary>
    public static class WebUSBExtensions
    {
        /// <summary>
        /// Uses the web usb.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns>An IServiceCollection.</returns>
        public static IServiceCollection UseWebUSB(this IServiceCollection services) => services.AddScoped<IUSB, USB>();
    }
}