using System;
using System.ComponentModel.DataAnnotations;

namespace Pvis.Biz.CommEnum
{
    /// <summary>主分類</summary>
    public enum eDocType
    {
        [Display(Name = "未定義")]
        None = 0,
        /// <summary>
        /// 最新消息
        /// </summary>
        [Display(Name = "最新消息")]
        NewsDoc = 1,
        /// <summary>
        /// 案場排出登記
        /// </summary>
        [Display(Name = "案場排出登記")]
        ScrapBookingDoc = 2 ,
        /// <summary>
        /// 帳號申請
        /// </summary>
        [Display(Name = "帳號申請")]
        AccountAppDoc=3,
        /// <summary>
        /// 太陽光電板附件
        /// </summary>
        [Display(Name = "太陽光電板附件")]
        UserSpInfoDoc = 4,
        /// <summary>
        /// 設備附件
        /// </summary>
        [Display(Name = "設備附件")]
        UserPvInfoDoc = 5,
        /// <summary>
        /// 清理行程
        /// </summary>
        [Display(Name = "清理行程")]
        ScheduleDoc = 6,
        /// <summary>
        /// 稽核量
        /// </summary>
        [Display(Name = "稽核量")]
        ProofSingle = 7

    }

    public enum eItemType
    {
        [Display(Name = "無")]
        None = 0 ,
        /// <summary>
        /// 帳號申請-授權委託書
        /// </summary>
        [Display(Name = "授權委託書")]
        AccountApp_LetterOfProxy=1,
        /// <summary>
        /// 帳號申請/設備附件-設備登記同意函
        /// </summary>
        [Display(Name = "設備登記同意函")]
        AccountApp_LetterOfAgreement=2,
        /// <summary>
        /// 帳號申請-貯存場設置核准函
        /// </summary>
        [Display(Name = "貯存場設置核准函")]
        AccountApp_ApprovalOfInstallations=3,
        /// <summary>
        /// 申請表
        /// </summary>
        [Display(Name = "申請表")]
        ApplyDoc = 4,
        /// <summary>
        /// 證明文件
        /// </summary>
        [Display(Name = "證明文件")]
        ProvDoc = 5,
        /// <summary>
        /// 清除機構文件
        /// </summary>
        [Display(Name = "清除機構文件")]
        CleDoc = 6,
        /// <summary>
        /// 處理機構文件
        /// </summary>
        [Display(Name = "處理機構文件")]
        TreDoc = 7,
        /// <summary>
        /// 案場排出表稽核結果
        /// </summary>
        [Display(Name = "案場排出表稽核結果")]
        AuditDoc = 8,
        /// <summary>
        /// 裝置容量證明文件-太陽光電模組序號
        /// </summary>
        [Display(Name = "裝置容量證明文件")]
        PvSnDoc = 9,

    }
}
