using nanoFramework.Networking;
using nanoFramework.Runtime.Native;

using System;
using System.Device.Wifi;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;

using WeatherDisplay.Service;
using WeatherDisplay.Utils;

using GC = nanoFramework.Runtime.Native.GC;

namespace WeatherDisplay
{
    public class Program
    {
        internal static DisplayManager _manager;
        static Thread _workingThread;
        const int _updateInterval = 5 * 1000;
        static HttpClient client;

        public static void Main()
        {
            Console.WriteLine("Weather Display Start !");

            _manager = new DisplayManager();
            _manager.InitOled();
            client = new HttpClient();
            client.SslVerification = System.Net.Security.SslVerification.NoVerification;
            client.Timeout = TimeSpan.FromSeconds(10);

            string ipaddress = "x.x.x.x";
            int mode = -1;
            try
            {
                var connectEnabled = Wireless80211.IsEnabled();
                if (connectEnabled)
                {
                    WifiNetworkHelper.Reconnect(true, token: new CancellationTokenSource(1000).Token);
                    for (int i = 0; i < 10; i++)
                    {
                        ipaddress = Wireless80211.GetCurrentIPAddress();
                        if (ipaddress == "0.0.0.0")
                        {
                            Thread.Sleep(100);
                        }
                        else
                        {
                            mode = 1;
                            _workingThread = new Thread(AppRun);
                            _workingThread.Start();
                            break;
                        }
                    }
                }

                if (mode < 0)
                {
                    mode = 0;
                    EnterSettingMode();
                    var apAddress = WirelessAP.GetIP();
                    ipaddress = apAddress;
                }
            }
            catch
            {
                Power.RebootDevice();
                return;
            }

            _manager.OLED.DrawAppTitle(mode > 0, mode, ipaddress);
            Thread.Sleep(Timeout.Infinite);
        }

        private static void AppRun()
        {
            while (true)
            {
                var response = client.Get("http://example.com");
                //var stream = response.Content.ReadAsString();
                response.Dispose();
                response = null;
                Debug.WriteLine("1");

                Thread.Sleep(_updateInterval);
            }
        }

        private static void EnterSettingMode()
        {
            WirelessAP.SetWifiAp();
            WebServer server = new WebServer();
            server.Start();
        }
    }
}
