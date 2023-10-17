using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Member;

namespace Pvis.Web.Areas.BackEnd.Pages.Review
{
    [PvisAuthorize(RoleList.Admin, RoleList.Auditor, RoleList.Epa)]
    public class ProofSingleVueModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
