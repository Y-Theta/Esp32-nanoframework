using Iot.Device.EPaper.Enums;
using Iot.Device.EPaper;

using nanoFramework.Hardware.Esp32;

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.Resources;
using System.Text;
using System.Threading;
using nanoFramework.UI;
using Iot.Device.Ssd13xx;
using System.Device.I2c;
using IFont = nanoFramework.UI.IFont;


namespace HzPrint
{
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
            Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);

            Configuration.SetPinFunction(15, DeviceFunction.SPI1_CLOCK);
            Configuration.SetPinFunction(2, DeviceFunction.SPI1_MOSI);

            using var gpioController = new GpioController(PinNumberingScheme.Logical);
            // Setup SPI connection with the display
            var spiConnectionSettings = new SpiConnectionSettings(busId: 1, chipSelectLine: 18)
            {
                ClockFrequency = 20000000,
                Mode = SpiMode.Mode1,
                ChipSelectLineActiveState = false,
                Configuration = SpiBusConfiguration.HalfDuplex,
                DataFlow = DataFlow.MsbFirst,
            };

            I2cDevice i2cDev = new I2cDevice(new I2cConnectionSettings(1, 0x3c));
            using var oled = new Ssd1306(i2cDev, Ssd13xx.DisplayResolution.OLED128x64);
            oled.ClearScreen();
            using var spiDevice = new SpiDevice(spiConnectionSettings);
            // Create an instance of the display driver
            using var display = new Ssd1681_154D67(
                spiDevice,
                resetPin: 4,
                busyPin: 19,
                dataCommandPin: 5,
                width: 200,
                height: 200,
                gpioController,
                enableFramePaging: false,
                shouldDispose: false);

            display.PowerOn();
            display.Initialize();
            using Graphics gfx = new Graphics(display);
            gfx.EPaperDisplay.FrameBuffer.Fill(System.Drawing.Color.White);
            gfx.EPaperDisplay.Flush();
            display.PerformFullRefresh();

            oled.DrawFilledRectangle(0, 0, 128, 15, true);
            oled.Display();

            while (true)
            {
                //Thread.Sleep(200);
            }

            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }

        public static void DrawString(string str, IFont font ,Graphics gfx)
        {
            int initc = 0;
            int col = 0;
            int row = 0;

            foreach (char c in str)
            {
                var bytt = font[c];
                for (int i = 0; i < bytt.Length;)
                {
                    if (i % 2 == 0)
                    {
                        row += 1;
                        col = initc;
                    }

                    var item = bytt[i];
                    for (int x = 0; x < 8; x++)
                    {
                        var bit = (item >> x) & 0b1;
                        if (bit > 0)
                        {
                            gfx.EPaperDisplay.FrameBuffer.SetPixel(new System.Drawing.Point(col, row), System.Drawing.Color.Black);
                        }
                        col += 1;
                    }
                    i++;
                }

                row = 0;
                initc += 16;
            }

        }

    }
}
