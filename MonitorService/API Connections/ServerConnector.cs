using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MonitorService.Properties;
using Newtonsoft.Json;
using System.IO;

namespace MonitorService.API_Connections
{
    public static class ServerConnector
    {
        public static bool IsServerInDatabase(int id)
        {
            HttpClient client = new HttpClient();
            APISetup.AddHeaders(client);
            HttpResponseMessage response = null;
            try
            {
                Console.WriteLine("geting server...");
                response = client.GetAsync("api/Servers/GetServer/" + id).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("Successfully");
                }
                else
                {
                    Console.WriteLine("getServerAsync - status code " + response.StatusCode);
                    Program.log.Info("getServerAsync - status code " + response.StatusCode);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("getServerAsync - Error " + e);
                Program.log.Info("getServerAsync - Error " + e);

            }
            if (response == null || response.StatusCode == HttpStatusCode.NotFound) return false;
            return true;
        }

        public static void SetupServer()
        {
            HttpClient client = new HttpClient();
            APISetup.AddHeaders(client);

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
                    Settings.Default.ServerId = s.Id;
                    Settings.Default.Save();
                    Program.ServerId = s.Id;
                }

            }
            catch (Exception e)
            {
                Program.log.Info("SendServerSetup - Error: " + e);
                Environment.Exit(0);
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
                    Program.log.Info("CreateServerAsync - status code " + response.StatusCode);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("CreateServerDetailAsync - Error " + e);
                Program.log.Info("CreateServerDetailAsync - Error " + e);

            }
            if (response == null) return null;
            var json = await response.Content.ReadAsStringAsync();
            var s = JsonConvert.DeserializeObject<Server>(json);

            // Return the URI of the created resource.
            return s;
        }

        private static string GetServerName()
        {
            var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            var filename = @"\MonitorService.txt";
            path += filename;
            string text = File.ReadAllText(path);
            if (string.IsNullOrEmpty(text))
            {
                text = Environment.MachineName;
            }
            return text;
           
            
        }
    }
}
