using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Extension;

namespace Pvis.Biz.Member
{
    public class PvisAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private RoleList[] _roles;

        public PvisAuthorizeAttribute(params RoleList[] roles)
        {
            _roles = roles;
        }

        void IAuthorizationFilter.OnAuthorization(AuthorizationFilterContext context)
        {

            if (context.Filters.Any(x => x is IAllowAnonymousFilter))
            {
                return;
            }

            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (context.HttpContext.User.HasRole(_roles)) {
                return;
            }

            context.Result = new UnauthorizedResult();

        }
    }
}
