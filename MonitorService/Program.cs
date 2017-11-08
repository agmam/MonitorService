using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.Devices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MonitorService
{
    class Program
    {
        public static string token = "";


        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
     
        static ComputerInfo computerinfo;
        static PerformanceCounter cpuCounter;
        static PerformanceCounter ramCounter;
        static PerformanceCounter upTime;
        static string url = Properties.Settings.Default.WebApiUrl;

        static string networkCard = "";
        static int bytesSent = 0;
        static int bytesReceived = 0;

        private static string Username = "server@monitor.dk";
        private static string Password = "server123";
        private static int ServerId = 0;
        static void Main(string[] args)
        {
            upTime = new PerformanceCounter("System", "System Up Time");
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue(); //always 0
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            computerinfo = new ComputerInfo();
            //Network.getNetworkCardName();
           

            
            Console.WriteLine("Main...");

            Login(Username, Password);

            Console.WriteLine("Setting up server...");
            ServerId = Properties.Settings.Default.ServerId;


            if (!IsServerInDatabase(ServerId))
            {
                SetupServer();
            }

            Console.WriteLine("server ID - ID:" + ServerId);
            while (true)
            {
               
                Networkcalc.ShowNetworkTraffic();
               // Thread.Sleep(6000);
                     
               //SendServerInfo();
            }
        }

        private static bool IsServerInDatabase(int id)
        {
            HttpClient client = new HttpClient();
            AddHeaders(client);
            HttpResponseMessage response = null;
            try
            {
                Console.WriteLine("geting server...");
                response =  client.GetAsync("api/Servers/GetServer/"+ id).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("Successfully");
                }
                else
                {
                    Console.WriteLine("getServerAsync - status code " + response.StatusCode);
                    log.Info("getServerAsync - status code " + response.StatusCode);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("getServerAsync - Error " + e);
                log.Info("getServerAsync - Error " + e);

            }
            if (response == null || response.StatusCode == HttpStatusCode.NotFound) return false;
            return true;
        }

        private static void SetupServer()
        {
            HttpClient client = new HttpClient();
            AddHeaders(client);

            try
            {
                var name = GetServerName();
                Server server = new Server()
                {
                    ServerName = name
                };
                var s = CreateServerAsync(server, client).Result;
                if (s != null && s.Id > 0)
                {
                    Properties.Settings.Default.ServerId = s.Id;
                    Properties.Settings.Default.Save();
                    ServerId = s.Id;
                }
               
            }
            catch (Exception e)
            {
                log.Info("SendServerSetup - Error: " + e);
                Thread.Sleep(5000);
                Environment.Exit(0);
            }

        }

        static async Task SendServerInfo()
        {
            HttpClient client = new HttpClient();
            AddHeaders(client);
            try
            {
                var q = Temperature.Temperatures;
                ServerDetail serverDetails = new ServerDetail
                {
                    Created = DateTime.Now,
                    CpuUtilization = Convert.ToInt32(cpuCounter.NextValue()),
                    Processes = GetProcesses(),
                    UpTime = Convert.ToInt32(upTime.NextValue()),
                    RamAvailable = Convert.ToInt32(ramCounter.NextValue()),
                    RamTotal = Convert.ToInt32(GetTotalMemoryInBytes()),
                    BytesReceived = bytesReceived,
                    BytesSent = bytesSent,
                    ServerId = ServerId,
                    Temperature = Convert.ToDecimal(q.FirstOrDefault(x => x.CurrentValue >= 0)?.CurrentValue),
                    NetworkUtilization = Convert.ToDecimal(GetNetworkUtilization(GetNetworkCard())),
                    Handles =Convert.ToDecimal(GetProcesses())
                    
                };
                bytesSent = 0;
                bytesReceived = 0;
                await CreateServerDetailAsync(serverDetails, client);
            }
            catch (Exception e)
            {
                log.Info("SendServerInfo - Error: " + e);
                Thread.Sleep(5000);
                Environment.Exit(0);
            }
        }
        private static void AddHeaders(HttpClient client)
        {
            try
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Remove("Authorization");
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            }
            catch (Exception e)
            {
                Console.WriteLine("AddHeaders - Error " + e);
                Thread.Sleep(5000);
                log.Info("AddHeaders - Error " + e);
            }
        }
        static async Task<Server> CreateServerAsync(Server server, HttpClient client)
        {
            HttpResponseMessage response = null;
            try
            {
                Console.WriteLine("Sending server...");
                response = await client.PostAsJsonAsync("api/Servers/PostServer/", server);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("Successfully sent");
                }
                else
                {
                    Console.WriteLine("CreateServerAsync - status code " + response.StatusCode);
                    log.Info("CreateServerAsync - status code " + response.StatusCode);
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("CreateServerDetailAsync - Error " + e);
                log.Info("CreateServerDetailAsync - Error " + e);
                
            }
            if (response == null) return null;
            var json = await response.Content.ReadAsStringAsync();
            var s = JsonConvert.DeserializeObject<Server>(json);

            // Return the URI of the created resource.
            return s;
        }

        static async Task<Uri> CreateServerDetailAsync(ServerDetail serverDetails, HttpClient client)
        {
            HttpResponseMessage response = null;
            try
            {
                Console.WriteLine("Sending server data...");
                response = await client.PostAsJsonAsync("api/ServerDetails/PostServerDetail/", serverDetails);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("Successfully sent information");
                }
                else
                {
                    Console.WriteLine("CreateServerDetailAsync - status code " + response.StatusCode);
                    log.Info("CreateServerDetailAsync - status code " + response.StatusCode);
                    Thread.Sleep(5000);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("CreateServerDetailAsync - Error " + e);
                log.Info("CreateServerDetailAsync - Error " + e);
                Thread.Sleep(5000);
            }

            // Return the URI of the created resource.
            return response.Headers.Location;
        }

        private static string GetServerName()
        {
            return Environment.MachineName;
        }

        private static int GetProcesses()
        {
            return Process.GetProcesses().Length;
        }

        private static int GetThreads()
        {
            return Process.GetCurrentProcess().Threads.Count;
        }

    

        private static float GetCurrentNetworkSpeed()
        {
            PerformanceCounterCategory performanceCounterCategory = new PerformanceCounterCategory("Network Interface");

            string instance = performanceCounterCategory.GetInstanceNames()[2]; //NIC
            PerformanceCounter performanceCounterSent = new PerformanceCounter("Network Interface", "Bytes Sent/sec", instance);
            // PerformanceCounter performanceCounterReceived = new PerformanceCounter("Network Interface", "Bytes Received/sec", instance);performanceCounterReceived.NextValue() / 1024
            //k\tbytes

            return performanceCounterSent.NextValue() / 1024;

        }

        private static int GetNetworkUtilization(string networkCard)
        {
            const int numberOfIterations = 10;

            PerformanceCounter bandwidthCounter = new PerformanceCounter("Network Interface", "Current Bandwidth", networkCard);
            float bandwidth = bandwidthCounter.NextValue();//valor fixo 10Mb/100Mn/
            PerformanceCounter dataSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", networkCard);

            PerformanceCounter dataReceivedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", networkCard);

            float sendSum = 0;
            float receiveSum = 0;
            for (int index = 0; index < numberOfIterations; index++)
            {
                sendSum += dataSentCounter.NextValue();
                receiveSum += dataReceivedCounter.NextValue();
            }
            float dataSent = sendSum;
            float dataReceived = receiveSum;


            //How to do this proper?
            bytesSent = Convert.ToInt32(dataSent);
            bytesReceived = Convert.ToInt32(dataReceived);


            double utilization = (8 * (dataSent + dataReceived)) / (bandwidth * numberOfIterations) * 100;
            int u = Convert.ToInt32(utilization);
            Console.WriteLine("                network :  "+ u);
            return u;
        }

        private static ulong GetTotalMemoryInBytes()
        {
            var totalPhysicalMemory = computerinfo.TotalPhysicalMemory / (1024 * 1024);
            return totalPhysicalMemory;
        }


        public static string GetNetworkCard()
        {
            string networkCard = "";
            PerformanceCounterCategory category = new PerformanceCounterCategory("Network Interface");
            String[] instancename = category.GetInstanceNames();

            foreach (string name in instancename)
            {
               // Console.WriteLine(name);
                if (name.Contains("Wireless") || name.Contains("Gigabit Network"))
                {
                    networkCard = name;
                }
            }
            return networkCard;
        }

        public static string Login(string username, string password)
        {
            log.Info("Login");
            //setup login data
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password)
            });

            //Request token
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(url);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage response = new HttpResponseMessage();

                    response = client.PostAsync("token", formContent).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;
                        var jObject = JObject.Parse(responseJson);
                        string tokenString = jObject.GetValue("access_token").ToString();
                        //get expire date, check om den er udlæbet - login igen
                        token = tokenString;
                        log.Info("Login success");
                    }
                    else
                    {
                        log.Info("Login failed - check your credentials");
                        Console.WriteLine("Exiting...");
                        Thread.Sleep(5000);
                        Environment.Exit(0);
                    }
                }
                catch (Exception e)
                {
                    log.Info("No connection to web api");
                    Console.WriteLine("Exiting...");
                    Thread.Sleep(5000);
                    Environment.Exit(0);
                }

                return token;
            }


        }

    }
}

