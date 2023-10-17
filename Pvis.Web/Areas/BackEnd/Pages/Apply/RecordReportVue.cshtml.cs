using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Member;
using Pvis.Biz.Models;

namespace Pvis.Web.Areas.BackEnd.Pages.Apply
{
    public class RecordReportVueModel : PageModel
    {
        public Record record { get; set; }
        public RecordItem recordItem { get; set; }
        public void OnGet(string No = "")
        {
            record = new Record();
            recordItem = new RecordItem();
            ViewData["No"] = No;
            ViewData["layout"] = "_LayoutIframe";
        }
    }
}
