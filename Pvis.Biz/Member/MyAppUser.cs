using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Pvis.Biz.CommEnum;

namespace Pvis.Biz.Member
{
    public class MyAppUser : IdentityUser
    {
        public MyAppUser(string userName) : base(userName)
        {
           
        }

        /// <summary>
        /// 識別序號
        /// </summary>
        public int Uid { get; set; }

        /// <summary>
        /// 姓名(暱稱)
        /// </summary>
        [StringLength(250)]
        public string DisplayName { get; set; }

        /// <summary>
        /// 機構名稱
        /// </summary>
        [StringLength(250)]
        public string CompanyName { get; set; }

        public RoleList Role { get; set; }

        /// <summary>
        /// 對應 AccountApp.Pid 資訊
        /// </summary>
        public int? AppPid { get; set; }
        [StringLength(250)]
        public string Address { get; set; }
    }
}
