using System.ComponentModel.DataAnnotations;

namespace Pvis.Biz.CommEnum
{
    /// <summary>
    /// 帳號申請表單分類
    /// </summary>
    public enum AppRoleList : byte
    {
        /// <summary>
        /// 尚未定義
        /// </summary>
        [Display(Name = "無")]
        none = 0,
        /// <summary>
        /// (個人)太陽光電板案場業者
        /// </summary>
        [Display(Name = "(個人)太陽光電板案場業者")]
        AppPersonal = 1,
        /// <summary>
        /// (機構)太陽光電板案場業者
        /// </summary>
        [Display(Name = "(機構)太陽光電板案場業者")]
        AppCompany = 2,
        /// <summary>
        /// 貯存業者
        /// </summary>
        [Display(Name = "貯存業者")]
        AppStore = 4,
        /// <summary>
        /// 處理業者
        /// </summary>
        [Display(Name = "處理業者")]
        AppTreat = 8
    }
    public enum FileExtension
    {
        JPG = 255216,
        GIF = 7173,
        BMP = 6677,
        PNG = 13780,
        COM = 7790,
        EXE = 7790,
        DLL = 7790,
        RAR = 8297,
        ZIP = 8075,
        XML = 6063,
        HTML = 6033,
        ASPX = 239187,
        CS = 117115,
        JS = 119105,
        TXT = 210187,
        SQL = 255254,
        BAT = 64101,
        BTSEED = 10056,
        RDP = 255254,
        PSD = 5666,
        PDF = 3780,
        CHM = 7384,
        LOG = 70105,
        REG = 8269,
        HLP = 6395,
        DOC = 208207,
        XLS = 208207,
        DOCX = 208207,
        XLSX = 208207
    }
    public enum AppStatusList : byte
    {
        /// <summary>
        /// 待審查
        /// </summary>
        [Display(Name = "待審")]
        none = 0,
        /// <summary>
        /// 通過
        /// </summary>
        [Display(Name = "通過")]
        accept = 1,
        /// <summary>
        /// 退件
        /// </summary>
        [Display(Name = "不通過")]
        reject = 2
    }
}
