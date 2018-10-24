import { USBDeviceFound, USBRequestDeviceOptions, ParseUSBDevice } from "./USBTypes";

type DotNetReferenceType = {
    invokeMethod<T>(methodIdentifier: string, ...args: any[]): T,
    invokeMethodAsync<T>(methodIdentifier: string, ...args: any[]): Promise<T>
}

export class USBManager {
    private usb: any = (<any>navigator).usb;
    private _onConnectCallbackMap: Map<string, (event) => Promise<{}>> = new Map<string, (event) => Promise<{}>>();

    public GetDevices = async (): Promise<USBDeviceFound[]> => {
        let devices = await this.usb.getDevices();
        let found: USBDeviceFound[] = [];
        devices.forEach(d => {
            found.push(ParseUSBDevice(d));
        });
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
                    console.log(d);
                    resolve(ParseUSBDevice(d));
                })
                .catch(err => reject(err));
        });
    }

    public AddOnConnectHandler = (callback: DotNetReferenceType) => {
        // TODO: Fix me
        const id = callback.invokeMethod<string>("Id");
        let localCallback = (event) => callback.invokeMethodAsync(event.device);
        this._onConnectCallbackMap.set(id, localCallback);
        this.usb.addEventListener("connect", localCallback);
    }

    public RemoveOnConnectHandler = (callback: DotNetReferenceType) => {
        const id = callback.invokeMethod<string>("Id");
        console.log("Removed " + id);
        // TODO: Fix me
        //Remove from callback collection
    }
}