﻿using System;

namespace WeatherDisplay.Utils
{
    internal class Icons
    {
        public static byte[] IconWifiOff = new byte[]
        {
            0x00, 0x00, 0x00, 0x00, 0x30, 0x00, 0x19, 0xe0, 0x1d, 0xf8, 0x7e, 0x1e, 0x63, 0x06, 0x07, 0x80,
            0x0f, 0xd0, 0x08, 0x60, 0x00, 0x30, 0x03, 0xd8, 0x01, 0x88, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };


        public static byte[] IconWifiOn = new byte[]
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0xe0, 0x1f, 0xf8, 0x78, 0x1e, 0x60, 0x06, 0x07, 0xe0,
            0x0f, 0xf0, 0x08, 0x10, 0x00, 0x00, 0x03, 0xc0, 0x01, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };


        public static byte[] IconWeatherCloud = new byte[]
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x18, 0x00, 0x3c, 0x0f, 0x3c, 0x3f, 0x9c, 0x3f, 0x80,
            0x3f, 0xe0, 0x3f, 0xf8, 0x3f, 0xf8, 0x1f, 0xf8, 0x0f, 0xf0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };


        public static byte[] IconWeatherRain = new byte[]
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0xc0, 0x07, 0xe0, 0x1f, 0xf8, 0x3f, 0xfc, 0x3f, 0xfc,
            0x3f, 0xfc, 0x0f, 0xf0, 0x00, 0x00, 0x0d, 0xb0, 0x0d, 0xb0, 0x0c, 0x30, 0x08, 0x00, 0x00, 0x00
        };


        public static byte[] IconWeatherSun = new byte[]
        {
            0x00, 0x00, 0x00, 0x00, 0x01, 0x80, 0x01, 0x80, 0x0d, 0xb0, 0x0f, 0xf0, 0x07, 0xe0, 0x3f, 0xfc,
            0x3f, 0xfc, 0x07, 0xe0, 0x0f, 0xf0, 0x0d, 0xb0, 0x01, 0x80, 0x01, 0x80, 0x00, 0x00, 0x00, 0x00
        };
    }
}
