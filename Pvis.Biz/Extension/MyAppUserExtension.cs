using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Pvis.Biz.CommEnum;

namespace Pvis.Biz.Extension
{
    public static class MyAppUserExtension
    {
        /// <summary>
        /// 取得目前使用者 Uid
        /// </summary>
        /// <param name="_user"></param>
        /// <returns></returns>
        public static int GetUid(this ClaimsPrincipal _user)
        {
            if (!_user.Identity.IsAuthenticated) return -1;
            int _Uid = 0;
            int.TryParse(_user.FindFirst(ClaimTypes.Sid).Value, out _Uid);
            return _Uid;
        }

        public static string GetDisplayName(this ClaimsPrincipal _user)
        {
            if (!_user.Identity.IsAuthenticated) return "未登入";
            return _user.FindFirst(ClaimTypes.GivenName)?.Value ?? _user.Identity.Name;
        }

        /// <summary>
        /// 取得目前使用者對應的申報機構 AppPid
        /// </summary>
        /// <param name="_user"></param>
        /// <returns></returns>
        public static int GetAppPid(this ClaimsPrincipal _user)
        {
            if (!_user.Identity.IsAuthenticated) return -1;
            int.TryParse(_user.FindFirst(ClaimTypes.GroupSid)?.Value ?? "-1", out int _AppPid);
            return _AppPid;
        }

        /// <summary>
        /// 取得目前使用者 權限清單
        /// </summary>
        /// <param name="_user"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetRoles(this ClaimsPrincipal _user)
        {
            if (!_user.Identity.IsAuthenticated) return null;
            return _user.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList();
        }


        /// <summary>
        /// 檢查使用者是否符合特定權限
        /// </summary>
        /// <param name="_user"></param>
        /// <param name="_roles"></param>
        /// <returns></returns>
        public static Boolean HasRole(this ClaimsPrincipal _user, params RoleList[] _roles)
        {
            if (_roles == null) return false;
            return _roles.Any(x => _user.IsInRole(x.ToString()));
        }

    }
}
