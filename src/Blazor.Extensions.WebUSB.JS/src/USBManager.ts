export class USBManager {
    private usb: any = (<any>navigator).usb;

    public GetDevices = () => {
        return this.usb.getDevices();
    }

    public RequestDevice = async (options: any) => {
        console.log(options);
        let device = await this.usb.requestDevice(options);
        console.log(device);
        let found = { vendorId: device.vendorId, productId: device.productId };
        return found;
    }
}