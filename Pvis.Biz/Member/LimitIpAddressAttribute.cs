using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Pvis.Biz.Member
{
    public class LimitSrcIpAttribute : Attribute, IAuthorizationFilter
    {
        private string[] _IpList;

        public LimitSrcIpAttribute(params string[] IpList)
        {
            _IpList = IpList;
        }

        void IAuthorizationFilter.OnAuthorization(AuthorizationFilterContext context)
        {
            if (_IpList == null)
            {
                return;
            }

            if (_IpList.Any(x => IPAddress.Parse(x).Equals(context.HttpContext.Connection.RemoteIpAddress)))
            {
                return;
            }
            context.Result = new UnauthorizedResult();
        }
    }
}
