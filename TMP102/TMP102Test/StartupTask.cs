using System;
using Microsoft.Maker.Devices.I2C.TMP102;
using Windows.ApplicationModel.Background;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace TMP102Test
{
    public sealed class StartupTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            Tmp102 sensor = new Tmp102("I2C1");
            bool result = await sensor.BeginAsync();

            if (result)
            {
                while (true)
                {
                    float temperature = sensor.Temperature;
                    System.Diagnostics.Debug.WriteLine("**************************************************");
                    System.Diagnostics.Debug.WriteLine("TMP102 Test Verification");
                    System.Diagnostics.Debug.WriteLine("**************************************************");
                    System.Diagnostics.Debug.WriteLine("TEMPERATURE = [{0}]", temperature);
                    if (temperature < 10)
                    {
                        System.Diagnostics.Debug.WriteLine(">>> Failed. Received bad values from sensor");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(">>> Passed.");
                    }

                }
            }
        }
    }
}
