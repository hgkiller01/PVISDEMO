using System;
using System.ComponentModel.DataAnnotations;

namespace Pvis.Biz.CommEnum
{
    /// <summary>
    /// 使用者權限
    /// </summary>
    [Flags]
    public enum RoleList : Int64
    {
        [Display(Name = "系統管理者")]
        Admin = 2,
        [Display(Name = "環保署")]
        Epa = 4,
        [Display(Name = "稽核認證小組")]
        Auditor = 8,
        [Display(Name = "案場業者")]
        Company = 16,
        [Display(Name = "處理業者")]
        Teart = 32,
        [Display(Name = "貯存業者")]
        Store = 64
    }
}
