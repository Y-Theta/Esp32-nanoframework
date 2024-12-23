using Iot.Device.Ssd13xx;

using System;
using System.Text;

namespace WeatherDisplay.Utils
{
    internal static class AppDrawExtension
    {
        public static void DrawAppTitle(this Ssd1306 oled, bool wifistatus, int wifimode, string ip)
        {
            oled.DrawFilledRectangle(0, 0, 128, 15, false);
            oled.DrawWifiStatus(wifistatus, false);
            oled.DrawWifiMode(wifimode, false);
            oled.DrawIPAddress(ip, false);
        }

        public static void DrawWifiStatus(this Ssd1306 oled, bool status, bool clear = true)
        {
            if (clear)
                oled.DrawFilledRectangle(0, 0, 16, 15, false);

            var icon = status ? Icons.IconWifiOn : Icons.IconWifiOff;
            oled.DrawBitmap(icon, 0, 0, 16);
        }

        public static void DrawWifiMode(this Ssd1306 oled, int mode, bool clear = true)
        {
            if (clear)
                oled.DrawFilledRectangle(20, 0, 80, 8, false);
            string modestr = "NONE";
            switch (mode)
            {
                case 0:
                    modestr = "AP";
                    break;
                case 1:
                    modestr = "80211";
                    break;
            }
            oled.DrawString(modestr, 20, 0);
        }

        public static void DrawIPAddress(this Ssd1306 oled, string addr, bool clear = true)
        {
            if (clear)
                oled.DrawFilledRectangle(20, 8, 80, 8, false);

            StringBuilder sb = new StringBuilder();
            int offset = 20;
            foreach (char c in addr)
            {
                if (c != '.')
                {
                    sb.Append(c);
                }
                else
                {
                    oled.DrawString(sb.ToString(), offset, 8);
                    offset += sb.Length * 8 + 4;
                    sb.Clear();
                }
            }

            if (sb.Length > 0)
            {
                oled.DrawString(sb.ToString(), offset, 8);
            }
        }

    }
}
