using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace Pvis.Web.Helper
{
    public class ExceptionFilter : IAsyncExceptionFilter
    {
        public Task OnExceptionAsync(ExceptionContext context)
        {
            var TraceId = context.HttpContext.TraceIdentifier;
            Console.WriteLine($"TraceId:{TraceId}");
            Console.WriteLine($"IP來源:{context.HttpContext.Connection.RemoteIpAddress.ToString()}");
            Console.WriteLine($"時間:{DateTime.Now}");
            Console.Write(JsonConvert.SerializeObject(context.Exception));
            Console.Write("\r\n");
            /*
             網路參考資料: 
                 [鐵人賽 Day17] ASP.NET Core 2 系列 - 例外處理 (Exception Handler)
                 https://blog.johnwu.cc/article/ironman-day17-asp-net-core-exception-handler.html
             */

            //TODO: 例外紀錄寫入尚未實做

            return Task.CompletedTask;
        }
    }
}
