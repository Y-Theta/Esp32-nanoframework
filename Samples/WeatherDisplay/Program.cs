using System;
using System.Diagnostics;
using System.Threading;

using WeatherDisplay.Service;
using WeatherDisplay.Utils;

namespace WeatherDisplay
{
    public class Program
    {
        static readonly object _displayLock = new object();
        static DisplayManager _manager;
        static Thread _displayThread;
        const int _updateInterval = 60 * 1000;

        public static void Main()
        {
            Console.WriteLine("Weather Display Start !");

            _manager = new DisplayManager();
            _manager.InitOled();

            string ipaddress = "x.x.x.x";
            int mode = -1;
            try
            {
                var address = Wireless80211.GetCurrentIPAddress();
                if (address != "0.0.0.0")
                {
                    ipaddress = address;
                    mode = 1;
                }
                else
                {
                    mode = 0;
                    var apAddress = WirelessAP.GetIP();
                    ipaddress = apAddress;

                    WirelessAP.SetWifiAp();
                    WebServer server = new WebServer();
                    server.Start();
                }
            }
            catch { }

            _manager.OLED.DrawAppTitle(mode > 0, mode, ipaddress);

            Thread.Sleep(Timeout.Infinite);
        }

        private static void AppRun()
        {

        }

        private static void EnterSettingMode()
        {

        }

        private static void EnterWorkingMode()
        {

        }
    }
}
