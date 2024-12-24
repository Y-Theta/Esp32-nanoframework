using nanoFramework.Runtime.Native;

using System;
using System.Collections;
using System.Device.Wifi;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

using WeatherDisplay.Utils;

namespace WeatherDisplay.Service
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
            string ssid = null;
            string password = null;
            bool isApSet = false;

            Program._manager.OLED.DrawConnectedDevice(request.RemoteEndPoint.Address.ToString());
            switch (request.HttpMethod)
            {
                case "GET":
                    string[] url = request.RawUrl.Split('?');
                    if (url[0] != "/favicon.ico")
                    {
                        response.ContentType = "text/html";
                        OutPutResponse(response, CreateMainPage());
                    }
                    break;
                case "POST":
                    // Pick up POST parameters from Input Stream
                    Hashtable hashPars = ParseParamsFromStream(request.InputStream);
                    ssid = (string)hashPars["ssid"];
                    password = (string)hashPars["password"];
                    isApSet = Wireless80211.Configure(ssid, password);
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

        static string CreateMainPage()
        {
            var html = "<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"UTF-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"><style>:root{--first-color:#1A73E8;--input-color:#80868B;--border-color:#DADCE0;--body-font:\"Roboto\",sans-serif;--normal-font-size:1rem;--small-font-size:.75rem}*,::after,::before{box-sizing:border-box}body{margin:0;padding:0;font-family:var(--body-font);font-size:var(--normal-font-size)}h1{margin:0}.l-form{display:flex;justify-content:center;align-items:center;height:100vh}.form{width:360px;padding:3rem 2rem;border-radius:1rem;box-shadow:0 10px 25px rgba(92,99,105,.2)}.form__title{font-weight:500;font-size:1.5rem;margin-bottom:2.5rem}.form__div{position:relative;height:52px;margin-bottom:1.5rem}.form__input{position:absolute;top:0;left:0;width:100%;height:100%;font-size:var(--normal-font-size);border:2px solid var(--border-color);border-radius:.5rem;outline:0;padding:1rem;background:0 0;z-index:1}.form__label{position:absolute;left:1rem;top:1rem;padding:0 .25rem;background-color:#fff;color:var(--input-color);font-size:var(--normal-font-size);transition:.3s}.form__button{display:block;margin-left:0;width:100%;padding:.75rem 2rem;outline:0;border:none;background-color:var(--first-color);color:#fff;font-size:var(--normal-font-size);border-radius:.25rem;cursor:pointer;transition:.3s}.form__input:focus+.form__label{top:-.5rem;left:.8rem;color:var(--first-color);font-size:var(--small-font-size);font-weight:500;z-index:10}.form__input:not(:placeholder-shown).form__input:not(:focus)+.form__label{top:-.5rem;left:.8rem;z-index:10;font-size:var(--small-font-size);font-weight:500}.form__input:focus{border:2px solid var(--first-color)}.form__fieldset{border:none}</style><title>nanoframework</title></head><body><div class=\"l-form\"><form method=\"POST\" class=\"form\"><h1 class=\"form__title\">设置WIFI连接信息</h1><fieldset class=\"form__fieldset\"><div class=\"form__div\"><input type=\"input\" name=\"ssid\" list=\"exampleList\" class=\"form__input\" placeholder=\" \"><label for=\"\" class=\"form__label\">Ssid</label></div><div class=\"form__div\"><input type=\"password\" name=\"password\" class=\"form__input\" placeholder=\" \"><label for=\"\" class=\"form__label\">Password</label></div><input type=\"submit\" class=\"form__button\" value=\"OK\"></fieldset></form></div></body></html>";
            return html;
        }
    }
}
