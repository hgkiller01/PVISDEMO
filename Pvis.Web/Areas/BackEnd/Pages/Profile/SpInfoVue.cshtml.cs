using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using Pvis.Biz.Models;
using Pvis.Web.Helper;
using Pvis.Biz.CommEnum;
using Npoi.Mapper;
using Npoi.Mapper.Attributes;
using System.Dynamic;
using Pvis.Biz.ViewModels;
using Pvis.Biz.Extension;
using Pvis.Biz.Member;
using System.Text;

namespace Pvis.Web.Areas.BackEnd.Pages.Profile
{
    [AutoValidateAntiforgeryToken]
    [PvisAuthorize(RoleList.Admin, RoleList.Company,RoleList.Epa)]
    public class SpInfoVueModel : PageModel
    {
        [BindProperty]
        public IFormFile Upload { get; set; }
        [BindProperty]
        public int Uid { get; set; } = 0;
        [BindProperty]
        public int Pvid { get; set; }
        [BindProperty]
        public string status { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, string> PvInofos { get; set; }
        public Dictionary<string, string> CompanyList { get; set; }
        private DataDbContext _context;
        public SpInfoVueModel(DataDbContext context)
        {
            _context = context;
        }
        public void OnGet()
        {
            //CompanyList = AuthHelper.GetUserQuery().Where(x => x.Role == RoleList.Company).ToDictionary(d => d.Uid.ToString(), d => d.UserName + "(" + d.CompanyName + ")");
            PvInofos = _context.UserPvInfo.Where(x => x.Uid == User.GetUid()).ToDictionary(d => d.Pid.ToString(), d => d.Pvno);
        }
        public void OnPost()
        {
            OnGet();
            if(Pvid <= 0)
            {
                ErrorMessage = "設備選擇尚未選擇";
                return;
            }
            if (Upload != null && Upload.Length > 0)
            {
                List<UserSpInfo> spInfos = new List<UserSpInfo>();
                List<string> repeatSno = new List<string>();
                string repeatAlert = "";
                StringBuilder InfoErrorMessage = new StringBuilder("");
                using (var stream = new MemoryStream())
                {
                    IEnumerable<SpData> data = null;
                    try
                    {
                        Upload.CopyTo(stream);
                        //通過上傳檔案流初始化Mapper
                        var mapper = new Mapper(stream);
                        var spdata = mapper.Take<SpData>("太陽光電板資料維護");
                        data = spdata.Select(x => x.Value).Where(x => CheckData(x));
                        if (data == null || data.Count() == 0)
                        {
                            ErrorMessage = "Excel格式不正確";
                            return;
                        }
                        if (data.Count() > 10000)
                        {
                            ErrorMessage = "一次匯入不可超過10000筆";
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = "請上傳Excel(xlsx)";
                        var excpt = ex.Message;
                        return;
                    }
                    UserPvInfo PvInfo = _context.UserPvInfo.Where(x => x.Pid == Pvid).FirstOrDefault();
                    int SpNowCount = _context.UserSpInfo.Where(x => x.Pvid == PvInfo.Pid).Count();
                    if (data.Count() + SpNowCount > PvInfo.SpQty)
                    {
                        ErrorMessage = "滙入資料加原有資料筆數大於設備設定數量";
                        return;
                    }
                    foreach (var item in data)
                    {
                        List<string> checkResult = HasData(item);
                        if (checkResult.Count > 0)
                        {
                            ErrorMessage = "以下欄位未填寫或欄位項目文字有誤<br>";
                            ErrorMessage += string.Join("<br>", checkResult);
                            return;
                        }
                        var pvinfo = _context.UserPvInfo.Where(x => x.Pid == Pvid).FirstOrDefault();
                        if(pvinfo?.Pvno != item.設備登記編號)
                        {
                            ErrorMessage = "設備登記編號與選擇之編號不一致";
                            return;
                        }
                        UserSpInfo spInfo = new UserSpInfo()
                        {
                            Hasno = item.有無序號 == "有" ? "1" : "0",
                            Sno = item.太陽光電板序號,
                            Sbid = 0,
                            Brand = item.模組廠牌,
                            Module = item.模組型號,
                            Style = GetStyle(item.模組樣態),
                            StyleDesc = item.樣態說明,
                            Spweight = Convert.ToDecimal(item.重量),
                            Pvid = Pvid,
                            Shipno = item.出貨單號,
                            Memo = item.備註,
                            Status = item.使用狀態 == "使用中" ? "1" : "0",
                            AlFrame = item.外觀鋁框完整度 == "有鋁框" ? "1" : "0",
                            Uid = User.GetUid(),
                            Createdate = DateTime.Now
                        };
                        if (spInfos.Any(x => x.Sno == spInfo.Sno))
                        {
                            InfoErrorMessage.Append("<br>" + spInfo.Sno);
                            //ErrorMessage = "有重複的光電板序號";
                            //return;
                        }
                        if (item.有無序號 == "有")
                        {
                            if (_context.UserSpInfo.Any(x => x.Sno == spInfo.Sno))
                            {
                                InfoErrorMessage.Append("<br>"+spInfo.Sno);
                                //ErrorMessage = "有重複的光電板序號";
                                //return;
                            }
                        }
                        if (item.有無序號 == "無")
                        {
                            if (string.IsNullOrEmpty(item.備註))
                            {
                                ErrorMessage = "若無光電板序號須備註說明";
                                return;
                            }
                            if (_context.UserSpInfo.Where(x => x.Hasno == "0" && x.Uid == User.GetUid()).Any(x => x.Sno == spInfo.Sno))
                            {
                                InfoErrorMessage.Append("<br>" + spInfo.Sno);
                                //ErrorMessage = "有重複的光電板序號";
                                //return;
                            }

                        }
                        spInfos.Add(spInfo);
                    }
                }
                if (InfoErrorMessage.Length==0)
                {
                    _context.UserSpInfo.AddRange(spInfos);
                    _context.SaveChanges();
                    repeatAlert = "共匯入" + spInfos.Count + "筆資料";
                    ViewData["result"] = repeatAlert;
                }
                else
                {
                    ErrorMessage = "光電板序號重複："+InfoErrorMessage.ToString();
                }
            }
            else
            {
                ErrorMessage = "尚未選擇檔案";
            }
           
        }
        /// <summary>
        /// 是否有欄位沒填寫
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        public List<string> HasData(SpData sp)
        {
            List<string> CheckResult = new List<string>();
            if (string.IsNullOrEmpty(sp.設備登記編號))
            {
                CheckResult.Add("設備登記編號");
            }
            if (string.IsNullOrEmpty(sp.模組廠牌))
            {
                CheckResult.Add("模組廠牌");
            }
            if (string.IsNullOrEmpty(sp.有無序號))
            {
                CheckResult.Add("有無序號");
            }
            if (string.IsNullOrEmpty(sp.太陽光電板序號))
            {
                CheckResult.Add("太陽光電板序號");
            }
            if (string.IsNullOrEmpty(sp.模組型號))
            {
                CheckResult.Add("模組型號");
            }
            if (string.IsNullOrEmpty(sp.模組樣態))
            {
                CheckResult.Add("模組樣態");
            }
            if (string.IsNullOrEmpty(sp.使用狀態))
            {
                CheckResult.Add("使用狀態");
            }
            if (string.IsNullOrEmpty(sp.外觀鋁框完整度))
            {
                CheckResult.Add("外觀鋁框完整度");
            }
            if(sp.重量 <= 0)
            {
                CheckResult.Add("重量");
            }
            return CheckResult;
        }
        /// <summary>
        /// 是否為空資料
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        public bool CheckData(SpData sp)
        {
            if (string.IsNullOrEmpty(sp.模組廠牌) && string.IsNullOrEmpty(sp.有無序號) && string.IsNullOrEmpty(sp.太陽光電板序號)
            && string.IsNullOrEmpty(sp.模組型號) && string.IsNullOrEmpty(sp.模組樣態) && string.IsNullOrEmpty(sp.使用狀態)
            && string.IsNullOrEmpty(sp.外觀鋁框完整度) && sp.重量 <= 0 && string.IsNullOrEmpty(sp.設備登記編號))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public string GetStyle(string style)
        {
            switch (style)
            {
                case "矽晶單片玻璃":
                    return "1";
                case "矽晶雙片玻璃":
                    return "2";
                case "薄膜型":
                    return "3";
                case "其他":
                    return "9";
                default:
                    return "9";
            }
        }
    }
}
