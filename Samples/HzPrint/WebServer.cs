using Iot.Device.DhcpServer;

using nanoFramework.Networking;
using nanoFramework.Runtime.Native;

using System;
using System.Collections;
using System.Device.Wifi;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace HzPrint
{
    public class WebServer
    {
        HttpListener _listener;
        Thread _serverThread;

        public void Start()
        {
            if (_listener == null)
            {
                _listener = new HttpListener("http");
                _serverThread = new Thread(RunServer);
                _serverThread.Start();
            }
        }

        public void Stop()
        {
            if (_listener != null)
                _listener.Stop();
        }
        private void RunServer()
        {
            _listener.Start();

            while (_listener.IsListening)
            {
                var context = _listener.GetContext();
                if (context != null)
                    ProcessRequest(context);
            }
            _listener.Close();

            _listener = null;
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;
            string responseString;
            string ssid = null;
            string password = null;
            bool isApSet = false;

            switch (request.HttpMethod)
            {
                case "GET":
                    string[] url = request.RawUrl.Split('?');
                    if (url[0] == "/favicon.ico")
                    {
                    }
                    else
                    {
                        response.ContentType = "text/html";
                        OutPutResponse(response, CreateMainPage("msg"));
                    }
                    break;

                case "POST":
                    // Pick up POST parameters from Input Stream
                    Hashtable hashPars = ParseParamsFromStream(request.InputStream);
                    ssid = (string)hashPars["ssid"];
                    password = (string)hashPars["password"];

                    Debug.WriteLine($"Wireless parameters SSID:{ssid} PASSWORD:{password}");

                    string message = "<p>New settings saved.</p><p>Rebooting device to put into normal mode</p>";

                    bool res = Wireless80211.Configure(ssid, password);
                    if (res)
                    {
                        message += $"<p>And your new IP address should be {Wireless80211.GetCurrentIPAddress()}.</p>";
                    }

                    responseString = CreateMainPage(message);

                    OutPutResponse(response, responseString);
                    isApSet = true;
                    break;
            }

            response.Close();

            if (isApSet && (!string.IsNullOrEmpty(ssid)) && (!string.IsNullOrEmpty(password)))
            {
                // Enable the Wireless station interface
                Wireless80211.Configure(ssid, password);

                // Disable the Soft AP
                WirelessAP.Disable();
                Thread.Sleep(200);
                Power.RebootDevice();
            }
        }

        static string ReplaceMessage(string page, string message)
        {
            int index = page.IndexOf("{message}");
            if (index >= 0)
            {
                return page.Substring(0, index) + message + page.Substring(index + 9);
            }

            return page;
        }

        static void OutPutResponse(HttpListenerResponse response, string responseString)
        {
            var responseBytes = System.Text.Encoding.UTF8.GetBytes(responseString);
            OutPutByteResponse(response, System.Text.Encoding.UTF8.GetBytes(responseString));
        }
        static void OutPutByteResponse(HttpListenerResponse response, Byte[] responseBytes)
        {
            response.ContentLength64 = responseBytes.Length;
            response.OutputStream.Write(responseBytes, 0, responseBytes.Length);

        }

        static Hashtable ParseParamsFromStream(Stream inputStream)
        {
            byte[] buffer = new byte[inputStream.Length];
            inputStream.Read(buffer, 0, (int)inputStream.Length);

            return ParseParams(System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length));
        }

        static Hashtable ParseParams(string rawParams)
        {
            Hashtable hash = new Hashtable();

            string[] parPairs = rawParams.Split('&');
            foreach (string pair in parPairs)
            {
                string[] nameValue = pair.Split('=');
                hash.Add(nameValue[0], nameValue[1]);
            }

            return hash;
        }

        static string CreateMainPage(string message)
        {
            return "<!doctypehtml><html lang=en><meta charset=UTF-8><meta content=\"width=device-width,initial-scale=1\"name=viewport><style>:root{--first-color:#1A73E8;--input-color:#80868B;--border-color:#DADCE0;--body-font:\"Roboto\",sans-serif;--normal-font-size:1rem;--small-font-size:.75rem}*,::after,::before{box-sizing:border-box}body{margin:0;padding:0;font-family:var(--body-font);font-size:var(--normal-font-size)}h1{margin:0}.l-form{display:flex;justify-content:center;align-items:center;height:100vh}.form{width:360px;padding:3rem 2rem;border-radius:1rem;box-shadow:0 10px 25px rgba(92,99,105,.2)}.form__title{font-weight:500;margin-bottom:2.5rem}.form__div{position:relative;height:52px;margin-bottom:1.5rem}.form__input{position:absolute;top:0;left:0;width:100%;height:100%;font-size:var(--normal-font-size);border:2px solid var(--border-color);border-radius:.5rem;outline:0;padding:1rem;background:0 0;z-index:1}.form__label{position:absolute;left:1rem;top:1rem;padding:0 .25rem;background-color:#fff;color:var(--input-color);font-size:var(--normal-font-size);transition:.3s}.form__button{display:block;margin-left:auto;padding:.75rem 2rem;outline:0;border:none;background-color:var(--first-color);color:#fff;font-size:var(--normal-font-size);border-radius:.25rem;cursor:pointer;transition:.3s}.form__button:hover{box-shadow:0 10px 36px rgba(0,0,0,.15)}.form__input:focus+.form__label{top:-.5rem;left:.8rem;color:var(--first-color);font-size:var(--small-font-size);font-weight:500;z-index:10}.form__input:not(:placeholder-shown).form__input:not(:focus)+.form__label{top:-.5rem;left:.8rem;z-index:10;font-size:var(--small-font-size);font-weight:500}.form__input:focus{border:2px solid var(--first-color)}</style><title>nanoframework</title><div class=l-form><form action=\"\"class=form><h1 class=form__title>Sign In</h1><div class=form__div><input class=form__input placeholder=\" \"> <label class=form__label for=\"\">Ssid</label></div><div class=form__div><input class=form__input placeholder=\" \"type=password> <label class=form__label for=\"\">Password</label></div><input class=form__button type=submit value=\"Sign In\"></form></div>";
        }
    }

    class Wireless80211
    {
        /// <summary>
        /// Checks if the wireless 802.11 interface is enabled.
        /// </summary>
        /// <returns>
        /// Returns true if the wireless 802.11 interface is enabled (i.e., the SSID is not null or empty), 
        /// otherwise returns false.
        /// </returns>
        public static bool IsEnabled()
        {
            Wireless80211Configuration wconf = GetConfiguration();
            return !string.IsNullOrEmpty(wconf.Ssid);
        }

        /// <summary>
        /// Get current IP address. Only valid if successfully provisioned and connected
        /// </summary>
        /// <returns>IP address string</returns>
        public static string GetCurrentIPAddress()
        {
            NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[0];

            // get first NI ( Wifi on ESP32 )
            return ni.IPv4Address.ToString();
        }

        /// <summary>
        /// Coonnects to the Wifi or sets the Access Point mode.
        /// </summary>
        /// <returns>True if access point is setup.</returns>
        public static bool ConnectOrSetAp()
        {
            if (IsEnabled())
            {
                Debug.WriteLine("Wireless client activated");
                if (!WifiNetworkHelper.Reconnect(true, token: new CancellationTokenSource(30_000).Token))
                {
                    WirelessAP.SetWifiAp();
                    return true;
                }
            }
            else
            {
                WirelessAP.SetWifiAp();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Disable the Wireless station interface.
        /// </summary>
        public static void Disable()
        {
            Wireless80211Configuration wconf = GetConfiguration();
            wconf.Options = Wireless80211Configuration.ConfigurationOptions.None | Wireless80211Configuration.ConfigurationOptions.SmartConfig;
            wconf.SaveConfiguration();
        }

        /// <summary>
        /// Configure and enable the Wireless station interface
        /// </summary>
        /// <param name="ssid"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool Configure(string ssid, string password)
        {
            // Make sure we are disconnected before we start connecting otherwise
            // ConnectDhcp will just return success instead of reconnecting.
            WifiAdapter wa = WifiAdapter.FindAllAdapters()[0];
            wa.Disconnect();

            CancellationTokenSource cs = new(30_000);
            Console.WriteLine("ConnectDHCP");
            WifiNetworkHelper.Disconnect();

            // Reconfigure properly the normal wifi
            Wireless80211Configuration wconf = GetConfiguration();
            wconf.Options = Wireless80211Configuration.ConfigurationOptions.AutoConnect | Wireless80211Configuration.ConfigurationOptions.Enable;
            wconf.Ssid = ssid;
            wconf.Password = password;
            wconf.SaveConfiguration();

            WifiNetworkHelper.Disconnect();
            bool success;

            success = WifiNetworkHelper.ConnectDhcp(ssid, password, WifiReconnectionKind.Automatic, true, token: cs.Token);

            if (!success)
            {
                wa.Disconnect();
                // Bug in network helper, we've most likely try to connect before, let's make it manual
                var res = wa.Connect(ssid, WifiReconnectionKind.Automatic, password);
                success = res.ConnectionStatus == WifiConnectionStatus.Success;
                Console.WriteLine($"Connected: {res.ConnectionStatus}");
            }

            return success;
        }

        /// <summary>
        /// Get the Wireless station configuration.
        /// </summary>
        /// <returns>Wireless80211Configuration object</returns>
        public static Wireless80211Configuration GetConfiguration()
        {
            NetworkInterface ni = GetInterface();
            return Wireless80211Configuration.GetAllWireless80211Configurations()[ni.SpecificConfigId];
        }

        public static NetworkInterface GetInterface()
        {
            NetworkInterface[] Interfaces = NetworkInterface.GetAllNetworkInterfaces();

            // Find WirelessAP interface
            foreach (NetworkInterface ni in Interfaces)
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    return ni;
                }
            }
            return null;
        }
    }

    public static class WirelessAP
    {
        /// <summary>
        /// Gets or sets the IP address of the Soft AP.
        /// </summary>
        public static string SoftApIP { get; set; } = "192.168.4.1";

        /// <summary>
        /// Gets or sets the SSID of the Soft AP.
        /// </summary>
        public static string SoftApSsid { get; set; } = "MySuperSSID";

        /// <summary>
        /// Sets the configuration for the wireless access point.
        /// </summary>
        public static void SetWifiAp()
        {
            Wireless80211.Disable();
            if (Setup() == false)
            {
                // Reboot device to Activate Access Point on restart
                Console.WriteLine($"Setup Soft AP, Rebooting device");
                Power.RebootDevice();
            }

            var dhcpserver = new DhcpServer
            {
                CaptivePortalUrl = $"http://{SoftApIP}"
            };
            var dhcpInitResult = dhcpserver.Start(IPAddress.Parse(SoftApIP), new IPAddress(new byte[] { 255, 255, 255, 0 }));
            if (!dhcpInitResult)
            {
                Console.WriteLine($"Error initializing DHCP server.");
                // This happens after a very freshly flashed device
                Power.RebootDevice();
            }

            Console.WriteLine($"Running Soft AP, waiting for client to connect");
            Console.WriteLine($"Soft AP IP address :{GetIP()}");
        }

        /// <summary>
        /// Disable the Soft AP for next restart.
        /// </summary>
        public static void Disable()
        {
            WirelessAPConfiguration wapconf = GetConfiguration();
            wapconf.Options = WirelessAPConfiguration.ConfigurationOptions.None;
            wapconf.SaveConfiguration();
        }

        /// <summary>
        /// Set-up the Wireless AP settings, enable and save
        /// </summary>
        /// <returns>True if already set-up</returns>
        public static bool Setup()
        {
            NetworkInterface ni = GetInterface();
            WirelessAPConfiguration wapconf = GetConfiguration();

            // Check if already Enabled and return true
            if (wapconf.Options == (WirelessAPConfiguration.ConfigurationOptions.Enable |
                                    WirelessAPConfiguration.ConfigurationOptions.AutoStart) &&
                ni.IPv4Address == SoftApIP)
            {
                return true;
            }

            // Set up IP address for Soft AP
            ni.EnableStaticIPv4(SoftApIP, "255.255.255.0", SoftApIP);

            // Set Options for Network Interface
            //
            // Enable    - Enable the Soft AP ( Disable to reduce power )
            // AutoStart - Start Soft AP when system boots.
            // HiddenSSID- Hide the SSID
            //
            wapconf.Options = WirelessAPConfiguration.ConfigurationOptions.AutoStart |
                            WirelessAPConfiguration.ConfigurationOptions.Enable;

            // Set the SSID for Access Point. If not set will use default  "nano_xxxxxx"
            wapconf.Ssid = SoftApSsid;

            // Maximum number of simultaneous connections, reserves memory for connections
            wapconf.MaxConnections = 1;

            // To set-up Access point with no Authentication
            wapconf.Authentication = System.Net.NetworkInformation.AuthenticationType.Open;
            wapconf.Password = "";

            // To set up Access point with no Authentication. Password minimum 8 chars.
            //wapconf.Authentication = AuthenticationType.WPA2;
            //wapconf.Password = "password";

            // Save the configuration so on restart Access point will be running.
            wapconf.SaveConfiguration();

            return false;
        }

        /// <summary>
        /// Find the Wireless AP configuration
        /// </summary>
        /// <returns>Wireless AP configuration or NUll if not available</returns>
        public static WirelessAPConfiguration GetConfiguration()
        {
            NetworkInterface ni = GetInterface();
            return WirelessAPConfiguration.GetAllWirelessAPConfigurations()[ni.SpecificConfigId];
        }

        /// <summary>
        /// Gets the network interface for the wireless access point.
        /// </summary>
        /// <returns>The network interface for the wireless access point.</returns>
        public static NetworkInterface GetInterface()
        {
            NetworkInterface[] Interfaces = NetworkInterface.GetAllNetworkInterfaces();

            // Find WirelessAP interface
            foreach (NetworkInterface ni in Interfaces)
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.WirelessAP)
                {
                    return ni;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the IP address of the Soft AP
        /// </summary>
        /// <returns>IP address</returns>
        public static string GetIP()
        {
            NetworkInterface ni = GetInterface();
            return ni.IPv4Address;
        }

    }
}
