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


                //List<Temperature> result = new List<Temperature>();
                //ManagementObjectSearcher searcher =
                //    new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
                //foreach (ManagementObject obj in searcher.Get())
                //{
                //    Double temp = Convert.ToDouble(obj["CurrentTemperature"].ToString());
                //    temp = (temp - 2732) / 10.0;
                //    result.Add(new Temperature {CurrentValue = temp, InstanceName = obj["InstanceName"].ToString()});
                //    //Console.WriteLine("temp: " + temp+ "   instance: " + obj["InstanceName"].ToString());
                //}
                //return result;

            }
        }
    }
}
