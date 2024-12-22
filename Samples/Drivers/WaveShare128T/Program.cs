using nanoFramework.Hardware.Esp32;
using nanoFramework.UI;
using nanoFramework.UI.GraphicDrivers;

using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;

namespace WaveShare128T
{
    public class Program
    {
        public const int ChipSelect = 9;
        public const int DataCommand = 8;
        public const int Reset = 14;

        public static void Main()
        {

            Configuration.SetPinFunction(10, DeviceFunction.SPI1_CLOCK);
            Configuration.SetPinFunction(11, DeviceFunction.SPI1_MOSI);
            Configuration.SetPinFunction(12, DeviceFunction.SPI1_MISO);

            GpioController ctl = new GpioController();

            var displaySpiConfig = new SpiConfiguration(
                1,
                ChipSelect,
                DataCommand,
                Reset,
                2);

            var screenConfig = new ScreenConfiguration(
                0,
                0,
                240,
                240,
                Gc9A01.GraphicDriver);

            var init = DisplayControl.Initialize(
                displaySpiConfig,
                screenConfig);

            ctl.OpenPin(2, PinMode.Output);
            ctl.Write(2, PinValue.High);

            ctl.OpenPin(DataCommand, PinMode.Output);

            DisplayControl.FullScreen.Clear();
            DisplayControl.FullScreen.DrawEllipse(System.Drawing.Color.Blue, 120, 120, 50, 50);
            DisplayControl.FullScreen.DrawLine(System.Drawing.Color.AliceBlue, 1, 0, 0, 120, 120);
            DisplayControl.FullScreen.MakeTransparent(System.Drawing.Color.Black);
            DisplayControl.FullScreen.Flush();

            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}
