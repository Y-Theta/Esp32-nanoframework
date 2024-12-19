using nanoFramework.Hardware.Esp32;
using nanoFramework.UI;
using nanoFramework.UI.GraphicDrivers;

using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

using TekuSP.Drivers.CST816D;

using Color = System.Drawing.Color;

namespace Gc9A01A
{
    public class Program
    {
        private const int cs = 9;
        private const int dc = 8;
        private const int rst = 14;
        private const int bl = 2;

        public static void Main()
        {
            Configuration.SetPinFunction(6, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(7, DeviceFunction.I2C1_CLOCK);

            Configuration.SetPinFunction(10, DeviceFunction.SPI1_CLOCK);
            Configuration.SetPinFunction(12, DeviceFunction.SPI1_MISO);
            Configuration.SetPinFunction(11, DeviceFunction.SPI1_MOSI);

            GpioController ctl = new GpioController();

            Thread t = new Thread(InitTouch);
            t.Start();

            Gc9A01.GraphicDriver.DefaultOrientation = DisplayOrientation.Portrait180;
            DisplayControl.Initialize(new SpiConfiguration(1, cs, dc, rst, -1), new ScreenConfiguration(0, 0, 240, 240, Gc9A01.GraphicDriver), 20 * 1024);

            ctl.OpenPin(bl, PinMode.Output);

            var blon = ctl.Read(bl);
            if (blon != PinValue.High)
            {
                ctl.Write(bl, PinValue.High);
            }

            DisplayControl.FullScreen.Clear();

            Thread.Sleep(Timeout.Infinite);
            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }

        private static void InitTouch()
        {
            CST816D d = new CST816D(1, 5, 13, 0x15);
            d.OnStateChanged += D_OnStateChanged;
            d.Start();
        }

        private static void D_OnStateChanged(object sender, TekuSP.Drivers.DriverBase.Interfaces.ITouchData data)
        {
            DisplayControl.FullScreen.Clear();
            DisplayControl.FullScreen.DrawRectangle(data.Y, data.X, 10, 10, 2, Color.Red);
            DisplayControl.FullScreen.Flush(data.Y, data.X, 10, 10);
        }
    }
}
