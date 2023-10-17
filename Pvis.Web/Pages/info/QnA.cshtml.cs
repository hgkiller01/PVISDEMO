using System.IO;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Pvis.Web.ViewModel;

namespace Pvis.Web.Pages.info
{
    public class QnAModel : PageModel
    {
        public QnAVM Data { get; set; }

        public void OnGet()
        {
            try
            {
                using (var sr = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mgr", "qna.json")))
                {

                    Data = JsonConvert.DeserializeObject<QnAVM>(sr.ReadToEnd());
                }

            }
            catch
            {
                Data = new QnAVM();
            }
        }
    }
}
