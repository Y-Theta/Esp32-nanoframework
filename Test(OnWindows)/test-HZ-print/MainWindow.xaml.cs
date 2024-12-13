using HzRes;

using System.Collections.Generic;
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
        public MainWindow()
        {
            InitializeComponent();
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

        private async Task DrawCode()
        {
            int pixelsize = 4;
            await Task.CompletedTask;
            lock (_lock)
            {
                try
                {
                    CANVAS.BeginInit();
                    CANVAS.Children.Clear();
                    byte.TryParse(GB1.Text, out var gb1);
                    byte.TryParse(GB2.Text, out var gb2);
                    var offset = GetOffset(gb1, gb2);
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
    }
}