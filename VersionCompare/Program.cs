using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32;

namespace VersionCompare
{
    public class Program
    {
        static string _url;

        public static void Main(string[] args)
        {
            LoadUrl();
            Task.Run(() =>
            {
                System.Threading.Thread.Sleep(2000);
                OpenIndexPage();
            });

            if (portInUse(new Uri(_url).Port))
            {
                OpenIndexPage();
            }
            else
            {
                CreateHostBuilder(args).Build().Run();
            }
        }

        private static void LoadUrl()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var config = builder.Build();
            _url = config.GetValue<string>("WebUrl");
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls(_url);
                });

        private static void OpenIndexPage()
        {
            try
            {
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"http\shell\open\command\");
                string s = key.GetValue("").ToString();
                s = s.Substring(0, s.IndexOf('"', 1) + 1);
                Console.WriteLine(s);
                System.Diagnostics.Process.Start(s, _url);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static bool portInUse(int port)
        {
            bool flag = false;
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipendpoints = properties.GetActiveTcpListeners();
            foreach (IPEndPoint ipendpoint in ipendpoints)
            {
                if (ipendpoint.Port == port)
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }
    }
}
