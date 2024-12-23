using nanoFramework.Runtime.Native;

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
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
                var connectEnabled = Wireless80211.IsEnabled();
                if (connectEnabled)
                {
                    Wireless80211.ConnectOrSetAp();
                    for (int i = 0; i < 10; i++)
                    {
                        ipaddress = Wireless80211.GetCurrentIPAddress();
                        if (ipaddress == "0.0.0.0")
                        {
                            Thread.Sleep(100);
                        }
                        else
                        {
                            break;
                        }
                    }
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

            //if (mode == 1)
            //{
            //    Wireless80211.Disable();
            //    Power.RebootDevice();
            //}

            WebRequest request = WebRequest.Create(ipaddress);
            request.Method = "GET";

            while (true)
            {

                Thread.Sleep(_updateInterval);
            }
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
