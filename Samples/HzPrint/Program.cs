using Iot.Device.EPaper;

using System.Diagnostics;
using System.Threading;

using IFont = nanoFramework.UI.IFont;


namespace HzPrint
{
    public class Program
    {

        public static void Main()
        {
            Debug.WriteLine("program Started !");
            DisplayManager manager = new DisplayManager();
            manager.InitOled();


            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }

        public static void DrawString(string str, IFont font ,Graphics gfx)
        {
            int initc = 0;
            int col = 0;
            int row = 0;

            foreach (char c in str)
            {
                var bytt = font[c];
                for (int i = 0; i < bytt.Length;)
                {
                    if (i % 2 == 0)
                    {
                        row += 1;
                        col = initc;
                    }

                    var item = bytt[i];
                    for (int x = 0; x < 8; x++)
                    {
                        var bit = (item >> x) & 0b1;
                        if (bit > 0)
                        {
                            gfx.EPaperDisplay.FrameBuffer.SetPixel(new System.Drawing.Point(col, row), System.Drawing.Color.Black);
                        }
                        col += 1;
                    }
                    i++;
                }

                row = 0;
                initc += 16;
            }

        }

    }
}
