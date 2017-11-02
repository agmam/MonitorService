﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace MonitorService
{
    public class Temperature
    {
        public double CurrentValue { get; set; }
        public string InstanceName { get; set; }
        public static List<Temperature> Temperatures
        {
            get
            {
                List<Temperature> result = new List<Temperature>();
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
                foreach (ManagementObject obj in searcher.Get())
                {
                    Double temp = Convert.ToDouble(obj["CurrentTemperature"].ToString());
                    temp = (temp - 2732) / 10.0;
                    result.Add(new Temperature { CurrentValue = temp, InstanceName = obj["InstanceName"].ToString() });
                    Console.WriteLine("temp: " + temp+ "   instance: " + obj["InstanceName"].ToString());
                }
                return result;

            }
        }
    }
}
