using nanoFramework.M2Mqtt.Messages;
using nanoFramework.M2Mqtt;
using nanoFramework.Networking;
using nanoFramework.Runtime.Native;

using System;
using System.Diagnostics;
using System.Threading;

using WeatherDisplay.Service;
using WeatherDisplay.Utils;
using System.IO;
using System.Text;


namespace WeatherDisplay
{
    public class Program
    {
        internal static DisplayManager _manager;
        static Thread _workingThread;
        const int _updateInterval = 60 * 1000;

        const string MQTT_server = "211.101.235.6";
        const string MQTT_client_id = "nano_2044";
        const string MQTT_topic = "22";

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
            MqttClient mqtt = new MqttClient(MQTT_server, 2883, false, null, null, MqttSslProtocols.None);
            mqtt.MqttMsgPublishReceived += Mqtt_MqttMsgPublishReceived;
            var ret = mqtt.Connect(MQTT_client_id, true);
            if (ret != MqttReasonCode.Success)
            {
                Debug.WriteLine($"ERROR connecting: {ret}");
                mqtt.Disconnect();
                return;
            }

            mqtt.Subscribe(new string[] { MQTT_topic }, new MqttQoSLevel[] { MqttQoSLevel.AtLeastOnce });
            while (true)
            {
                try
                {
                    if (_lastRequest != null)
                    {
                        int offset = 16;
                        StringBuilder sb = new StringBuilder();
                        foreach (var item in _lastRequest.result.forecasts)
                        {
                            int weather = 0;
                            switch (item.text_day)
                            {
                                case "“ı":
                                    weather = 1;
                                    break;
                                case "«Á":
                                case "∂‡‘∆":
                                    weather = 0;
                                    break;
                                case "”Í":
                                    weather = 2;
                                    break;
                            }

                            sb.Clear();
                            for (int i = 0; i < item.date.Length; i++)
                            {
                                if (i < 4)
                                    continue;
                                sb.Append(item.date[i]);
                            }
                            _manager.OLED.DrawTemp(offset, sb.ToString(), weather, item.high, item.low, true);
                            offset += 16;
                            if (offset > 63)
                            {
                                break;
                            }
                        }
                    }
                }
                catch { }
                Thread.Sleep(_updateInterval);
            }
        }

        private static Root _lastRequest = null;
        private static void Mqtt_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            if (e.Message != null)
            {
                using (var stream = new MemoryStream(e.Message))
                {
                    _lastRequest = nanoFramework.Json.JsonConvert.DeserializeObject(stream, typeof(Root)) as Root;
                }
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
