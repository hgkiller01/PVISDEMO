using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Member;

namespace Pvis.Web.Areas.BackEnd.Pages.Download
{
    [Authorize]
    public class ManualModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
