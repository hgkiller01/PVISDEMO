using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Pvis.Web.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel : PageModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public ActionResult OnGet()
        {
            return ErrorHandling();
        }

        public ActionResult OnPost()
        {
            return ErrorHandling();
        }

        private ActionResult ErrorHandling()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            if (HttpContext.Request.ContentType == null || !HttpContext.Request.ContentType.StartsWith("application/json;"))
            {
                return Page();
            }
            else
            {
                return BadRequest(new
                {
                    errors = new List<string>() { $"系統發生異常事件編號:{RequestId}" }
                });
            }
        }


    }
}
