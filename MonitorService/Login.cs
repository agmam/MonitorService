using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MonitorService
{
    public static class Login
    {
        public static string token = "";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static string url = Properties.Settings.Default.WebApiUrl;
        public static string TryLogin(string username, string password)
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
                    Console.WriteLine(response.StatusCode);
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
                        Console.WriteLine(" Login failed exiting...");
                        Thread.Sleep(5000);
                        Environment.Exit(0);
                    }
                }
                catch (Exception e)
                {
                    log.Info("No connection to web api");
                    Console.WriteLine("error login failed Exiting... : " +e);
                    Thread.Sleep(5000);
                    Environment.Exit(0);
                }

                return token;
            }
        }
    }
}
