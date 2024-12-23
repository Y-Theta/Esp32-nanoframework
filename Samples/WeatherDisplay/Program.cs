using System;
using System.Diagnostics;
using System.Threading;

using WeatherDisplay.Service;

namespace WeatherDisplay
{
    public class Program
    {
        static readonly object _displayLock = new object();
        static DisplayManager _manager;
        static Thread _displayThread;

        public static void Main()
        {
            Console.WriteLine("Weather Display Start !");

            _manager = new DisplayManager();
            _manager.InitOled();

            WirelessAP.SetWifiAp();
            WebServer server = new WebServer();
            server.Start();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
