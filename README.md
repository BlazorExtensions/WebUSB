# WebUSB
HTML5 WebUSB API implementation for Microsoft Blazor

[![Build status](https://dotnet-ci.visualstudio.com/DotnetCI/_apis/build/status/Blazor-Extensions-WebUSB-CI?branch=master)](https://dotnet-ci.visualstudio.com/DotnetCI/_build/latest?definitionId=18&branch=master)
[![Package Version](https://img.shields.io/nuget/v/Blazor.Extensions.WebUSB.svg)](https://www.nuget.org/packages/Blazor.Extensions.WebUSB)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Blazor.Extensions.WebUSB.svg)](https://www.nuget.org/packages/Blazor.Extensions.WebUSB)
[![License](https://img.shields.io/github/license/BlazorExtensions/WebUSB.svg)](https://github.com/BlazorExtensions/WebUSB/blob/master/LICENSE)

# Blazor Extensions

Blazor Extensions are a set of packages with the goal of adding useful things to [Blazor](https://blazor.net).

# Blazor Extensions WebUSB

This package wraps [HTML5 WebUSB](https://wicg.github.io/webusb/) APIs. 

# Installation

```
Install-Package Blazor.Extensions.WebUSB
```

# Sample

## Usage

- First add the USB services on Blazor `IServiceCollection`:

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.UseWebUSB(); // Makes IUSB available to the DI container
}
```

### To consume on your `.cshtml`:

- On your `_ViewImports.cshtml` add the using entry:

```c#
@using Blazor.Extensions.WebUSB
```

- Then, on your `.cshtml` inject the `IUSB`:

```c#
@inject IUSB usb
```

And then use the `usb` object to interact with connected USB devices thru your Blazor application.

### To inject on a `BlazorComponent` class:

Define a property of type `IUSB` and mark it as `[Injectable]`:

```c#
[Inject] private IUSB _usb { get; set; }
```

Then use the `_usb` variable to interact with the connected USB devices.

**Note**: For now, you have to call `await IUSB.Initialize()` once in your application. This is a temporary requirement and we are looking on a better way to automatically register to _Connect/Disconnect_ events.

# Contributions and feedback

Please feel free to use the component, open issues, fix bugs or provide feedback.

# Contributors

The following people are the maintainers of the Blazor Extensions projects:

- [Attila Hajdrik](https://github.com/attilah)
- [Gutemberg Ribiero](https://github.com/galvesribeiro)
