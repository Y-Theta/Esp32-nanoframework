using nanoFramework.Hardware.Esp32;

using System;
using System.Diagnostics;
using System.Threading;

using TekuSP.Drivers.CST816D;

namespace Gc9A01A
{
    public class Program
    {
        public static void Main()
        {
            Configuration.SetPinFunction(6, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(7, DeviceFunction.I2C1_CLOCK);

            Configuration.SetPinFunction(10, DeviceFunction.SPI1_CLOCK);
            Configuration.SetPinFunction(12, DeviceFunction.SPI1_MISO);
            Configuration.SetPinFunction(11, DeviceFunction.SPI1_MOSI);

            CST816D d = new CST816D(1, 5, 13, 0x15);
            d.OnStateChanged += D_OnStateChanged;
            d.Start();


            Gc9A01ADriver dr = new Gc9A01ADriver("GC9a01a", 1, 9, 14, 8, new Gc9A01AConfig { BackLightPin = 2 });
            dr.Start();
            dr.ClearScreen(Color.GC9A01A_PURPLE);
            for (int i = 0; i < 100; i++)
            {
                dr.DrawPixel((byte)(80 + i), 100, (UInt16)Color.GC9A01A_RED);
                dr.DrawPixel((byte)(80 + i), 101, (UInt16)Color.GC9A01A_RED);
                dr.DrawPixel((byte)(80 + i), 102, (UInt16)Color.GC9A01A_RED);
            }

            Thread.Sleep(Timeout.Infinite);
            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }

        private static void D_OnStateChanged(object sender, TekuSP.Drivers.DriverBase.Interfaces.ITouchData data)
        {
            Console.WriteLine($"{data.X}-{data.Y}");
        }
    }
}
