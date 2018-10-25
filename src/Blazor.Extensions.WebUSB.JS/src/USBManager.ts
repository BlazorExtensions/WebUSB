import { USBDeviceFound, USBRequestDeviceOptions, ParseUSBDevice, USBConfiguration, USBInterface, USBDirection, USBInTransferResult, USBOutTransferResult, USBTransferStatus } from "./USBTypes";

type DotNetReferenceType = {
    invokeMethod<T>(methodIdentifier: string, ...args: any[]): T,
    invokeMethodAsync<T>(methodIdentifier: string, ...args: any[]): Promise<T>
}

export class USBManager {
    private usb: any = (<any>navigator).usb; // The WebUSB API root object
    // All devices found on the last request
    // We keep a list of the most recent object because we can't serialize the "real" USBDevice to send back to C#
    // TODO: Find a better way to maintain it and keep consistent with the C# side...
    private _foundDevices: any[] = [];

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

    public CloseDevice = (device: USBDeviceFound): Promise<USBDeviceFound> => {
        let usbDevice = this.GetUSBDevice(device);
        return new Promise<USBDeviceFound>((resolve, reject) => {
            if (!usbDevice) return reject("Device not connected");
            usbDevice.close()
                .then(() => {
                    resolve(ParseUSBDevice(usbDevice));
                })
                .catch(err => reject(err));
        });
    }

    public ResetDevice = (device: USBDeviceFound): Promise<USBDeviceFound> => {
        let usbDevice = this.GetUSBDevice(device);
        return new Promise<USBDeviceFound>((resolve, reject) => {
            if (!usbDevice) return reject("Device not connected");
            usbDevice.reset()
                .then(() => {
                    resolve(ParseUSBDevice(usbDevice));
                })
                .catch(err => reject(err));
        });
    }

    public SelectConfiguration = (device: USBDeviceFound, configurationValue: number): Promise<USBDeviceFound> => {
        let usbDevice = this.GetUSBDevice(device);
        return new Promise<USBDeviceFound>((resolve, reject) => {
            if (!usbDevice) return reject("Device not connected");
            usbDevice.selectConfiguration(configurationValue)
                .then(() => {
                    resolve(ParseUSBDevice(usbDevice));
                })
                .catch(err => reject(err));
        });
    }

    public ClaimInterface = (device: USBDeviceFound, interfaceNumber: number): Promise<USBDeviceFound> => {
        let usbDevice = this.GetUSBDevice(device);
        return new Promise<USBDeviceFound>((resolve, reject) => {
            if (!usbDevice) return reject("Device not connected");

            usbDevice.claimInterface(interfaceNumber)
                .then(() => {
                    resolve(ParseUSBDevice(usbDevice));
                })
                .catch(err => reject(err));
        });
    }

    public ReleaseInterface = (device: USBDeviceFound, interfaceNumber: number): Promise<USBDeviceFound> => {
        let usbDevice = this.GetUSBDevice(device);
        return new Promise<USBDeviceFound>((resolve, reject) => {
            if (!usbDevice) return reject("Device not connected");

            usbDevice.releaseInterface(interfaceNumber)
                .then(() => {
                    resolve(ParseUSBDevice(usbDevice));
                })
                .catch(err => reject(err));
        });
    }

    public SelectAlternateInterface = (device: USBDeviceFound, interfaceNumber: number, alternateSetting: number): Promise<USBDeviceFound> => {
        let usbDevice = this.GetUSBDevice(device);
        return new Promise<USBDeviceFound>((resolve, reject) => {
            if (!usbDevice) return reject("Device not connected");

            usbDevice.selectAlternateInterface(interfaceNumber, alternateSetting)
                .then(() => {
                    resolve(ParseUSBDevice(usbDevice));
                })
                .catch(err => reject(err));
        });
    }

    public ClearHalt = (device: USBDeviceFound, direction: USBDirection, endpointNumber: number): Promise<USBDeviceFound> => {
        let usbDevice = this.GetUSBDevice(device);
        return new Promise<USBDeviceFound>((resolve, reject) => {
            if (!usbDevice) return reject("Device not connected");

            usbDevice.clearHalt(direction, endpointNumber)
                .then(() => {
                    resolve(ParseUSBDevice(usbDevice));
                })
                .catch(err => reject(err));
        });
    }

    public TransferIn = (device: USBDeviceFound, endpointNumber: number, length: number): Promise<USBInTransferResult> => {
        return new Promise<USBInTransferResult>((resolve, reject) => {
            reject("Not implemented");
        });
    }

    public TransferOut = (device: USBDeviceFound, endpointNumber: number, data: ArrayBuffer): Promise<USBOutTransferResult> => {
        let usbDevice = this.GetUSBDevice(device);
        return new Promise<USBOutTransferResult>((resolve, reject) => {
            if (!usbDevice) return reject("Device not connected");
            console.log(data);
            usbDevice.transferOut(endpointNumber, data)
                .then(out => {
                    console.log(out);
                    resolve({ bytesWritten: out.bytesWritten, status: out.status });
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