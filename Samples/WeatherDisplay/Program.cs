using nanoFramework.M2Mqtt.Messages;
using nanoFramework.M2Mqtt;
using nanoFramework.Networking;
using nanoFramework.Runtime.Native;

using System;
using System.Device.Wifi;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
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
            MqttClient mqtt = new MqttClient("211.101.235.6", 2883, false, null, null, MqttSslProtocols.None);
            mqtt.MqttMsgPublishReceived += Mqtt_MqttMsgPublishReceived;
            var ret = mqtt.Connect("nanoTestDevice", true);
            if (ret != MqttReasonCode.Success)
            {
                Debug.WriteLine($"ERROR connecting: {ret}");
                mqtt.Disconnect();
                return;
            }

            mqtt.Subscribe(new string[] { "22" }, new MqttQoSLevel[] { MqttQoSLevel.AtLeastOnce });
            while (true)
            {
                if (buffer != null)
                {
                    Console.WriteLine($"has buffer {buffer.Length}");
                }
                Thread.Sleep(_updateInterval);
            }
        }

        private static byte[] buffer = null;
        private static void Mqtt_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            if (e.Message != null)
            {
                buffer = e.Message;
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
