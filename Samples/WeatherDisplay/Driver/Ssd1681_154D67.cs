using Iot.Device.EPaper.Drivers.Ssd168x;

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Text;
using System.Threading;

namespace HzPrint
{
    internal class Ssd1681_154D67 : Ssd168x
    {
        protected override int PagesPerFrame => 1;

        int waitsec = 20;
        SpiDevice _device;
        GpioController _pioController;

        int _dataCommandPin = 0;
        int _busyPin = 0;
        public Ssd1681_154D67(SpiDevice spiDevice, int resetPin, int busyPin, int dataCommandPin, int width, int height, GpioController gpioController = null, bool enableFramePaging = true, bool shouldDispose = true) :
            base(spiDevice, resetPin, busyPin, dataCommandPin, width, height, gpioController, enableFramePaging, shouldDispose)
        {
            _device = spiDevice;
            _pioController = gpioController;
            _dataCommandPin = dataCommandPin;
            _busyPin = busyPin;
        }


        public override void SendData(params byte[] data)
        {
            _pioController.Write(_dataCommandPin, PinValue.High);

            _device.Write(new SpanByte(data));

            _pioController.Write(_dataCommandPin, PinValue.Low);
        }

        public override void Initialize()
        {
            if (PowerState != Iot.Device.EPaper.Enums.PowerState.PoweredOn)
            {
                throw new InvalidOperationException();
            }

            WaitMs(10);
            SendCommand(0x12);
            WaitMs(10);
            SendCommand(0x01);
            SendData(0xC7);
            SendData(0x00);
            SendData(0x00);
            SendCommand(0x3C);
            SendData(0x05);
            SendCommand(0x18);
            SendDataD(0x80);
            SetParticalRam(0, 0, (UInt16)Width, (UInt16)Height);

            PerformFullRefresh();
        }

        private void SetParticalRam(UInt16 x, UInt16 y, UInt16 w, UInt16 h)
        {
            SendCommand(0x11);
            SendData(0x03);
            SendCommand(0x44);
            SendData((byte)(x / 8));
            SendData((byte)((x + w - 1) / 8));
            SendCommand(0x45);
            SendData((byte)(y % 256));
            SendData((byte)(y / 256));
            SendData((byte)((y + h - 1) % 256));
            SendData((byte)((y + h - 1) / 256));
            SendCommand(0x4e);
            SendData((byte)(x / 8));
            SendCommand(0x4f);
            SendData((byte)(y % 256));
            SendData((byte)(y / 256));
        }

        public void SendDataD(params ushort[] data)
        {
            // set the data/command pin to high to indicate to the display we will be sending data
            _pioController.Write(_dataCommandPin, PinValue.High);

            _device.Write(data);

            // go back to low (command mode)
            _pioController.Write(_dataCommandPin, PinValue.Low);
        }

        public override bool PerformPartialRefresh()
        {
            uint x = 0, y = 0, w = (uint)Width, h = (uint)Height;
            uint w1 = x < 0 ? w + x : w; // reduce
            uint h1 = y < 0 ? h + y : h; // reduce
            uint x1 = x < 0 ? 0 : x; // limit
            uint y1 = y < 0 ? 0 : y; // limit
            w1 = (uint)(x1 + w1 < Width ? w1 : Width - x1); // limit
            h1 = (uint)(y1 + h1 < Height ? h1 : Height - y1); // limit
            if ((w1 <= 0) || (h1 <= 0))
                return false;
            // make x1, w1 multiple of 8
            w1 += x1 % 8;
            if (w1 % 8 > 0) w1 += 8 - w1 % 8;
            x1 -= x1 % 8;
            SetParticalRam((UInt16)x1, (UInt16)y1, (UInt16)w1, (UInt16)h1);
            SendCommand(0x22);
            SendData(0xfc);
            SendCommand(0x20);
            return WaitReady();
        }

        public override bool PerformFullRefresh()
        {
            SendCommand(0x1A);
            SendData(0x64);
            SendCommand(0x22);
            SendData(0xd7);
            SendCommand(32);
            return WaitReady();
        }

        protected override void SoftwareReset()
        {
            SendCommand(18);
            WaitReady();
        }
    }
}
