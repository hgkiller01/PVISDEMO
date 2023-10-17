using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npoi.Mapper;
using Npoi.Mapper.Attributes;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Member;
using Pvis.Biz.Models;
using Pvis.Web.Helper;

namespace Pvis.Web.Areas.BackEnd.Pages.Profile
{
    [ValidateAntiForgeryToken]
    [PvisAuthorize(RoleList.Admin, RoleList.Epa)]
    public class SpInfoImportVueModel : PageModel
    {
        [BindProperty]
        public IFormFile Upload { get; set; }
        [BindProperty]
        public int Uid { get; set; }
        [BindProperty]
        public int Pvid { get; set; }
        [BindProperty]
        public string status { get; set; }
        [BindProperty]
        public string PvSno { get; set; }
        [BindProperty]
        public string OriginalPvSno { get; set; }
        [BindProperty]
        public string ChangePvSno { get; set; }
        public Dictionary<string, string> CompanyList { get; set; }
        public Dictionary<string,string> PvInofos { get; set; }
        public string SearchResult { get; set; } = "";
        private DataDbContext _context;
        private ApplicationDbContext applicationDbContext;
        public SpInfoImportVueModel(DataDbContext context, ApplicationDbContext applicationDbContext)
        {
            _context = context;
            this.applicationDbContext = applicationDbContext;
        }
        public void OnGet()
        {
            CompanyList = AuthHelper.GetUserQuery().Where(x => x.Role == RoleList.Company).ToDictionary(d => d.Uid.ToString(), d => d.UserName + "(" + d.CompanyName + ")");
            PvInofos = _context.UserPvInfo.ToDictionary(x => x.Pid.ToString(), x => x.Pvno);
        }
        public void OnPostImport()
        {
            if (Upload.Length > 0)
            {
                List<UserSpInfo> spInfos = new List<UserSpInfo>();
                List<string> repeatSno = new List<string>();
                using (var stream = new MemoryStream())
                {
                    
                    Upload.CopyTo(stream);
                    //通過上傳檔案流初始化Mapper
                    var mapper = new Mapper(stream);
                    var spdata = mapper.Take<SpData>("太陽光電板資料維護");
                    var data = spdata.Select(x => x.Value);
                    foreach (var item in data)
                    {
                        var pv = _context.UserPvInfo.Where(x => x.Pvno == item.設備登記編號
                            && x.Uid == Uid).FirstOrDefault();
                        if (pv == null)
                            break;

                        if (string.IsNullOrEmpty(item.模組廠牌) || string.IsNullOrEmpty(item.有無序號) || string.IsNullOrEmpty(item.太陽光電板序號)
                            || string.IsNullOrEmpty(item.模組型號) || string.IsNullOrEmpty(item.模組樣態) || string.IsNullOrEmpty(item.使用狀態)
                            || string.IsNullOrEmpty(item.外觀鋁框完整度) || item.重量 <= 0)
                        {
                            break;
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
                            Pvid = pv.Pid,
                            Shipno = item.出貨單號,
                            Memo = item.備註,
                            Status = item.使用狀態 == "使用中" ? "1" : "0",
                            AlFrame = item.外觀鋁框完整度 == "有鋁框" ? "1" : "0",
                            Uid = Uid,
                            Createdate = DateTime.Now
                        };

                        if(item.有無序號 == "有")
                        {
                            if (_context.UserSpInfo.Any(x => x.Sno == spInfo.Sno) || spInfos.Any(x => x.Sno == spInfo.Sno))
                            {
                                repeatSno.Add(spInfo.Sno);
                                continue;
                            }                           
                        }
                        spInfos.Add(spInfo);
                    }
                }
                string repeatAlert = "";
                if (repeatSno.Count > 0)
                {
                    repeatAlert = "重複的序號如下:\\r";
                    foreach(var item in repeatSno)
                    {
                        repeatAlert += item + "\\r";
                    }
                }
                else
                {
                    _context.UserSpInfo.AddRange(spInfos);
                    _context.SaveChanges();
                    repeatAlert = "共匯入" + spInfos.Count + "筆資料";
                }
                ViewData["result"] = repeatAlert;
            }
            OnGet();
            
        }
        public void OnPostDelete()
        {
            if(Pvid > 0)
            {
                var pvinfo = _context.UserPvInfo.Where(x => x.Pid == Pvid).FirstOrDefault();
                var deleteItems = _context.UserSpInfo.Where(x => x.Pvid == Pvid);
                if (!string.IsNullOrEmpty(status))
                {
                    deleteItems = deleteItems.Where(x => x.Status == status);
                }
                _context.UserSpInfo.RemoveRange(deleteItems);
                _context.SaveChanges();
                ViewData["result"] = pvinfo.Pvno + "之下的光電板已經刪除";
            }
            OnGet();
        }
        public void OnPostGetSpAndCompany()
        {
           UserSpInfo Result =  _context.UserSpInfo.Where(x => x.Sno == PvSno).FirstOrDefault();
           if(Result != null)
           {
                var UserData = applicationDbContext.Users.Where(x => x.Uid == Result.Uid).FirstOrDefault();
                var PvData = _context.UserPvInfo.Where(x => x.Pid == Result.Pvid).FirstOrDefault();
                SearchResult = "公司名稱:"+" "+ UserData.CompanyName + " "+ "設備序號: " + PvData.Pvno;
           }
            OnGet();
        }
        public void OnPostSetPvSno()
        {
            UserSpInfo Result = _context.UserSpInfo.Where(x => x.Sno == OriginalPvSno).FirstOrDefault();
            if(Result != null)
            {
                Result.Sno = ChangePvSno;
                _context.SaveChanges();
            }
            OnGet();
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
    public class SpData
        {
            [Column("有無序號")]
            public string 有無序號 { get; set; }
            [Column("太陽光電板序號")]
            public string 太陽光電板序號 { get; set; }
            [Column("模組廠牌")]
            public string 模組廠牌 { get; set; }
            [Column("模組型號")]
            public string 模組型號 { get; set; }
            [Column("模組樣態")]
            public string 模組樣態 { get; set; }
            [Column("樣態說明")]
            public string 樣態說明 { get; set; }
            [Column("重量")]
            public decimal 重量 { get; set; }
            [Column("設備登記編號")]
            public string 設備登記編號 { get; set; }
            [Column("出貨單號")]
            public string 出貨單號 { get; set; }
            [Column("備註")]
            public string 備註 { get; set; }
            [Column("使用狀態")]
            public string 使用狀態 { get; set; }
            [Column("外觀鋁框完整度")]
            public string 外觀鋁框完整度 { get; set; }
        }
    
}
