import { USBDeviceFound, USBRequestDeviceOptions, ParseUSBDevice, USBConfiguration, USBInterface, USBDirection, USBInTransferResult, USBOutTransferResult, USBTransferStatus, USBControlTransferParameters } from "./USBTypes";

type DotNetReferenceType = {
	invokeMethod<T>(methodIdentifier: string, ...args: any[]): T,
	invokeMethodAsync<T>(methodIdentifier: string, ...args: any[]): Promise<T>
}

export class USBManager {
	private usb: any = (<any>navigator).usb; // The WebUSB API root object
	private _usbReference: DotNetReferenceType | undefined = undefined;
	// All devices found on the last request
	// We keep a list of the most recent object because we can't serialize the "real" USBDevice to send back to C#
	// TODO: Find a better way to maintain it and keep consistent with the C# side...
	private _foundDevices: any[] = [];
	private _eventsRegistered: boolean = false;

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

	public RequestDevice = async (options: USBRequestDeviceOptions): Promise<USBDeviceFound> => {
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

		let authorizedDevice = await this.usb.requestDevice(reqOptions);
		let usbDevice = ParseUSBDevice(authorizedDevice);
		this._foundDevices.push(authorizedDevice);
		return usbDevice;
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
		let usbDevice = this.GetUSBDevice(device);
		return new Promise<USBInTransferResult>((resolve, reject) => {
			if (!usbDevice) return reject("Device not connected");

			usbDevice.transferIn(endpointNumber, length)
				.then(out => {
					resolve({
						data: Array.prototype.slice.call(new Uint8Array(out.data.buffer)), // Hack to support Uint8Array to byte[] serialization.
						status: out.status
					});
				})
				.catch(err => {
					console.error(err);
					reject(err);
				});
		});
	}

	public TransferOut = (device: USBDeviceFound, endpointNumber: number, data: any): Promise<USBOutTransferResult> => {
		let usbDevice = this.GetUSBDevice(device);
		return new Promise<USBOutTransferResult>((resolve, reject) => {
			if (!usbDevice) return reject("Device not connected");
			const buffer = Uint8Array.from(data);

			usbDevice.transferOut(endpointNumber, buffer)
				.then(out => {
					resolve({
						bytesWritten: out.bytesWritten,
						status: out.status
					});
				})
				.catch(err => {
					console.error(err);
					reject(err);
				});
		});
	}

	public ControlTransferIn = (device: USBDeviceFound, setup: USBControlTransferParameters, length: number): Promise<USBInTransferResult> => {
		let usbDevice = this.GetUSBDevice(device);
		return new Promise<USBInTransferResult>((resolve, reject) => {
			if (!usbDevice) return reject("Device not connected");

			usbDevice.ControlTransferIn(setup, length)
				.then(out => {
					resolve({
						data: Array.prototype.slice.call(new Uint8Array(out.data.buffer)), // Hack to support Uint8Array to byte[] serialization.
						status: out.status
					});
				})
				.catch(err => {
					console.error(err);
					reject(err);
				});
		});
	}

	public ControlTransferOut = (device: USBDeviceFound, setup: USBControlTransferParameters, data: any): Promise<USBOutTransferResult> => {
		let usbDevice = this.GetUSBDevice(device);
		return new Promise<USBOutTransferResult>((resolve, reject) => {
			if (!usbDevice) return reject("Device not connected");
			const buffer = data ? Uint8Array.from(data) : undefined;

			usbDevice.controlTransferOut(setup, buffer)
				.then(out => {
					resolve({
						bytesWritten: out.bytesWritten,
						status: out.status
					});
				})
				.catch(err => {
					console.error(err);
					reject(err);
				});
		});
	}

	private GetUSBDevice = (device: USBDeviceFound): any => {
		return this._foundDevices.find(
			d => d.vendorId == device.vendorId &&
				d.productId == device.productId &&
				d.deviceClass == device.deviceClass &&
				d.serialNumber == device.serialNumber);
	}

	private ConnectionStateChangedCallback = (event: any) => {
		if (!this._usbReference) return;
		let method: string = "";

		let usbDevice = this.GetUSBDevice(event.device);

		if (event.type == "disconnect") {
			method = "OnDisconnect";
			this._foundDevices = this._foundDevices.filter((d, index, arr) => {
				return d.vendorId != usbDevice.vendorId &&
					d.productId != usbDevice.productId &&
					d.deviceClass != usbDevice.deviceClass &&
					d.serialNumber != usbDevice.serialNumber;
			});
		}
		else if (event.type == "connect") {
			method = "OnConnect";
		}
		else {
			console.warn(event);
			return;
		}

		this._usbReference.invokeMethodAsync(method, ParseUSBDevice(event.device));
	}

	public RegisterUSBEvents = (usb: DotNetReferenceType) => {
		this._usbReference = usb;
		//TODO: Check why this event is not consistently being fired.
		if (!this._eventsRegistered) {
			this.usb.addEventListener("connect", this.ConnectionStateChangedCallback);
			this.usb.addEventListener("disconnect", this.ConnectionStateChangedCallback);
			this._eventsRegistered = true;
		}
	}

	public RemoveUSBEvents = (usb: DotNetReferenceType) => {
		if (this._eventsRegistered) {
			this.usb.removeEventListener("connect", this.ConnectionStateChangedCallback);
			this.usb.removeEventListener("disconnect", this.ConnectionStateChangedCallback);
		}
	}
}