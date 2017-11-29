using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.Devices;
using MonitorService.API_Connections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;

namespace MonitorService
{
    class Program
    {
        public static string token = "";


        public static ComputerInfo computerinfo;
        public static PerformanceCounter cpuCounter;
        public static PerformanceCounter ramCounter;
        public static PerformanceCounter upTime;

        private static Thread networkThread;
        private static Thread mainThread;
        private static Thread harddiskThread;

        static string url = Properties.Settings.Default.WebApiUrl;

        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static int bytesSent = 0;
        public static int bytesReceived = 0;

        private static string Username = "server@monitor.dk";
        private static string Password = "server123";
        public static int ServerId = 0;

        static void Main(string[] args)
        {
            Temperature cc = new Temperature(); //for the constructor
            upTime = new PerformanceCounter("System", "System Up Time");
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue(); //always 0 
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            computerinfo = new ComputerInfo();
           

            
            Console.WriteLine("Main...");
            Login.TryLogin(Username, Password);
            Console.WriteLine("Setting up server...");
            ServerId = Properties.Settings.Default.ServerId;


            if (!ServerConnector.IsServerInDatabase(ServerId))
            {
                ServerConnector.SetupServer();
            }

            Console.WriteLine("server ID - ID:" + ServerId);
            networkThread = new Thread(new ThreadStart(NetworkThreadMethod));
            mainThread =  new Thread(new ThreadStart(MainThreadMethod));
            networkThread.Start();
            mainThread.Start();
        }
        //1 Thread working
        static void NetworkThreadMethod()
        {
            while (true)
            {
                //Get networks utilization 5 times each second
                Network.CalculateNetworkUtilization();
                Thread.Sleep(200);
            }
            
        }

        //Another Thread working
        static void MainThreadMethod()
        {
            while (true)
            {
                //Sends server info every 5 seconds
                Thread.Sleep(5000);
                Console.WriteLine(Network.GetNetworkUtilization() + " %");
                Console.WriteLine(HarddiskStatus.UsedDiskSpace() + "GB - used disk space");
                Console.WriteLine(HarddiskStatus.TotalDiskSpace() + "GB - total disk space");
                ServerDetailConnector.SendServerInfo();
            }
            
        }
    }
}

