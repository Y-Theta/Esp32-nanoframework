using Iot.Device.EPaper;
using Iot.Device.Ssd13xx;

using System;
using System.Diagnostics;
using System.Threading;

namespace HzPrint
{
    public class Program
    {

        public static void Main()
        {
            Debug.WriteLine("program Started !");
            DisplayManager manager = new DisplayManager();

            WirelessAP.SetWifiAp();
            WebServer server = new WebServer();
            server.Start();
            manager.InitOled();

            manager.OLED.ClearScreen();
            //manager.OLED.DrawFilledRectangle(0, 0, 32, 15, true);
            manager.OLED.DrawBitmap(Icons.IconWifiOn, 0, 0, 16, false);
            manager.OLED.DrawBitmap(Icons.IconWifiOff, 16, 0, 16, false);
            manager.OLED.DrawString("192.168.120.222", 0, 16, true);
            manager.OLED.Display();

            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }

    }
}
