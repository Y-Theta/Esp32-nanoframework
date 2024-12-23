using HzPrint;

using Iot.Device.EPaper;
using Iot.Device.Ssd13xx;

using nanoFramework.Hardware.Esp32;

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Text;

namespace WeatherDisplay.Service
{
    internal class DisplayManager : IDisposable
    {
        Ssd1306 _oled = null;
        public Ssd1306 OLED => _oled;

        SpiDevice _epaperSpi = null;
        Ssd1681_154D67 _epaperdriver = null;
        Graphics _epaper = null;
        public Graphics Epaper => _epaper;

        public void InitOled()
        {
            Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
            Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
            I2cDevice i2cDev = new I2cDevice(new I2cConnectionSettings(1, 0x3c));
            _oled = new Ssd1306(i2cDev, Ssd13xx.DisplayResolution.OLED128x64);
            _oled.ClearScreen();
        }

        public void InitEpaper()
        {
            Configuration.SetPinFunction(15, DeviceFunction.SPI1_CLOCK);
            Configuration.SetPinFunction(2, DeviceFunction.SPI1_MOSI);
            var gpioController = new GpioController(PinNumberingScheme.Logical);
            // Setup SPI connection with the display
            var spiConnectionSettings = new SpiConnectionSettings(busId: 1, chipSelectLine: 18)
            {
                ClockFrequency = 20000000,
                Mode = SpiMode.Mode1,
                ChipSelectLineActiveState = false,
                Configuration = SpiBusConfiguration.HalfDuplex,
                DataFlow = DataFlow.MsbFirst,
            };

            _epaperSpi = new SpiDevice(spiConnectionSettings);
            // Create an instance of the display driver
            _epaperdriver = new Ssd1681_154D67(
                _epaperSpi,
                resetPin: 4,
                busyPin: 19,
                dataCommandPin: 5,
                width: 200,
                height: 200,
                gpioController,
                enableFramePaging: false,
                shouldDispose: true);

            _epaperdriver.PowerOn();
            _epaperdriver.Initialize();

            _epaper = new Graphics(_epaperdriver);
        }

        /// <summary>
        /// enter power save mode
        /// </summary>
        public void PowerSave()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public void Resume()
        {

        }

        public void Dispose()
        {
            _oled?.Dispose();
            _epaper?.Dispose();
            _epaperdriver?.Dispose();
            _epaperSpi?.Dispose();
        }
    }
}
