using Iot.Device.EPaper.Enums;
using Iot.Device.EPaper;
using Iot.Device.Ssd13xx;

using nanoFramework.Hardware.Esp32;

using NF_Hanz;

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Diagnostics;
using System.Resources;
using System.Text;
using System.Threading;


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
            using var gfx = new Graphics(display)
            {
                DisplayRotation = Rotation.Degrees90Clockwise
            };
            display.Clear();
            display.PerformFullRefresh();

            I2cDevice device = I2cDevice.Create(new I2cConnectionSettings(1, 0x3c, I2cBusSpeed.FastMode));
            Ssd1306 oled = new Ssd1306(device,Ssd13xx.DisplayResolution.OLED128x64);
            oled.Font = new hzFont();
            oled.ClearScreen();

            while (true)
            {
                oled.DrawString(0, 0, "一二三", 1);
                oled.Display();

                display.Clear();
                DrawString("我喜欢宝宝", oled.Font, gfx);
                display.Flush();
                display.PerformFullRefresh();
                Thread.Sleep(1000);
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
                            gfx.DrawPixel(col, row, System.Drawing.Color.Black);
                        }
                        col += 1;
                    }
                    i++;
                }

                row = 0;
                initc += 16;
            }

        }

   
        public class hzFont : IFont
        {
            public override byte Height => 16;
            public override byte Width => 16;

            public override byte[] this[char character]
            {
                get
                {
                    try
                    {
                        var bytes = UTF8Encoding.UTF8.GetBytes(character.ToString());
                        byte[] buffer = new byte[32];
                        var gb = ChineseHelper.Utf2Gb2312(bytes);
                        var pts = ChineseHelper.GetHanzPoint(gb);
                        for (int i = 0; i < 32; i++)
                        {
                            buffer[i] = ChineseHelper.ReverseByte(pts[i]);
                        }
                        return buffer;
                        //var hanz = Resources.GetBytes(BinaryResources.hzk16h);
                        //for (int i = 0; i < 32; i++)
                        //{
                        //    buffer[i] = hanz[offset + i];
                        //}
                    }
                    catch { }

                    return new byte[0];
                }
            }
        }
    }
}
