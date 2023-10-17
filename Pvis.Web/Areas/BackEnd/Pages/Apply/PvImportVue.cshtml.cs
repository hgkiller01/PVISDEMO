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
    [PvisAuthorize(RoleList.Admin, RoleList.Teart, RoleList.Epa, RoleList.Store)]
    public class PvImportVue : PageModel
    {
        public PvImport PvImport { get; set; }
        public void OnGet(string No = "")
        {
            PvImport = new PvImport();
            ViewData["layout"] = "_LayoutIframe";
            ViewData["No"] = No;
        }
    }
}
