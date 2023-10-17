using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Pvis.Biz.Member
{
    public class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<MyAppUser, IdentityRole>
    {
        public AppClaimsPrincipalFactory(
            UserManager<MyAppUser> userManager
            , RoleManager<IdentityRole> roleManager
            , IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
        { }

        public async override Task<ClaimsPrincipal> CreateAsync(MyAppUser user)
        {
            var principal = await base.CreateAsync(user);

            if (user.Uid >= 0)
            {
                ((ClaimsIdentity)principal.Identity)
                    .AddClaims(new[] {
                        new Claim(ClaimTypes.Sid,user.Uid.ToString()) ,
                        new Claim(ClaimTypes.GivenName,user.DisplayName) ,
                        new Claim(ClaimTypes.GroupSid,$"{user.AppPid}")
                    });
            }

            return principal;
        }
    }
}
