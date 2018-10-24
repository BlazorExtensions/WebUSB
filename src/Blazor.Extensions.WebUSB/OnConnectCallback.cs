using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Blazor.Extensions.WebUSB
{
    public class OnConnectCallback : IDisposable
    {
        private const string REMOVE_ON_CONNECTED_METHOD = "BlazorExtensions.WebUSB.RemoveOnConnectHandler";
        private readonly Func<USBDevice, Task> _callback;
        public string Id { [JSInvokable(nameof(Id))]get; private set; }

        public OnConnectCallback(string id, Func<USBDevice, Task> callback)
        {
            this._callback = callback;
            this.Id = id;
        }

        [JSInvokable]
        public Task OnConnected(USBDevice device) => this._callback(device);

        public void Dispose() => JSRuntime.Current.InvokeAsync<object>(REMOVE_ON_CONNECTED_METHOD, new DotNetObjectRef(this)).GetAwaiter().GetResult();
    }
}