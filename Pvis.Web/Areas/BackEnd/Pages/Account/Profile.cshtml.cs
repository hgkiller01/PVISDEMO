using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pvis.Biz.Member;
using Pvis.Biz.Models;
using Pvis.Biz.Services;

namespace Pvis.Web.Areas.BackEnd.Pages.Account
{
    public class ProfileModel : PageModel
    {

        private readonly MemberBusinessLayer _MemberBiz;

        public ProfileModel(
            DataDbContext context,
            ApplicationDbContext appcontext,
            UserManager<MyAppUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            _MemberBiz = new MemberBusinessLayer(
                userManager,
                roleManager,
                context,
                appcontext);
        }

        public MyAppUserFormProfile Rec { get; set; }

        public async Task OnGetAsync()
        {
            Rec = await _MemberBiz.GetUserProfileAsync(User);
        }

    }
}
