using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Blazor.Extensions.WebUSB.Test.V2
{
    public class TestPage : ComponentBase
    {
        [Inject] private IUSB? _usb { get; set; }
        [Inject] private ILogger<TestPage>? _logger { get; set; }
       
        private bool _initialized = false;

        /// <inheritdoc/>
        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            if (!this._initialized && this._usb != null)
            {
                this._usb.OnConnect += this.OnConnect;
                this._usb.OnDisconnect += this.OnDisconnect;
                this._initialized = true;
                return this._usb.Initialize();
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Ons the connect.
        /// </summary>
        /// <param name="device">The device.</param>
        private void OnConnect(USBDevice device)
        {
            if (this._logger != null)
            {
                this._logger.LogInformation("OnConnect:");
                this._logger.LogInformation(device.ToString());
            }            
        }

        /// <summary>
        /// Ons the disconnect.
        /// </summary>
        /// <param name="device">The device.</param>
        private void OnDisconnect(USBDevice device)
        {
            if (this._logger != null)
            {
                this._logger.LogInformation("OnDisconnect:");
                this._logger.LogInformation(device.ToString());
            }                
        }

        /// <summary>
        /// Requests the devices.
        /// </summary>
        /// <returns>A Task.</returns>
        protected async Task RequestDevices()
        {
            USBDevice? device = null;

            if (this._usb != null)
            {
                device = await this._usb.RequestDevice();
            }
            

            if (device != null)
            {
                device = await device.Open();
                device = await device.SelectConfiguration(device.Configuration);
                device = await device.ClaimInterface(0);
                if (this._logger != null)
                {
                    this._logger.LogInformation(device.ToString());

                    var inResult = await device.TransferIn(1, 3);
                    this._logger.LogInformation("Read response:");
                    this._logger.LogInformation(inResult.ToString());
                }                
            }
        }
    }
}