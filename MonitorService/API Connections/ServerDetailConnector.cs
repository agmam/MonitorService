using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MonitorService.API_Connections
{
    public static class ServerDetailConnector
    {
        public static void SendServerInfo()
        {
            HttpClient client = new HttpClient();
            APISetup.AddHeaders(client);
            try
            {
                var q = Temperature.Temperatures;
                ServerDetail serverDetails = new ServerDetail
                {
                    Created = DateTime.Now,
                    CpuUtilization = Convert.ToInt32(Program.cpuCounter.NextValue()),
                    Processes = GetProcesses(),
                    UpTime = Convert.ToInt32(Program.upTime.NextValue()),
                    RamAvailable = Convert.ToInt32(Program.ramCounter.NextValue()),
                    RamTotal = Convert.ToInt32(GetTotalMemoryInBytes()),
                    BytesReceived = Program.bytesReceived,
                    BytesSent = Program.bytesSent,
                    ServerId = Program.ServerId,
                    Temperature = Convert.ToDecimal(q.FirstOrDefault(x => x.CurrentValue >= 0)?.CurrentValue),
                    NetworkUtilization = Convert.ToDecimal(Network.GetNetworkUtilization()),
                    Handles = Convert.ToDecimal(GetProcesses())


                };
                Network.ResetNetwork();
                Program.bytesSent = 0;
                Program.bytesReceived = 0;
                var result =  CreateServerDetailAsync(serverDetails, client).Result;
            }
            catch (Exception e)
            {
                Program.log.Info("SendServerInfo - Error: " + e);
                Environment.Exit(0);
            }
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
                    Program.log.Info("CreateServerDetailAsync - status code " + response.StatusCode);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("CreateServerDetailAsync - Error " + e);
                Program.log.Info("CreateServerDetailAsync - Error " + e);
            }

            // Return the URI of the created resource.
            return response.Headers.Location;
        }
        private static int GetProcesses()
        {
            return Process.GetProcesses().Length;
        }

        private static int GetThreads()
        {
            return Process.GetCurrentProcess().Threads.Count;
        }

        private static ulong GetTotalMemoryInBytes()
        {
            var totalPhysicalMemory = Program.computerinfo.TotalPhysicalMemory / (1024 * 1024);
            return totalPhysicalMemory;
        }
    }
}
