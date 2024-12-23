using HzRes;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace test_HZ_print
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int StrCmpLogicalW(string x, string y);

        public MainWindow()
        {
            InitializeComponent();
            var comp = StrCmpLogicalW("", "");
        }

        public int GetOffset(byte gb1, byte gb2)
        {
            return (int)(94 * (gb1 - 1) + (gb2 - 1)) * 32;
        }

        private readonly object _lock = new object();
        private async void GB1_TextChanged(object sender, TextChangedEventArgs e)
        {
            await DrawCode();
        }

        static byte[] ToBigEndianBytes(int num, int length)
        {
            // 将整数转换为字节数组
            byte[] bytes = BitConverter.GetBytes(num);

            Array.Reverse(bytes);

            // 截取或填充字节数组到指定长度
            byte[] result = new byte[length];
            Array.Copy(bytes, 2, result, 0, Math.Min(bytes.Length, length));

            return result;
        }

        private int search(int low, int high, int m)
        {
            if (low >= 0 && low <= 7296 && high <= 7296 && high >= 0 && low <= high)
            {
                int mid = (low + high) / 2;
                int index = mid * 12;
                byte[] bytsl = new byte[6];
                byte[] bytsh = new byte[4];
                for (int i = 0; i < 12; i++)
                {
                    if (i < 6)
                    {
                        bytsl[i] = HzRes.Resource.utf2gb2312[index + i];
                    }
                    else if (i > 6 && i < 11)
                    {
                        bytsh[i - 7] = HzRes.Resource.utf2gb2312[index + i];
                    }
                }
                var mm = ToNum(bytsl);
                if (mm < m)
                    return search(mid + 1, high, m);
                else if (mm > m)
                    return search(low, mid - 1, m);
                else
                    return ToNum(bytsh);
            }

            return 0;
        }

        private int ToNum(byte[] hexarray)
        {
            int hex = 0;
            for (int i = hexarray.Length - 1; i >= 0; i--)
            {
                int cur = 0;
                var single = hexarray[i];
                if (single >= 97)
                {
                    cur = single - 87;
                }
                else
                {
                    cur = single - 48;
                }
                hex += cur * (int)Math.Pow(16, hexarray.Length - 1 - i);
            }
            return hex;
        }

        private async Task DrawCode()
        {
            await Task.CompletedTask;
            byte.TryParse(GB1.Text, out var gb1);
            byte.TryParse(GB2.Text, out var gb2);
            var offset = GetOffset(gb1, gb2);
            Draw(offset);
        }

        private void Draw(int offset)
        {
            lock (_lock)
            {
                try
                {
                    CANVAS.BeginInit();
                    CANVAS.Children.Clear();
                    int pixelsize = 4;
                    int row = -pixelsize;
                    int col = 0;
                    for (int i = 0; i < 32; i++)
                    {
                        if (i % 2 == 0)
                        {
                            row += pixelsize;
                            col = 0;
                        }

                        var code = HzRes.Resource.hzk16h[offset + i];
                        for (int o = 7; o >= 0; o--)
                        {
                            var bit = (code >> o) & 0b1;
                            if (bit > 0)
                            {
                                var rect = new Rectangle { Fill = Brushes.Black, Width = pixelsize, Height = pixelsize };
                                CANVAS.Children.Add(rect);
                                rect.SetValue(Canvas.LeftProperty, (double)col);
                                rect.SetValue(Canvas.TopProperty, (double)row);
                            }
                            col += pixelsize;
                        }
                    }
                }
                catch { }
                finally
                {
                    CANVAS.EndInit();
                }
            }
        }

        private void CH1_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var text = CH1.Text[0].ToString();
                var utfbyte = Encoding.UTF8.GetBytes(text);
                int s = 0;
                for (int i = 0; i < utfbyte.Length; i++)
                {
                    s = s << 8;
                    s += utfbyte[i];
                }
                var sd = search(0, 7296, s);
                var gbbytes = ToBigEndianBytes(sd, 2);
                var offset = GetOffset((byte)(gbbytes[0] - 0xa0), (byte)(gbbytes[1] - 0xa0));
                Draw(offset);
            }
            catch { }
        }

        public enum E1num
        {
            A = 1,
            B = 2
        }
    }
}