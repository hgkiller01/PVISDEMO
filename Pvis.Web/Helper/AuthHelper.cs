using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Pvis.Biz.Extension;
using Pvis.Biz.Member;
using Pvis.Biz.Models;
using Pvis.Biz.Services;
using System.Collections;
using System.Collections.Generic;
using Pvis.Biz.CommEnum;

namespace Pvis.Web.Helper
{
    public static class AuthHelper
    {
        private static IServiceScope _scope;

        private static TimeSpan defaultCacheTime = TimeSpan.FromMinutes(15);

        private const string CacheKeyCompany = "CurrentCompany";

        private const string CacheKeyUser = "CurrentUser";

        private static DataDbContext dataDbcontext { get; set; }

        private static ApplicationDbContext appContext { get; set; }
        private static UserManager<MyAppUser> userManager { get; set; }
        private static IMemoryCache memoryCache { get; set; }

        /// <summary>
        /// 初始化需要的 _applicationServices 物件
        /// </summary>
        /// <param name="_applicationServices"></param>
        /// <param name="_defaultCacheTime"></param>
        internal static void Init(IServiceProvider _applicationServices, TimeSpan _defaultCacheTime)
        {
            _scope = _applicationServices.CreateScope();

            /*
             追蹤與不追蹤的查詢
             https://docs.microsoft.com/zh-tw/ef/core/querying/tracking
             */

            dataDbcontext = _scope.ServiceProvider.GetService<DataDbContext>();

            dataDbcontext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            appContext = _scope.ServiceProvider.GetService<ApplicationDbContext>();

            appContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            userManager = _scope.ServiceProvider.GetService<UserManager<MyAppUser>>();

            memoryCache = _scope.ServiceProvider.GetService<IMemoryCache>();
        }

        public static string GetCompanyName(this ClaimsPrincipal _user)
        {
            var _u = _user.GetCurrentUser();
            if (_u is null) return "匿名訪客";
            return _u.CompanyName;
        }

        /// <summary>
        /// 取得目前使用者的 GravatarUrl
        /// </summary>
        /// <param name="_user"></param>
        /// <returns></returns>
        public static string GetGravatarUrl(this ClaimsPrincipal _user)
        {
            return "//www.gravatar.com/avatar/" + _user.GetCurrentUser().Email.ToLower().Trim().ToMD5String();
        }

        /// <summary>
        /// 取得目前使用者完整資訊
        /// </summary>
        /// <param name="_user"></param>
        /// <returns></returns>
        public static MyAppUser GetCurrentUser(this ClaimsPrincipal _user)
        {

            if (!_user.Identity.IsAuthenticated) return new MyAppUser("guest");

            MyAppUser CurrentUser = memoryCache.Get<MyAppUser>(CacheKeyUser + _user.GetUid());
            if (CurrentUser != null) return CurrentUser;

            CurrentUser = appContext.Users.Where(x => x.Uid == Convert.ToInt32(_user.GetUid())).FirstOrDefault();

            if (CurrentUser != null) memoryCache.Set<MyAppUser>(CacheKeyUser + CurrentUser.Uid, CurrentUser, defaultCacheTime);

            return CurrentUser;
        }
        public static List<MyAppUser> GetUserList()
        {
            return appContext.Users.ToList();
        }
        public static IQueryable<MyAppUser> GetUserQuery()
        {
            return appContext.Users;
        }
        public static List<MyAppUser> GetUserListByType(RoleList role, RoleList? role2 = null)
        {
            if (role2.HasValue)
            {
                return appContext.Users.Where(x => x.Role == role || x.Role == role2).ToList();
            }
            else
            {
                return appContext.Users.Where(x => x.Role == role).ToList();
            }
            
        }
        /// <summary>
        /// 紀錄登入失敗次數
        /// </summary>
        /// <param name="httpContext"></param>
        internal static byte LogFailCount(HttpContext httpContext)
        {
            var count = memoryCache.Get<byte?>(GetFailCountCacheKey(httpContext));
            count = (byte)((count ?? 0) + 1);
            memoryCache.Set<byte?>(GetFailCountCacheKey(httpContext), count, TimeSpan.FromMinutes(10));
            return count.Value;
        }

        /// <summary>
        /// 取得登入失敗次數紀錄
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        internal static byte GetFailCount(HttpContext httpContext)
        {
            var count = memoryCache.Get<byte?>(GetFailCountCacheKey(httpContext));
            return (count ?? 0);
        }

        /// <summary>
        /// 清除登入失敗計數器
        /// </summary>
        /// <param name="httpContext"></param>
        internal static void ClearFailCount(HttpContext httpContext)
        {
            memoryCache.Remove(GetFailCountCacheKey(httpContext));
        }

        /// <summary>
        /// 判定是否超出連續失敗次數上限
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        internal static bool CheckOverFailCount(HttpContext httpContext)
        {
            return GetFailCount(httpContext) >= 5;
        }

        private static string GetFailCountCacheKey(HttpContext httpContext)
        {
            return httpContext.Connection.RemoteIpAddress.ToString() + "LogFailCount";
        }

        /// <summary>
        /// 取得目前登入者對應的申報單位
        /// </summary>
        /// <param name="_user"></param>
        /// <returns></returns>
        public static AccountApp GetCurrentCompany(this ClaimsPrincipal _user)
        {

            if (!_user.Identity.IsAuthenticated) return new AccountApp();

            if (_user.GetAppPid() <= 0) return new AccountApp();

            AccountApp CurrentCompany = memoryCache.Get<AccountApp>(CacheKeyCompany + _user.GetAppPid());

            if (CurrentCompany != null) return CurrentCompany;

            CurrentCompany = dataDbcontext.AccountApp.Where(x => x.Pid == _user.GetAppPid()).FirstOrDefault();

            if (CurrentCompany != null) memoryCache.Set<AccountApp>(CacheKeyCompany + CurrentCompany.Pid, CurrentCompany, defaultCacheTime);

            return CurrentCompany;
        }

        /// <summary>
        /// 清除相關 Cache 資料
        /// </summary>
        /// <param name="_user"></param>
        public static void ClearCache(this ClaimsPrincipal _user)
        {

            if (!_user.Identity.IsAuthenticated) return;

            memoryCache.Remove(CacheKeyUser + _user.GetUid());

            if (_user.GetAppPid() > 0) memoryCache.Remove(CacheKeyCompany + _user.GetAppPid());
        }

        internal static void ClearCache(this MyAppUserForm _user)
        {
            memoryCache.Remove(CacheKeyUser + _user.Uid);
            if (_user.AppPid > 0) memoryCache.Remove(CacheKeyCompany + _user.AppPid );
        }


    }
}
