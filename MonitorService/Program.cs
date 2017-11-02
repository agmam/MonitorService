﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.Devices;
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

        static void Main(string[] args)
        {
            upTime = new PerformanceCounter("System", "System Up Time");
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue(); //always 0
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            computerinfo = new ComputerInfo();
            Console.WriteLine("Main...");

            Login(Username, Password);
            while (true)
            {
                Thread.Sleep(6000);
                SendServerInfo();
            }
        }

        static async Task SendServerInfo()
        {
            HttpClient client = new HttpClient();
            AddHeaders(client);
            try
            {
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
                    Server = new Server() { ServerName  = GetServerName() }
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

        static async Task<Uri> CreateServerDetailAsync(ServerDetail serverDetails, HttpClient client)
        {
            HttpResponseMessage response = null;
            try
            {
                Console.WriteLine("Sending server data...");
                response = await client.PostAsJsonAsync("api/ServerDetails", serverDetails);
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
                Console.WriteLine(name);
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

