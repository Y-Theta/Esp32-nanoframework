
using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;

using TekuSP.Drivers.DriverBase;
using TekuSP.Drivers.DriverBase.Interfaces;

namespace Gc9A01A
{
    public enum Command : byte
    {
        GC9A01A_SWRESET = 0x01,///< Software Reset (maybe, not documented)
        GC9A01A_RDDID = 0x04,///< Read display identification information
        GC9A01A_RDDST = 0x09,///< Read Display Status
        GC9A01A_SLPIN = 0x10,///< Enter Sleep Mode
        GC9A01A_SLPOUT = 0x11,///< Sleep Out
        GC9A01A_PTLON = 0x12,///< Partial Mode ON
        GC9A01A_NORON = 0x13,///< Normal Display Mode ON
        GC9A01A_INVOFF = 0x20,///< Display Inversion OFF
        GC9A01A_INVON = 0x21,///< Display Inversion ON
        GC9A01A_DISPOFF = 0x28,///< Display OFF
        GC9A01A_DISPON = 0x29,///< Display ON
        GC9A01A_CASET = 0x2A,///< Column Address Set
        GC9A01A_RASET = 0x2B,///< Row Address Set
        GC9A01A_RAMWR = 0x2C,///< Memory Write
        GC9A01A_PTLAR = 0x30,///< Partial Area
        GC9A01A_VSCRDEF = 0x33,///< Vertical Scrolling Definition
        GC9A01A_TEOFF = 0x34,///< Tearing Effect Line OFF
        GC9A01A_TEON = 0x35,///< Tearing Effect Line ON
        GC9A01A_MADCTL = 0x36,///< Memory Access Control
        GC9A01A_VSCRSADD = 0x37,///< Vertical Scrolling Start Address
        GC9A01A_IDLEOFF = 0x38,///< Idle mode OFF
        GC9A01A_IDLEON = 0x39,///< Idle mode ON
        GC9A01A_COLMOD = 0x3A,///< Pixel Format Set
        GC9A01A_CONTINUE = 0x3C,///< Write Memory Continue
        GC9A01A_TEARSET = 0x44,///< Set Tear Scanline
        GC9A01A_GETLINE = 0x45,///< Get Scanline
        GC9A01A_SETBRIGHT = 0x51,///< Write Display Brightness
        GC9A01A_SETCTRL = 0x53,///< Write CTRL Display
        GC9A01A1_POWER7 = 0xA7,///< Power Control 7
        GC9A01A_TEWC = 0xBA,///< Tearing effect width control
        GC9A01A1_POWER1 = 0xC1,///< Power Control 1
        GC9A01A1_POWER2 = 0xC3,///< Power Control 2
        GC9A01A1_POWER3 = 0xC4,///< Power Control 3
        GC9A01A1_POWER4 = 0xC9,///< Power Control 4
        GC9A01A_RDID1 = 0xDA,///< Read ID 1
        GC9A01A_RDID2 = 0xDB,///< Read ID 2
        GC9A01A_RDID3 = 0xDC,///< Read ID 3
        GC9A01A_FRAMERATE = 0xE8,///< Frame rate control
        GC9A01A_SPI2DATA = 0xE9,///< SPI 2DATA control
        GC9A01A_INREGEN2 = 0xEF,///< Inter register enable 2
        GC9A01A_GAMMA1 = 0xF0,///< Set gamma 1
        GC9A01A_GAMMA2 = 0xF1,///< Set gamma 2
        GC9A01A_GAMMA3 = 0xF2,///< Set gamma 3
        GC9A01A_GAMMA4 = 0xF3,///< Set gamma 4
        GC9A01A_IFACE = 0xF6,///< Interface control
        GC9A01A_INREGEN1 = 0xFE,///< Inter register enable 1
    }

    public enum Color : UInt16
    {
        GC9A01A_BLACK = 0x0000,       ///<   0,   0,   0
        GC9A01A_NAVY = 0x000F,        ///<   0,   0, 123
        GC9A01A_DARKGREEN = 0x03E0,   ///<   0, 125,   0
        GC9A01A_DARKCYAN = 0x03EF,    ///<   0, 125, 123
        GC9A01A_MAROON = 0x7800,      ///< 123,   0,   0
        GC9A01A_PURPLE = 0x780F,      ///< 123,   0, 123
        GC9A01A_OLIVE = 0x7BE0,       ///< 123, 125,   0
        GC9A01A_LIGHTGREY = 0xC618,   ///< 198, 195, 198
        GC9A01A_DARKGREY = 0x7BEF,    ///< 123, 125, 123
        GC9A01A_BLUE = 0x001F,        ///<   0,   0, 255
        GC9A01A_GREEN = 0x07E0,       ///<   0, 255,   0
        GC9A01A_CYAN = 0x07FF,        ///<   0, 255, 255
        GC9A01A_RED = 0xF800,         ///< 255,   0,   0
        GC9A01A_MAGENTA = 0xF81F,     ///< 255,   0, 255
        GC9A01A_YELLOW = 0xFFE0,      ///< 255, 255,   0
        GC9A01A_WHITE = 0xFFFF,       ///< 255, 255, 255
        GC9A01A_ORANGE = 0xFD20,      ///< 255, 165,   0
        GC9A01A_GREENYELLOW = 0xAFE5, ///< 173, 255,  41
        GC9A01A_PINK = 0xFC18,        ///< 255, 130, 198
    }

    public enum DisplayMode : byte
    {
        MADCTL_MY = 0x80,  ///< Bottom to top
        MADCTL_MX = 0x40,  ///< Right to left
        MADCTL_MV = 0x20,  ///< Reverse Mode
        MADCTL_ML = 0x10,  ///< LCD refresh Bottom to top
        MADCTL_RGB = 0x00, ///< Red-Green-Blue pixel order
        MADCTL_BGR = 0x08, ///< Blue-Green-Red pixel order
        MADCTL_MH = 0x04,  ///< LCD refresh right to left
    }

    internal class Bounds
    {
        public Int16 X;
        public Int16 Y;
        public Int16 Width;
        public Int16 Height;

        public Bounds(Int16 x, Int16 y, Int16 width, Int16 height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    internal class DisplayArea
    {
        public Point P1 = new Point();
        public Point P2 = new Point();
    }

    internal class Point
    {
        public Int16 X;
        public Int16 Y;

        public Point() { }

        public Point(Int16 x, Int16 y)
        {
            X = x;
            Y = y;
        }
    }

    public class Gc9A01AConfig
    {
        public byte BackLightPin { get; set; }

        public DisplayMode Mode { get; set; }

        public Int16 Xoffset { get; set; }

        public Int16 Yoffset { get; set; }

        public bool WarpX { get; set; }

        public bool WarpY { get; set; }

        public byte Rotation { get; set; }
    }

    public class Gc9A01ADriver : DriverBaseSPI, IPowerSaving
    {

        public const byte Width = 240;
        public const byte Height = 240;

        private const byte xoffset = 0;
        private const byte yoffset = 0;

        public const int SPI_DEFAULT_FREQ = 200000000;

        private GpioController _controller;

        public GpioController GPIOCtl => _controller;

        internal readonly Bounds DisplayBounds;

        internal readonly DisplayArea DisplayArea = new DisplayArea();

        public readonly Gc9A01AConfig Config;

        public readonly int RST = -1;

        public readonly int DC = -1;

        public readonly int BL = -1;

        public readonly int CS = -1;

        public Gc9A01ADriver(string name, int SPIBusID, int chipSelectPin, int rstPin, int dataCommand, Gc9A01AConfig config = null) 
            : base(name, SPIBusID, -1)
        {
            RST = rstPin;
            DC = dataCommand;
            CS = chipSelectPin;

            Config = config;
            if (config != null)
            {
                BL = config.BackLightPin;
            }
            DisplayBounds = new Bounds(xoffset, yoffset, Width, Height);
        }

        public override void Start()
        {
            base.Start();
            InitPins();

            HardRst();
            DelayMs(100);
            SoftRst();
            DelayMs(100);

            InitScreen();
        }

        public override void Stop()
        {
            base.Stop();
            ReleasePins();
        }

        private void InitPins()
        {
            _controller = new GpioController();
            _controller.OpenPin(RST, PinMode.Output);
            _controller.OpenPin(DC, PinMode.Output);
            _controller.OpenPin(BL, PinMode.Output);
            _controller.OpenPin(CS, PinMode.Output);
            //_controller.OpenPin(SpiDevice.ConnectionSettings.ChipSelectLine, PinMode.Output);
        }

        private void ReleasePins()
        {
            _controller.Dispose();
        }

        #region   ScreenOperation
        private void InitScreen()
        {
            WriteCmd((byte)Command.GC9A01A_INREGEN2, null, (byte)0);
            WriteCmd((byte)0xEB, new byte[] { 0x14 }, (byte)1);
            WriteCmd((byte)Command.GC9A01A_INREGEN1, null, (byte)0);
            WriteCmd((byte)Command.GC9A01A_INREGEN2, null, (byte)0);
            WriteCmd((byte)0xEB, new byte[] { 0x14 }, (byte)1);
            WriteCmd((byte)0x84, new byte[] { 0x40 }, (byte)1);
            WriteCmd((byte)0x85, new byte[] { 0xff }, (byte)1);
            WriteCmd((byte)0x86, new byte[] { 0xff }, (byte)1);
            WriteCmd((byte)0x87, new byte[] { 0xff }, (byte)1);
            WriteCmd((byte)0x88, new byte[] { 0x0A }, (byte)1);
            WriteCmd((byte)0x89, new byte[] { 0x21 }, (byte)1);
            WriteCmd((byte)0x8A, new byte[] { 0x00 }, (byte)1);
            WriteCmd((byte)0x8b, new byte[] { 0x80 }, (byte)1);
            WriteCmd((byte)0x8C, new byte[] { 0x01 }, (byte)1);
            WriteCmd((byte)0x8D, new byte[] { 0x01 }, (byte)1);
            WriteCmd((byte)0x8E, new byte[] { 0xff }, (byte)1);
            WriteCmd((byte)0x8F, new byte[] { 0xff }, (byte)1);
            WriteCmd((byte)0xB6, new byte[] { 0x00, 0x00 }, (byte)2);
            WriteCmd((byte)0x3A, new byte[] { 0x55 }, (byte)1);
            WriteCmd((byte)0x90, new byte[] { 0x08, 0x08, 0x08, 0x08 }, (byte)4);
            WriteCmd((byte)0xBD, new byte[] { 0x06 }, (byte)1);
            WriteCmd((byte)0xBC, new byte[] { 0x00 }, (byte)1);
            WriteCmd((byte)0xFF, new byte[] { 0x60, 0x01, 0x04 }, (byte)3);
            WriteCmd((byte)0xC3, new byte[] { 0x13 }, (byte)1);
            WriteCmd((byte)0xC4, new byte[] { 0x13 }, (byte)1);
            WriteCmd((byte)0xC9, new byte[] { 0x22 }, (byte)1);
            WriteCmd((byte)0xBE, new byte[] { 0x11 }, (byte)1);
            WriteCmd((byte)0xE1, new byte[] { 0x10, 0x0e }, (byte)2);
            WriteCmd((byte)0xDF, new byte[] { 0x21, 0x0c, 0x02 }, (byte)3);
            WriteCmd((byte)0xF0, new byte[] { 0x45, 0x09, 0x08, 0x08, 0x26, 0x2a }, (byte)6);
            WriteCmd((byte)0xF1, new byte[] { 0x43, 0x70, 0x72, 0x36, 0x37, 0x6f }, (byte)3);
            WriteCmd((byte)0xF2, new byte[] { 0x45, 0x09, 0x08, 0x08, 0x26, 0x2a }, (byte)6);
            WriteCmd((byte)0xF3, new byte[] { 0x43, 0x70, 0x72, 0x36, 0x37, 0x6f }, (byte)6);
            WriteCmd((byte)0xED, new byte[] { 0x1b, 0x0b }, (byte)2);
            WriteCmd((byte)0xAE, new byte[] { 0x77 }, (byte)1);
            WriteCmd((byte)0xCD, new byte[] { 0x63 }, (byte)1);
            WriteCmd((byte)0x70, new byte[] { 0x07, 0x07, 0x04, 0x0e, 0x0f, 0x09, 0x07, 0x08, 0x03 }, (byte)9);
            WriteCmd((byte)0xE8, new byte[] { 0x34 }, (byte)1);
            WriteCmd((byte)0x62, new byte[] { 0x18, 0x0d, 0x71, 0xed, 0x70, 0x70, 0x18, 0x0f, 0x71, 0xef, 0x70, 0x70 }, (byte)12);
            WriteCmd((byte)0x63, new byte[] { 0x18, 0x11, 0x71, 0xf1, 0x70, 0x70, 0x18, 0x13, 0x71, 0xf3, 0x70, 0x70 }, (byte)12);
            WriteCmd((byte)0x64, new byte[] { 0x28, 0x29, 0xf1, 0x01, 0xf1, 0x00, 0x07 }, (byte)7);
            WriteCmd((byte)0x66, new byte[] { 0x3c, 0x00, 0xcd, 0x67, 0x45, 0x45, 0x10, 0x00, 0x00, 0x00 }, (byte)10);
            WriteCmd((byte)0x67, new byte[] { 0x00, 0x3c, 0x00, 0x00, 0x00, 0x01, 0x54, 0x10, 0x32, 0x98 }, (byte)10);
            WriteCmd((byte)0x74, new byte[] { 0x10, 0x85, 0x80, 0x00, 0x00, 0x4e, 0x00 }, (byte)7);
            WriteCmd((byte)0x98, new byte[] { 0x3e, 0x07 }, (byte)2);
            WriteCmd((byte)0x35, null, 0);
            WriteCmd((byte)0x21, null, 0);
            WriteCmd((byte)0x11, null, 0);
            DelayMs(120);
            WriteCmd((byte)0x29, null, 0);
            DelayMs(20);
            SetRotation();
            BL_HIGH();
            WriteCmd((byte)Command.GC9A01A_DISPON, null, 0);
            DelayMs(120);
        }

        private void SetRotation()
        {
            byte ctl = (byte)DisplayMode.MADCTL_BGR;

            if (Config != null)
            {
                switch (Config.Rotation)
                {
                    case 0:
                        ctl |= (byte)DisplayMode.MADCTL_MX;
                        DisplayBounds.Width = Width;
                        DisplayBounds.Height = Height;
                        break;
                    case 1:
                        ctl |= (byte)DisplayMode.MADCTL_MV;
                        DisplayBounds.Width = Height;
                        DisplayBounds.Height = Width;
                        break;
                    case 2:
                        ctl |= (byte)DisplayMode.MADCTL_MY;
                        DisplayBounds.Width = Width;
                        DisplayBounds.Height = Height;
                        break;
                    case 3:
                        ctl |= (byte)DisplayMode.MADCTL_MX | (byte)DisplayMode.MADCTL_MY | (byte)DisplayMode.MADCTL_MV;
                        DisplayBounds.Width = Height;
                        DisplayBounds.Height = Width;
                        break;
                    case 4:
                        DisplayBounds.Width = Width;
                        DisplayBounds.Height = Height;
                        break;
                    case 5:
                        ctl |= (byte)DisplayMode.MADCTL_MX | (byte)DisplayMode.MADCTL_MV;
                        DisplayBounds.Width = Height;
                        DisplayBounds.Height = Width;
                        break;
                    case 6:
                        ctl |= (byte)DisplayMode.MADCTL_MX | (byte)DisplayMode.MADCTL_MY;
                        DisplayBounds.Width = Width;
                        DisplayBounds.Height = Height;
                        break;
                    case 7:
                        ctl |= (byte)DisplayMode.MADCTL_MV| (byte)DisplayMode.MADCTL_MY;
                        DisplayBounds.Width = Height;
                        DisplayBounds.Height = Width;
                        break;
                }
            }

            DisplayArea.P1.X = DisplayBounds.Width;
            DisplayArea.P1.Y = DisplayBounds.Height;
            DisplayArea.P2.X = 0;
            DisplayArea.P2.Y = 0;

            WriteCmd((byte)Command.GC9A01A_MADCTL, new byte[] { ctl }, 1);
        }

        public void ClearScreen(Color bg)
        {
            SetWindow(0, 0, 239, 239);
            byte hi = (byte)((UInt16)bg >> 8);
            byte lo = (byte)bg;
            for (int i = 0; i < 240; i++)
            {
                for (int j = 0; j < 240; j++)
                {
                    CS_LOW();
                    DC_HIGH();
                    SpiDevice.WriteByte(hi);
                    SpiDevice.WriteByte(lo);
                    CS_HIGH();
                }
            }
        }

        private void FillColorBuffer(UInt16 color, Int16 length)
        {
            int buffer_pixel_size = 128;
            int chunks = length / buffer_pixel_size;
            int rest = length % buffer_pixel_size;
            UInt16 color_swapped = SwapBytes(color);
            UInt16[] buffer = new UInt16[buffer_pixel_size];

            for (int i = 0; i < length && i < buffer_pixel_size; i++)
            {
                buffer[i] = color_swapped;
            }

            if (chunks > 0)
            {
                for (int j = 0; j < chunks; j++)
                {
                    WriteData(buffer, buffer_pixel_size * 2);
                }
            }

            if (rest > 0)
            {
                WriteData(buffer, rest * 2);
            }
        }

        public void DrawPixel(Int16 x, Int16 y, UInt16 color)
        {
            if (Config != null)
            {
                if (Config.WarpX && ((x >= DisplayBounds.Width) || (x < 0)))
                {
                    x = (Int16)(x % DisplayBounds.Width);
                }

                if (Config.WarpY && ((y >= DisplayBounds.Height) || (y < 0)))
                {
                    y = (Int16)(y % DisplayBounds.Height);
                }
            }

            if ((x < DisplayBounds.Width) && (y < DisplayBounds.Height) && (x >= 0) && (y >= 0))
            {
                byte hi = B8(color >> 8), lo = B8(color & 0xff);

                SetWindow(x, y, x, y);
                DC_HIGH();
                CS_LOW();
                WriteData(new byte[] { hi });
                WriteData(new byte[] { lo });
                CS_HIGH();
            }
        }

        public void FastHline(Int16 x, Int16 y, Int16 w, UInt16 color)
        {
            if (y >= 0 && DisplayBounds.Width > x && DisplayBounds.Height > y)
            {
                if (0 > x)
                {
                    w += x;
                    x = 0;
                }

                if (DisplayBounds.Width < x + w)
                {
                    w = (Int16)(DisplayBounds.Width - x);
                }

                if (w > 0)
                {
                    Int16 x2 = (Int16)(x + w - 1);

                    SetWindow(x, y, x2, y);
                    DC_HIGH();
                    CS_LOW();
                    FillColorBuffer(color, w);
                    CS_HIGH();
                }
            }
        }

        private void SetWindow(DisplayArea area)
        {
            SetWindow(area.P1.X, area.P1.Y, area.P2.X, area.P2.Y);
        }

        private void SetWindow(Int16 x0, Int16 y0, Int16 x1, Int16 y1)
        {
            if (x0 > x1 || x1 >= DisplayBounds.Width)
            {
                return;
            }

            if (y0 > y1 || y1 >= DisplayBounds.Height)
            {
                return;
            }

            if (x0 < DisplayArea.P1.X)
            {
                DisplayArea.P1.X = x0;
            }

            if (x1 > DisplayArea.P2.X)
            {
                DisplayArea.P2.X = x1;
            }

            if (y0 < DisplayArea.P1.Y)
            {
                DisplayArea.P1.Y = y0;
            }

            if (y1 > DisplayArea.P2.Y)
            {
                DisplayArea.P2.Y = y1;
            }

            int offsetx = 0, offsety = 0;
            if (Config != null)
            {
                offsetx = Config.Xoffset;
                offsety = Config.Yoffset;
            }

            byte[] bufx = new byte[] { B8((x0 + offsetx) >> 8), B8((x0 + offsetx) & 0xFF), B8((x1 + offsetx) >> 8), B8((x1 + offsetx) & 0xFF) };
            byte[] bufy = new byte[] { B8((y0 + offsety) >> 8), B8((y0 + offsety) & 0xFF), B8((y1 + offsety) >> 8), B8((y1 + offsety) & 0xFF) };

            WriteCmd((byte)Command.GC9A01A_CASET, bufx, 4);
            WriteCmd((byte)Command.GC9A01A_RASET, bufy, 4);
            WriteCmd((byte)Command.GC9A01A_RAMWR, null, 0);
        }
        #endregion

        #region   SPI enhanced

        private void DelayMs(int ms)
        {
            Thread.Sleep(ms);
        }

        private void HardRst()
        {
            CS_LOW();
            RST_HIGH();
            DelayMs(50);
            RST_LOW();
            DelayMs(50);
            RST_HIGH();
            DelayMs(150);
            CS_HIGH();
        }

        private void SoftRst()
        {
            WriteCmd((byte)Command.GC9A01A_SWRESET, null, (byte)0);
            DelayMs(150);
        }

        #endregion

        #region   Cmd
        private void WriteCmd(byte cmd, byte[] data, byte len)
        {
            CS_LOW();
            if (cmd > 0)
            {
                DC_LOW();
                SpiDevice.Write(new byte[] { 0, 1, cmd });
            }
            if (len > 0)
            {
                DC_HIGH();
                SpiDevice.Write(new byte[] { 0, len });
                SpiDevice.Write(data);
            }
            CS_HIGH();
        }

        #endregion

        #region   PinControl
        private void CS_LOW()
        {
            _controller.Write(CS, 0);
        }

        private void CS_HIGH()
        {
            _controller.Write(CS, 1);
        }

        private void DC_LOW()
        {
            _controller.Write(DC, 0);
        }

        private void DC_HIGH()
        {
            _controller.Write(DC, 1);
        }

        private void RST_LOW()
        {
            _controller.Write(RST, 0);
        }

        private void RST_HIGH()
        {
            _controller.Write(RST, 1);
        }

        private void BL_LOW()
        {
            if (BL > 0)
            {
                _controller.Write(BL, 0);
            }
        }

        private void BL_HIGH()
        {
            if (BL > 0)
            {
                _controller.Write(BL, 1);
            }
        }
        #endregion

        #region   DriverBaseSPI

        public override long ReadData(byte pointer)
        {
            return SpiDevice.ReadByte();
        }

        public override long ReadData(byte[] data)
        {
            SpiDevice.Read(data);
            return data.Length;
        }

        public override string ReadDeviceId()
        {
            return "GC9A01A";
        }

        public override string ReadManufacturerId()
        {
            return default;

        }

        public override string ReadSerialNumber()
        {
            return default;
        }

        public void WriteData(UInt16[] data, int length)
        {
            var hi = B8(length >> 8 & 0xff);
            var lo = B8(length & 0xff);
            SpiDevice.WriteByte(hi);
            SpiDevice.WriteByte(lo);
            SpiDevice.Write(data);
        }

        public override void WriteData(byte[] data)
        {
            SpiDevice.WriteByte(B8(data.Length >> 8 & 0xff));
            SpiDevice.WriteByte(B8(data.Length & 0xff));
            SpiDevice.Write(data);
        }
        #endregion

        #region   IPowerSaving
        public void Sleep()
        {
            throw new System.NotImplementedException();
        }

        public void Wakeup()
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region   utils

        private static byte B8(int x)
        {
            return (byte)x;
        }

        private static UInt16 SwapBytes(UInt16 val)
        {
            return (UInt16)(((val >> 8) & 0xff) | ((val << 8) & 0xff00)); 
        }
        #endregion
    }
}
