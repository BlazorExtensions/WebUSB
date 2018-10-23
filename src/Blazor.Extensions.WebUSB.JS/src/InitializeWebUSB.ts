import { USBManager } from "./USBManager";

namespace WebUSB {
    const blazorExtensions: string = 'BlazorExtensions';
    // define what this extension adds to the window object inside BlazorExtensions
    const extensionObject = {
        WebUSB: new USBManager()
    };

    export function initialize(): void {
        if (typeof window !== 'undefined' && !window[blazorExtensions]) {
            // when the library is loaded in a browser via a <script> element, make the
            // following APIs available in global scope for invocation from JS
            window[blazorExtensions] = {
                ...extensionObject
            };
        } else {
            window[blazorExtensions] = {
                ...window[blazorExtensions],
                ...extensionObject
            };
        }
    }
}

WebUSB.initialize();