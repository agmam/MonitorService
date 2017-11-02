using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace MonitorService
{
    class Network
    {
        public static string getNetworkCardName()
        {
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface networkInterface in adapters)
            {
                Console.WriteLine(networkInterface.Name + " : " + networkInterface.Description+ " : " + networkInterface.OperationalStatus + " : " + networkInterface.NetworkInterfaceType);

                //if (networkInterface.OperationalStatus == OperationalStatus.Up)
                //{
                //    Console.WriteLine(networkInterface.Name + ": UP");
                //}  
            }
            return null;
        }
    }
}
