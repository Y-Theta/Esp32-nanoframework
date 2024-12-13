using Iot.Device.Ssd13xx;

using nanoFramework.Hardware.Esp32;

using NF_Hanz;

using System;
using System.Device.I2c;
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

            I2cDevice device = I2cDevice.Create(new I2cConnectionSettings(1, 0x3c, I2cBusSpeed.FastMode));
            Ssd1306 oled = new Ssd1306(device,Ssd13xx.DisplayResolution.OLED128x64);
            oled.Font = new hzFont();
            oled.ClearScreen();

            while (true)
            {
                oled.DrawString(0, 0, "我", 2);
                oled.Display();
                Thread.Sleep(1000);
            }

            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
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
