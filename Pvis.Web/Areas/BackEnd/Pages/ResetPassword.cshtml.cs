using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pvis.Biz.Extension;
using System;
using System.Collections.Generic;

namespace Pvis.Web.Areas.BackEnd.Pages
{

    [AllowAnonymous]
    public class ResetPasswordModel : PageModel
    {
        public IActionResult OnGet(string code = null)
        {
            List<string> errors = new List<string>();
            string url = string.Empty;
            if (string.IsNullOrWhiteSpace(code.Decode(Exp: TimeSpan.FromHours(24) )))
            {
                errors.Add("重設連結已經逾時無法使用");
                url = Url.Content("~/BackEnd/ForgotPassword");

            }
            Input = new InputModel {
                Code = code,
                UserId = string.Empty ,
                NewPwd = string.Empty,
                ConfirmPwd = string.Empty,
                errors = errors,
                url = url
            };
            return Page();
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public string Code { get; set; }
            public string ConfirmPwd { get; set; }
            public string NewPwd { get; set; }
            public string UserId { get; set; }
            public string url;
            public List<string> errors;
        }
    }
}
