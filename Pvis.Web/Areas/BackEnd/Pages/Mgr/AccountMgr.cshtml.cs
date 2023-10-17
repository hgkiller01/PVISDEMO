using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pvis.Biz.Services;

namespace Pvis.Web.Areas.BackEnd.Pages.Mgr
{
    public class AccountMgrModel : PageModel
    {
        public readonly Dictionary<string, string> MemberRoleCode = MemberBusinessLayer.MemberRoleCode;
    }
}
