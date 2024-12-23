using Iot.Device.Ssd13xx;

using System;

namespace HzPrint
{
    public static class OledExtension
    {
        public static void DrawString(this Ssd1306 oled, string str, int x, int y, bool wrap = false)
        {
            int baseoffsetx = x;
            int baseoffsety = y;

            foreach (char c in str)
            {
                var offset = c << 3;
                SpanByte bitmap = new SpanByte(BaseFont.LCD, offset, 8);
                for (int i = 0; i < 8; i++)
                {
                    var bit = bitmap[i];
                    var l = 0x80;
                    for (int b = 0; b < 8; b++)
                    {
                        if ((bit & l) == l)
                        {
                            oled.DrawPixel(baseoffsetx + i, baseoffsety + 8 - b, true);
                        }
                        l = l >> 1;
                    }
                }

                baseoffsetx += 8;

                if (wrap)
                {
                    if (baseoffsetx >= 128)
                    {
                        baseoffsetx %= 128;
                        baseoffsety += 8;
                    }
                }
            }

            oled.Display();
            GC.WaitForPendingFinalizers();
        }

        public static void DrawBitmap(this Ssd1306 oled, byte[] bmp, int x, int y, int imgwidh, bool invert = false)
        {
            int layer = 0;
            int col = 0;
            for (int i = 0; i < bmp.Length; i++)
            {
                var bit = bmp[i];
                var l = 0x80;
                for (int b = 0; b < 8; b++)
                {
                    if ((bit & l) == l)
                    {
                        oled.DrawPixel(x + col, y + layer, invert ? false : true);
                    }
                    l = l >> 1;
                    col++;
                    if (col >= imgwidh)
                    {
                        layer++;
                        col = 0;
                    }
                }
            }
        }
    }
}
