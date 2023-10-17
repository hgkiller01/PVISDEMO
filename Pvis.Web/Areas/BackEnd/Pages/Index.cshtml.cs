using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Pvis.Biz.Member;

namespace Pvis.Web.Areas.BackEnd.Pages
{
    public class IndexModel : PageModel
    {

        private readonly SignInManager<MyAppUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public IndexModel(SignInManager<MyAppUser> signInManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("./Login");
            }

            return Page();

        }

    }
}
