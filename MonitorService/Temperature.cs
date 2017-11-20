using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using OpenHardwareMonitor.Hardware;

namespace MonitorService
{
    public class Temperature
    {
        private static Computer computer;
        public double CurrentValue { get; set; }
        public string InstanceName { get; set; }

        public Temperature()
        {
            computer = new Computer() { CPUEnabled = true };
            computer.Open();
        }
        public static float? Temperatures
        {
            get
            {
                foreach (var hardwareItem in computer.Hardware)
                {
                    if (hardwareItem.HardwareType == HardwareType.CPU)
                    {
                        hardwareItem.Update();
                        foreach (IHardware subHardware in hardwareItem.SubHardware)
                            subHardware.Update();

                        foreach (var sensor in hardwareItem.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Temperature)
                            {

                                return sensor.Value;

                            }
                        }
                    }
                }
                return 0;
            }
        }
    }
}
