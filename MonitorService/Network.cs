using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonitorService
{
    public static class Network
    {
        public static int intervals = 0;
        private static double accumulatedDataTotal = 0;
        private static int AvgUtilization = 0;
        public static void CalculateNetworkUtilization()
        {

            try
            {
                intervals++;
                //We get the network interface used, which is the WiFi card.
                //We get a list with the WiFi card and not a list of all network cards.
                //WQL is WMI SQL some differences ex. LIKE vs CONTAINS
                ObjectQuery colItems = new ObjectQuery("SELECT * FROM Win32_PerfFormattedData_Tcpip_NetworkInterface WHERE Name LIKE '%Wireless%'");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(colItems);
                ManagementObjectCollection result = searcher.Get();
                //Initializion variables
                double dataTotal = -1;
                double utilization = 0;
                double bandwidth = 0;
                foreach (var sent in result)
                {
                    dataTotal = Convert.ToDouble(sent["BytesTotalPersec"]);//Get the property BytesTotalPerSec
                    accumulatedDataTotal += dataTotal;
                    bandwidth = Convert.ToDouble(sent["CurrentBandwidth"]);
                    break;
                }
                if (intervals < 1)
                {
                   intervals = 1; 
                }
                utilization = (((accumulatedDataTotal / intervals) * 8) / bandwidth) * 100;
                AvgUtilization = Convert.ToInt32(utilization);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Program.log.Error("Network read error: " + e);
            }

        }

        public static void ResetNetwork()
        {
            intervals = 0;
            accumulatedDataTotal = 0;
            AvgUtilization = 0;
        }

        public static int GetNetworkUtilization()
        {
            return AvgUtilization;
        }

    }
}
