using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Member;

namespace Pvis.Web.Areas.BackEnd.Pages.Apply
{
    public class CheckReportVueModel : PageModel
    {
        public void OnGet(string No = "")
        {
            ViewData["No"] = No;
            ViewData["layout"] = "_LayoutIframe";
        }
    }
}
