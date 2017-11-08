using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonitorService
{
    public static class Networkcalc
    {
        public static void ShowNetworkTraffic()
        {
            PerformanceCounterCategory performanceCounterCategory = new PerformanceCounterCategory("Network Interface");
            string instance = "";
            
            PerformanceCounterCategory category = new PerformanceCounterCategory("Network Interface");
            String[] instancename = category.GetInstanceNames();

            foreach (string name in instancename)
            {
                // Console.WriteLine(name);
                if (name.Contains("Wireless") || name.Contains("Gigabit Network"))
                {
                    instance = name;
                }
            }
          
            PerformanceCounter performanceCounterSent = new PerformanceCounter("Network Interface", "Bytes Sent/sec", instance);
            PerformanceCounter performanceCounterReceived = new PerformanceCounter("Network Interface", "Bytes Received/sec", instance);
            float bytesen = 0;
            float bytesrec = 0;
            for (int i = 0; i < 10; i++)
            {
                bytesen += performanceCounterSent.NextValue();
                bytesrec += performanceCounterReceived.NextValue();
                if (i%2 == 0)
                {
                    Console.WriteLine("bytes sent: {0}mbps\tbytes received: {1}mbps", Math.Round((bytesen / 1024)/1024), Math.Round((bytesrec / 1024) / 1024));
                    bytesen = 0;
                    bytesrec = 0;
                }
               
                Thread.Sleep(500);
            }
        }
       
    }
}
