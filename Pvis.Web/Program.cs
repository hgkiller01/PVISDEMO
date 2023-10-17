using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Pvis.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                /// 這段如果榜定 IIS 似乎會導致網站爆錯無法運行
                //.UseKestrel(options => options.AddServerHeader = false)
                .UseStartup<Startup>().UseIIS();//.UseUrls("http://0.0.0.0:5000;https://0.0.0.0:5001")
                //.UseKestrel();
    }
}
