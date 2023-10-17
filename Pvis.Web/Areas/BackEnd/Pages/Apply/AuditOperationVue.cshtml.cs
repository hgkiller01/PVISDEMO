﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Member;

namespace Pvis.Web.Areas.BackEnd.Pages.Apply
{
    [PvisAuthorize(RoleList.Admin, RoleList.Teart, RoleList.Epa, RoleList.Store)]
    public class AuditOperationVue : PageModel
    {
        public void OnGet()
        {

        }
    }
}
