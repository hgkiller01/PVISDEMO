﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Pvis.Web.Areas.BackEnd.Pages
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        public void OnGet()
        {

        }
    }
}
