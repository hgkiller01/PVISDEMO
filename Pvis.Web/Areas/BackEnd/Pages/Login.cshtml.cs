using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Pvis.Biz.Member;
using Pvis.Web.Helper;

namespace Pvis.Web.Areas.BackEnd.Pages
{
    public class LoginModel : PageModel
    {

        private readonly SignInManager<MyAppUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly ApplicationDbContext _application;
        public Boolean IsOverFailCount
        {
            get
            {
                return AuthHelper.CheckOverFailCount(HttpContext);
            }
        }

        public object GetFailCount { get; private set; }

        public LoginModel(SignInManager<MyAppUser> signInManager, ILogger<LoginModel> logger, ApplicationDbContext application)
        {
            _signInManager = signInManager;
            _logger = logger;
            _application = application;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {

            if (!string.IsNullOrWhiteSpace(id) && id == "logout")
            {
                var userData = AuthHelper.GetCurrentUser(User);
                LoginLog log = new LoginLog()
                {
                    ActionTime = DateTime.Now,
                    ActionType = "Logout",
                    CompanyName = userData.CompanyName,
                    DisplayName = userData.CompanyName,
                    Uid = userData.Uid,
                    UserName = userData.UserName,
                    IP = Request.HttpContext.Connection.RemoteIpAddress.ToString()
                };
                _application.LoginLog.Add(log);
                _application.SaveChanges();
                await _signInManager.SignOutAsync();
                _logger.LogInformation("User logged out.");
                return RedirectToPage("./Login");
            }

            if (User.Identity.IsAuthenticated)
            {
                return RedirectToPage("./Index");
            }

            return Page();

        }
    }
}
