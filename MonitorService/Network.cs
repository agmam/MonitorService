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
                // We calculate the utilization in %
                //Reason why we multiply by 8 is to convert from bytes to bites
                utilization = (((accumulatedDataTotal / intervals) * 8) / bandwidth) * 100;
                //We convert it to an int32 integer
                AvgUtilization = Convert.ToInt32(utilization);
            }
            catch (Exception e)
            {
                //If it fails we write a console error and post it on our log4net
                Console.WriteLine(e);
                Program.log.Error("Network read error: " + e);
            }

        }

        public static void ResetNetwork()
        {
            //We reset the counter used
            //Intervals is used for collection data in the 5 mins intervals we send data to the API
            intervals = 0;
            //We set accumilatedDataTotal which is the total amount of data from the network card
            accumulatedDataTotal = 0;
            //This is the avgutilization we send to the webapi and then resets it to 0
            AvgUtilization = 0;
        }

        public static int GetNetworkUtilization()
        {
            return AvgUtilization;
        }

    }
}
