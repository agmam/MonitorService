using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualBasic.Devices;
using MonitorService.API_Connections;
using System.IO;

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

        static string url = Properties.Settings.Default.WebApiUrl;

        public static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
            Console.WriteLine("Setting up server...");
          
            Login.TryLogin(Username, Password);
            int id = 0;
            try
            {
                var pathtoid = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                var filenametoid = @"\MonitorServiceId.txt";
                pathtoid += filenametoid;
                
                if (File.Exists(pathtoid))
                {
                    id = Convert.ToInt32(File.ReadAllText(pathtoid));
                }
                else
                {
                    File.WriteAllText(pathtoid, 0+"");
                }
                Properties.Settings.Default.ServerId = id;
                Properties.Settings.Default.Save();
            }
            catch (Exception e)
            {

                Console.WriteLine("Id error: "+ e);
                log.Error("id error: " + e);
            }
           
            ServerId = Properties.Settings.Default.ServerId;
           

            if (!ServerConnector.IsServerInDatabase(ServerId))
            {
                ServerConnector.SetupServer();
            }

            Console.WriteLine("server ID - ID:" + ServerId);
            networkThread = new Thread(new ThreadStart(NetworkThreadMethod));
            mainThread = new Thread(new ThreadStart(MainThreadMethod));
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
                ServerDetailConnector.SendServerInfo();

            }
            
        }
    }
}

