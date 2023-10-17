using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Member;
using Pvis.Biz.Models;
using Pvis.Biz.Extension;
using Microsoft.JSInterop;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Pvis.Web.Areas.BackEnd.Pages.Review
{
    [Authorize(Roles = "Admin,Epa,Auditor")]
    public partial class CompareVue
    {
        [Inject]
        public IJSRuntime JS { get; set; }
        [Inject]
        public DataDbContext context { get; set; }
        [Inject]
        public IHttpContextAccessor httpContext { get; set; }
        [Inject]
        public NavigationManager MyNavigationManager { get; set; }
        [Inject]
        public AuthenticationStateProvider Authenticationstateprovider { get; set; }
        /// <summary>
        /// 顯示掃碼功能
        /// </summary>
        public bool ShowScanBarcode { get; set; } = false;
        /// <summary>
        /// 主表目前頁數
        /// </summary>
        public int MainNowPage { get; set; } = 1;
        /// <summary>
        /// 副表目前頁數
        /// </summary>
        public int SubNowPage { get; set; } = 1;
        /// <summary>
        /// 顯示結果視窗
        /// </summary>
        public bool ShowResultModal { get; set; } = false;
        /// <summary>
        /// 掃讀,手動太陽能版編號
        /// </summary>
        public string ScanResult { get; set; }
        /// <summary>
        /// 公司名稱
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// 比對結果(主檔)
        /// </summary>
        public bool? CompareStatus { get; set; }
        /// <summary>
        /// 主檔資料數量
        /// </summary>
        public int MainTotalCount { get; set; }
        /// <summary>
        /// 掃碼資料數量
        /// </summary>
        public int SubTotalCount { get; set; }
        /// <summary>
        /// 是否手動輸入條碼
        /// </summary>
        public bool IFedit { get; set; } = false;
        /// <summary>
        /// 1:一致 2:不一致 3:尚未比對
        /// </summary>
        public string status { get; set; }
        /// <summary>
        /// 比較結果
        /// </summary>
        public bool? CompareResultStatus { get; set; }
        /// <summary>
        /// 目前選擇主檔
        /// </summary>
        public CompareVm comparevm { get; set; }
        /// <summary>
        /// 主檔List
        /// </summary>
        public List<CompareVm> Comparedata { get; set; }
        /// <summary>
        /// 副檔List
        /// </summary>
        public List<CompareVm2> compare_Details { get; set; }
        /// <summary>
        /// 所有副檔List
        /// </summary>
        public IQueryable<CompareVm2> compare_all { get; set; }
        /// <summary>
        /// 比對後一致的太陽能版流水號
        /// </summary>
        public int? compareSpid { get; set; }
        /// <summary>
        /// 目前位置 Main:主檔 Sub:副檔
        /// </summary>
        public string Action { get; set; } = "Main";
        /// <summary>
        /// 分頁檔案數
        /// </summary>
        public int Pagesize { get; set; } = 10;
        /// <summary>
        /// 輸入是否為空字串
        /// </summary>
        public bool ReqStr { get; set; } = false;
        /// <summary>
        /// 判斷是否掃過碼
        /// </summary>
        public bool AfterScan { get; set; } = false;
        /// <summary>
        /// 更改掃碼結果
        /// </summary>
        public bool ShowChangeCode { get; set; } = false;
        /// <summary>
        /// 暫存條碼副檔
        /// </summary>
        public CompareVm2 CodeData { get; set; }
        /// <summary>
        /// 是否有輸入條碼
        /// </summary>
        public bool HasInputCode { get; set; } = false;
        /// <summary>
        /// 條碼是否重複
        /// </summary>
        public bool isRight { get; set; } = false;
        /// <summary>
        /// 暫存太陽能板資料
        /// </summary>
        public CompareVm2 SnoData { get; set; }
        public AuthenticationState authState { get; set; }
        protected override async Task OnInitializedAsync()
        {
            authState = await Authenticationstateprovider.GetAuthenticationStateAsync();
            await ChangeMainData(Pagesize, 0);        
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JS.InvokeVoidAsync("after");
        }
        public async Task ChangeMainData(int pagesize, int nowpage, string CompanyName = "", bool? Compare_State = null)
        {
            var TempCompare = (from ssb in context.ScheduleAudit_SB
                               join sca in context.ScheduleAudit on ssb.Aud_Sch_No equals sca.Aud_Sch_No
                               join sb in context.ScrapBooking on ssb.Bookingno equals sb.Bookingno
                               where sca.Aud_State == "Y1"
                               select new CompareVm()
                               {
                                   Aud_Sch_No = sca.Aud_Sch_No,
                                   bookingno = sb.Bookingno,
                                   contact = sb.Contact,
                                   BookingCount = context.Compare_Detail.Where(x => x.bookingno == sb.Bookingno).Count(),
                                   SnoCount = context.UserSpInfo.Where(x => x.Sbid == sb.Pid).Count(),
                                   Compare_State = ssb.Compare_State,
                                   CompanyName = sca.CompanyName
                               });
            if (!string.IsNullOrEmpty(CompanyName))
                TempCompare = TempCompare.Where(x => x.CompanyName.Contains(CompanyName));
            if (Compare_State.HasValue)
            {
                if (Compare_State.Value)
                {
                    TempCompare = TempCompare.Where(x => x.Compare_State == Compare_State.Value);
                }
                else
                {
                    TempCompare = TempCompare.Where(x => x.Compare_State == null || x.Compare_State == false);
                }
            }

            MainTotalCount = TempCompare.Count();
            Comparedata = await TempCompare.Skip(nowpage * pagesize).Take(pagesize).ToListAsync();
        }
        public void ChangeSubData(string bookingno, int pagesize, int nowpage)
        {
            //找出所以尚未比對
            var tempCompare = (from sp in context.UserSpInfo
                               join sb in context.ScrapBooking
                               on sp.Sbid equals sb.Pid
                               where sb.Bookingno == bookingno
                               select new CompareVm2()
                               {
                                   Sno = sp.Sno,
                                   Compare_Sno = context.Compare_Detail.Where(x => x.Spid == sp.Pid).FirstOrDefault().Compare_Sno,
                                   Status = context.Compare_Detail.Where(x => x.Spid == sp.Pid).FirstOrDefault().status,
                                   Al_frame = context.Compare_Detail.Where(x => x.Spid == sp.Pid).FirstOrDefault().Al_frame,
                                   SpAl_frame = sp.AlFrame,
                                   Spid = sp.Pid,
                                   CDID = null
                               });
            //找出所有一致與不一致並且合拼
            var data = (from Cd in context.Compare_Detail
                        join sp in context.UserSpInfo on Cd.Spid equals sp.Pid
                        into csp
                        from result2 in csp.DefaultIfEmpty()
                        where Cd.bookingno == bookingno
                        select new CompareVm2()
                        {
                            Sno = result2.Sno,
                            Compare_Sno = Cd.Compare_Sno,
                            Status = Cd.status,
                            Al_frame = Cd.Al_frame,
                            SpAl_frame = result2.AlFrame,
                            Spid = result2.Pid,
                            CDID = Cd.CDID
                        }).Where(x => !(x.Sno == x.Compare_Sno)).Union(tempCompare);

            compare_all = data;
            switch (status)
            {
                case "1":
                    data = data.Where(x => x.Status == true);
                    break;
                case "2":
                    data = data.Where(x => x.Status == false);
                    break;
                case "3":
                    data = data.Where(x => !x.Status.HasValue);
                    break;

            }
            SubTotalCount = data.Count();


            compare_Details = data.Skip(nowpage * pagesize).Take(pagesize).OrderBy(x => x.Status).ToList();
        }
        public string CompareSno()
        {
            var havedata = context.Compare_Detail.Where(x => x.Compare_Sno == ScanResult).FirstOrDefault();
            if (havedata != null)
            {
                CompareResultStatus = null;
                return "此序號已刷讀!";
            }
            UserSpInfo userSpInfo = (from sb in context.ScrapBooking
                                     join sp in context.UserSpInfo on sb.Pid equals sp.Sbid
                                     where sp.Sno == ScanResult && sb.Bookingno == comparevm.bookingno
                                     select sp
                               ).FirstOrDefault();
            if (userSpInfo != null)
            {
                compareSpid = userSpInfo.Pid;
                CompareResultStatus = true;
                return "序號一致 目前刷讀序號為 " + ScanResult;
            }
            else
            {
                compareSpid = null;
                CompareResultStatus = false;
                return "序號不一致  目前刷讀序號為 " + ScanResult;
            }
        }
        public async Task AddCompareDetail(string alframe)
        {
            if (IFedit)
            {
                if (string.IsNullOrEmpty(ScanResult))
                {
                    ReqStr = true;
                    return;
                }
                else
                {
                    string result = CompareSno();
                    await JS.InvokeVoidAsync("showAlert", result);
                    ReqStr = false;
                    if (result == "此序號已刷讀")
                        return;
                }

            }
            ShowResultModal = false;
            var addData = new Compare_detail()
            {
                Al_frame = alframe,
                CompareTime = DateTime.Now,
                Compare_Sno = ScanResult,
                CreateUser = authState.User.GetUid(),
                status = CompareResultStatus,
                Aud_Sch_No = comparevm.Aud_Sch_No,
                bookingno = comparevm.bookingno,
                Spid = compareSpid
            };
            context.Compare_Detail.Add(addData);
            context.SaveChanges();
            await JS.InvokeVoidAsync("closeModal");
            ScanResult = "";
            CompareResultStatus = null;
            AfterScan = false;
            SubNowPage = 1;
            GetSubPage(SubNowPage);
        }
        public async Task GetMainPage(int page)
        {
            MainNowPage = page;
            await ChangeMainData(Pagesize, page - 1);
        }
        public void GetSubPage(int page)
        {
            SubNowPage = page;
            ChangeSubData(comparevm.bookingno, Pagesize, page - 1);
        }
        public void ChangeSubPage(CompareVm detail, int nowpage,string action)
        {
            Action = action;
            this.comparevm = detail;
            status = "";
            ChangeSubData(this.comparevm.bookingno, 10, nowpage - 1);
        }
        public async Task SearchMain()
        {
            Action = "Main";
            MainNowPage = 1;
            await ChangeMainData(Pagesize, 0, CompanyName, CompareStatus);
        }
        public void SearchSub()
        {
            SubNowPage = 1;
            ChangeSubData(comparevm.bookingno, Pagesize, 0);
        }
        public void ChangeCode()
        {
            if (CodeData.CDID.HasValue)
            {
                if (string.IsNullOrEmpty(CodeData.Compare_Sno))
                {
                    HasInputCode = true;
                    return;
                }
                var repeatCode = context.Compare_Detail
                         .Where(x => x.Compare_Sno == CodeData.Compare_Sno && x.CDID != CodeData.CDID).FirstOrDefault();
                if (repeatCode != null)
                {
                    isRight = true;
                    return;
                }

                var spdata = context.UserSpInfo.Where(x => x.Sno == CodeData.Compare_Sno).FirstOrDefault();
                var changeCompare = context.Compare_Detail.Find(CodeData.CDID);
                if (spdata != null)
                {
                    changeCompare.Spid = spdata.Pid;
                    changeCompare.status = true;
                }
                changeCompare.Compare_Sno = CodeData.Compare_Sno;
                changeCompare.Al_frame = CodeData.Al_frame;
            }
            CodeData = null;
            context.SaveChanges();
            SubNowPage = 1;
            GetSubPage(SubNowPage);
        }
        public void DeleteCode(int? CDID)
        {
            if (CDID.HasValue)
            {
                var removeData = context.Compare_Detail.Find(CDID);
                context.Compare_Detail.Remove(removeData);
                context.SaveChanges();
            }
            SubNowPage = 1;
            GetSubPage(SubNowPage);
        }
        public void ChangeSno()
        {
            if (SnoData.Spid.HasValue)
            {
                if (string.IsNullOrEmpty(SnoData.Sno))
                {
                    HasInputCode = true;
                    return;
                }
                var repeatSno = context.UserSpInfo
                   .Where(x => x.Sno == SnoData.Sno && x.Pid != SnoData.Spid).FirstOrDefault();
                if (repeatSno != null)
                {
                    isRight = true;
                    return;
                }

                var getSnoData = context.UserSpInfo.Find(SnoData.Spid.Value);
                Compare_CLog log = new Compare_CLog()
                {
                    Sno = getSnoData.Sno,
                    Change_Sno = SnoData.Sno,
                    Al_frame = getSnoData.AlFrame,
                    Change_frame = SnoData.Al_frame,
                    CreateDate = DateTime.Now,
                    Spid = getSnoData.Pid,
                    Uid = authState.User.GetUid()
                };
                context.Compare_CLog.Add(log);
                getSnoData.AlFrame = SnoData.SpAl_frame;
                getSnoData.Sno = SnoData.Sno;
                var compareCodeData = context.Compare_Detail.Where(x => x.Compare_Sno == SnoData.Sno).FirstOrDefault();
                if (compareCodeData != null)
                {
                    compareCodeData.Spid = SnoData.Spid;
                    compareCodeData.Compare_Sno = SnoData.Sno;
                    compareCodeData.status = true;
                }

            }
            context.SaveChanges();
            SnoData = null;
            HasInputCode = false;
            isRight = false;
            SubNowPage = 1;
            GetSubPage(SubNowPage);
        }
        public void Scan(string ResultStr)
        {
            ShowScanBarcode = !ShowScanBarcode;
            ScanResult = ResultStr;
            ShowResultModal = true;
            AfterScan = true;
            JS.InvokeVoidAsync("showModal");
        }
        public async Task CompareComplete()
        {
            var ScaSb = context.ScheduleAudit_SB.Where(x => x.Aud_Sch_No == comparevm.Aud_Sch_No &&
            x.Bookingno == comparevm.bookingno).FirstOrDefault();
            ScaSb.Compare_State = true;
            context.SaveChanges();
            Action = "Main";
            await ChangeMainData(Pagesize, 0, CompanyName, CompareStatus);
        }
        public class CompareVm
        {
            public string CompanyName { get; set; }
            public string Aud_Sch_No { get; set; }
            public string bookingno { get; set; }
            public string contact { get; set; }
            public int BookingCount { get; set; }
            public int SnoCount { get; set; }
            public bool? Compare_State { get; set; }

        }
        public class CompareVm2
        {
            public object Clone()
            {
                return this.MemberwiseClone();
            }
            public string Sno { get; set; }
            public string SpAl_frame { get; set; }
            public string Compare_Sno { get; set; }
            public string Al_frame { get; set; }
            public bool? Status { get; set; }
            public int? Spid { get; set; }
            public int? CDID { get; set; }
        }
        public class CompareResult
        {
            public int CDID { get; set; }
            public string Sno { get; set; }
            public string Compare_Sno { get; set; }
            public bool status { get; set; }
            public int Al_frame { get; set; }
        }
    }
}
