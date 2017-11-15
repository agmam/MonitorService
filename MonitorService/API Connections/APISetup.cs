using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MonitorService.API_Connections
{
    public class APISetup
    {
        static string url = Properties.Settings.Default.WebApiUrl;
        public static void AddHeaders(HttpClient client)
        {
            try
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Remove("Authorization");
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Login.token);
            }
            catch (Exception e)
            {
                Console.WriteLine("AddHeaders - Error " + e);
                Program.log.Info("AddHeaders - Error " + e);
            }
        }
    }
}
