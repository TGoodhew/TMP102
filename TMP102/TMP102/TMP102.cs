using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.Foundation;

namespace Microsoft.Maker.Devices.I2C.TMP102
{
    /// <summary>
    /// TMP102 Digital Temperature Sensor
    /// https://www.sparkfun.com/datasheets/Sensors/Temperature/tmp102.pdf
    /// </summary>
    public sealed class Tmp102
    {
        /// <summary>
        /// Device I2C Bus
        /// </summary>
        private string i2cBusName;

        /// <summary>
        /// Device I2C Address
        /// </summary>
        private const ushort Tmp102I2cAddress = 0x0048;

        /// <summary>
        /// Used to signal that the device is properly initialized and ready to use
        /// </summary>
        private bool available = false;

        /// <summary>
        /// I2C Device
        /// </summary>
        private I2cDevice i2c;

        /// <summary>
        /// Constructs Tmp102 with I2C bus identified
        /// </summary>
        /// <param name="i2cBusName">
        /// The bus name to provide to the enumerator
        /// </param>
        public Tmp102(string i2cBusName)
        {
            this.i2cBusName = i2cBusName;
        }

        /// <summary>
        /// Initialize the temerature device.
        /// </summary>
        /// <returns>
        /// Async operation object.
        /// </returns>
        public IAsyncOperation<bool> BeginAsync()
        {
            return this.BeginAsyncHelper().AsAsyncOperation<bool>();
        }

        /// <summary>
        /// Gets the current temperature
        /// </summary>
        /// <returns>
        /// The temperature in Celcius (C)
        /// </returns>
        public float Temperature
        {
            get
            {
                if (!this.available)
                {
                    return 0f;
                }

                ushort rawTemperatureData = this.RawTemperature;
                double temperatureCelsius = rawTemperatureData * 0.0625; // Per datasheet one LSB equals 0.0625 degrees C

                return Convert.ToSingle(temperatureCelsius);
            }
        }

        /// <summary>
        /// Private helper to initialize the TMP102 device.
        /// </summary>
        /// <remarks>
        /// Setup and instantiate the I2C device object for the HTU21D.
        /// </remarks>
        /// <returns>
        /// Task object.
        /// </returns>
        private async Task<bool> BeginAsyncHelper()
        {
            // Acquire the I2C device
            // MSDN I2C Reference: https://msdn.microsoft.com/en-us/library/windows/apps/windows.devices.i2c.aspx
            //
            // Use the I2cDevice device selector to create an advanced query syntax string
            // Use the Windows.Devices.Enumeration.DeviceInformation class to create a collection using the advanced query syntax string
            // Take the device id of the first device in the collection
            string advancedQuerySyntax = I2cDevice.GetDeviceSelector(i2cBusName);
            DeviceInformationCollection deviceInformationCollection = await DeviceInformation.FindAllAsync(advancedQuerySyntax);
            string deviceId = deviceInformationCollection[0].Id;

            // Establish an I2C connection to the TMP102
            //
            // Instantiate the I2cConnectionSettings using the device address of the HTU21D
            // - Set the I2C bus speed of connection to fast mode
            // - Set the I2C sharing mode of the connection to shared
            //
            // Instantiate the the TMP102 I2C device using the device id and the I2cConnectionSettings
            I2cConnectionSettings tmp102Connection = new I2cConnectionSettings(Tmp102.Tmp102I2cAddress);
            tmp102Connection.BusSpeed = I2cBusSpeed.FastMode;
            tmp102Connection.SharingMode = I2cSharingMode.Shared;

            this.i2c = await I2cDevice.FromIdAsync(deviceId, tmp102Connection);

            // Test to see if the I2C devices are available.
            //
            if (null == this.i2c)
            {
                this.available = false;
            }
            else
            {
                byte[] i2cTemperatureData = new byte[2];

                try
                {
                    this.i2c.Read(i2cTemperatureData);
                    this.available = true;
                }
                catch
                {
                    this.available = false;
                }
            }

            return this.available;
        }

        /// <summary>
        /// Gets the raw temperature value from the IC.
        /// </summary>
        private ushort RawTemperature
        {
            get
            {
                ushort temperature = 0;
                byte[] i2cTemperatureData = new byte[2];

                // Request temperature data from the TMP102
                // TMP102 datasheet: https://www.sparkfun.com/datasheets/Sensors/Temperature/tmp102.pdf
                //
                // Read the two bytes returned by the TMP102
                // - byte 0 - MSB of the temperature
                // - byte 1 - LSB of the temperature
                this.i2c.Read(i2cTemperatureData);

                // Reconstruct the result using the two bytes returned from the device
                //
                temperature = (ushort)(i2cTemperatureData[0] << 4);
                temperature |= (ushort)(i2cTemperatureData[1] >> 4);

                return temperature;
            }
        }
    }
}
