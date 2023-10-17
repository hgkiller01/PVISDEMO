using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Pvis.Web.Areas.BackEnd.Pages.Review
{
    [ValidateAntiForgeryToken]
    public class ProofSingleReportVueModel : PageModel
    {
        public void OnPost(string No,string ProofNo)
        {
            ViewData["layout"] = "_LayoutIframe";
            ViewData["Aud_Sch_No"] = No;
            ViewData["ProofNo"] = ProofNo;
        }
    }
}
