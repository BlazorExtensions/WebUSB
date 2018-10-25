import { USBDeviceFound, USBRequestDeviceOptions, ParseUSBDevice, USBConfiguration, USBInterface } from "./USBTypes";

type DotNetReferenceType = {
    invokeMethod<T>(methodIdentifier: string, ...args: any[]): T,
    invokeMethodAsync<T>(methodIdentifier: string, ...args: any[]): Promise<T>
}

export class USBManager {
    private usb: any = (<any>navigator).usb; // The WebUSB API root object
    private _foundDevices: any[] = []; // All devices found on the last request

    public GetDevices = async (): Promise<USBDeviceFound[]> => {
        let devices = await this.usb.getDevices();
        let found: USBDeviceFound[] = [];
        if (devices) {
            devices.forEach(d => {
                found.push(ParseUSBDevice(d));
                this._foundDevices.push(d);
            });
        }
        return found;
    }

    public RequestDevice = (options: USBRequestDeviceOptions): Promise<USBDeviceFound> => {
        function isEmpty(obj) {
            for (var prop in obj) {
                if (Object.prototype.hasOwnProperty.call(obj, prop)) {
                    return false;
                }
            }
            return true;
        }

        let filters: any[] = [];
        let reqOptions: any = undefined;

        if (options && options != null && options.filters && options.filters != null && options.filters.length > 0) {
            options.filters.forEach(f => {
                let filter: any = {};
                Object.keys(f).forEach(key => {
                    if (f[key] != null) {
                        filter[key] = f[key];
                    }
                });
                if (!isEmpty(filter)) {
                    filters.push(filter);
                }
            });

            if (filters.length > 0) {
                reqOptions = { filters: filters };
            }
        } else {
            reqOptions = { filters: [] };
        }

        return new Promise((resolve, reject) => {
            this.usb.requestDevice(reqOptions)
                .then(d => {
                    let usbDevice = ParseUSBDevice(d);
                    this._foundDevices.push(d);
                    resolve(usbDevice);
                })
                .catch(err => reject(err));
        });
    }

    public OpenDevice = (device: USBDeviceFound): Promise<USBDeviceFound> => {
        let usbDevice = this.GetUSBDevice(device);
        return new Promise<USBDeviceFound>((resolve, reject) => {
            if (!usbDevice) return reject("Device not connected");
            usbDevice.open()
                .then(() => {
                    resolve(ParseUSBDevice(usbDevice));
                })
                .catch(err => reject(err));
        });
    }

    public SelectConfiguration = (device: USBDeviceFound, config: USBConfiguration): Promise<USBDeviceFound> => {
        let usbDevice = this.GetUSBDevice(device);
        return new Promise<USBDeviceFound>((resolve, reject) => {
            if (!usbDevice) return reject("Device not connected");
            usbDevice.selectConfiguration(config.configurationValue)
                .then(() => {
                    resolve(ParseUSBDevice(usbDevice));
                })
                .catch(err => reject(err));
        });
    }

    public ClaimInterface = (device: USBDeviceFound, usbInterface: USBInterface): Promise<USBDeviceFound> => {
        let usbDevice = this.GetUSBDevice(device);
        return new Promise<USBDeviceFound>((resolve, reject) => {
            if (!usbDevice) return reject("Device not connected");

            usbDevice.claimInterface(usbInterface.interfaceNumber)
                .then(() => {
                    resolve(ParseUSBDevice(usbDevice));
                })
                .catch(err => reject(err));
        });
    }

    private GetUSBDevice = (device: USBDeviceFound): any => {
        return this._foundDevices.find(
            d => d.vendorId == device.vendorId &&
                d.productId == device.productId &&
                d.deviceClass == device.deviceClass &&
                d.serialNumber == device.serialNumber);
    }

    private FireUSBDeviceEvent = (event: string, eventPayload: any, usb: DotNetReferenceType) => {
        let device = ParseUSBDevice(eventPayload.device);
        return usb.invokeMethodAsync(event, device);
    }

    public RegisterUSBEvents = (usb: DotNetReferenceType) => {
        //TODO: Check why this event is not consistently being fired.
        this.usb.addEventListener("connect", (event) => this.FireUSBDeviceEvent("OnConnect", event, usb));
        this.usb.addEventListener("disconnect", (event) => this.FireUSBDeviceEvent("OnDisconnect", event, usb));
    }

    public RemoveUSBEvents = (usb: DotNetReferenceType) => {
        this.usb.removeEventListener("connect", (event) => this.FireUSBDeviceEvent("OnConnect", event, usb));
        this.usb.removeEventListener("disconnect", (event) => this.FireUSBDeviceEvent("OnDisconnect", event, usb));
    }
}