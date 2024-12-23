using Iot.Device.Ssd13xx;

using System;

namespace WeatherDisplay.Utils
{
    internal static class AppDrawExtension
    {
        public static void DrawAppTitle(this Ssd1306 oled, bool wifistatus, int wifimode)
        {
            oled.DrawWifiStatus(wifistatus);
            oled.DrawWifiMode(wifimode);
        }

        public static void DrawWifiStatus(this Ssd1306 oled, bool status)
        {
            var icon = status ? Icons.IconWifiOn : Icons.IconWifiOff;
            oled.DrawBitmap(icon, 0, 0, 16);
        }

        public static void DrawWifiMode(this Ssd1306 oled, int mode)
        {
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

    }
}
