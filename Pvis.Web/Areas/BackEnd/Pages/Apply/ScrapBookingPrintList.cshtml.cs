using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pvis.Biz.Models;
using Microsoft.AspNetCore.Mvc;
using Pvis.Biz.Member;
using System;

namespace Pvis.Web.Areas.BackEnd.Pages.Apply
{    
    public class ScrapBookingPrintListModel : PageModel
    {
        private DataDbContext _context;
        private readonly ApplicationDbContext _appcontext;

        public List<SBPrintmodel> sbPrintList = new List<SBPrintmodel>();

        //排出表聯單編號
        public Dictionary<string, int> dicSB = new Dictionary<string, int>();
        //太陽能板光電序號
        public Dictionary<string, string> dicSP = new Dictionary<string, string>();


        public ScrapBookingPrintListModel(
            DataDbContext context,
            ApplicationDbContext applicationDbContext
            )
        {
            _context = context;
            _appcontext = applicationDbContext;
        }


        public async Task<IActionResult> OnPost(string cno)
        {
            if (string.IsNullOrEmpty(cno)) return NotFound();

            //排出登記表
            var scsb = _context.Schedule_SB.Where(x => x.Cle_Sch_No == cno).ToList();
            foreach (var item in scsb)
            {
                int idx_af = 0;    //有鋁框的index
                int idx_noaf = 0;  //無鋁框的index

                int sb_afcnt = 0;  //有鋁框的筆數
                int sb_noafcnt = 0;  //無鋁框的筆數

                //有無鋁框及序號的片數
                int Alframe1_hasno1 = 0;
                int Alframe1_hasno0 = 0;
                int Alframe0_hasno1 = 0;
                int Alframe0_hasno0 = 0;


                //排出登記表
                var sb = await _context.ScrapBooking.Where(x => x.Bookingno == item.Bookingno).FirstOrDefaultAsync();

                //建檔者資料
                var user = await _appcontext.Users.Where(x => x.Uid == sb.Uid).FirstOrDefaultAsync();

                //清理行程主檔
                var sc = await _context.Schedule.Where(x => x.Cle_Sch_No == item.Cle_Sch_No && (x.Cle_State == "C1" || x.Cle_State == "Y1")).FirstOrDefaultAsync();
                sc ??= new Schedule();

                //設備編號
                string pvno_tmp = string.Empty;
                var pv = await _context.ApplyPvInfo.Where(x => x.SBPid == sb.Pid).ToListAsync();
                //var pv = _context.ApplyPvInfo.Where(x => x.SBPid <3000).ToList();  //test
                foreach (var p in pv)
                {
                    pvno_tmp += ", " + p.Pvno;

                    //01-1 序號清單表資料
                    var sp0 = await _context.UserSpInfo.Where(x => x.Sbid == sb.Pid && x.Pvid == p.Pid && x.Hasno == "1").ToListAsync();
                    foreach (var s in sp0)
                    {
                        string sp_key = string.Empty;
                        if (s.AlFrame == "1")  //有鋁框
                        {
                            idx_af++;
                            sp_key = sb.Bookingno + "_1-" + idx_af;
                            sb_afcnt++;
                        }
                        else  //無鋁框
                        {
                            idx_noaf++;
                            sp_key = sb.Bookingno + "_0-" + idx_noaf;
                            sb_noafcnt++;
                        }
                        dicSP.Add(sp_key, s.Sno);
                    }

                    //片數
                    var sp = await _context.UserSpInfo.Where(x => x.Sbid == sb.Pid && x.Pvid == p.Pid).ToListAsync();
                    foreach (var s in sp)
                    {
                        if (s.AlFrame == "1")  //有鋁框
                        {
                            if (s.Hasno == "1") //有序號
                            {
                                Alframe1_hasno1++;
                            }
                            else //無序號
                            {
                                Alframe1_hasno0++;
                            }
                        }
                        else //無鋁框
                        {
                            if (s.Hasno == "1") //有序號
                            {
                                Alframe0_hasno1++;
                            }
                            else //無序號
                            {
                                Alframe0_hasno0++;
                            }
                        }
                    }
                }

                pvno_tmp = pvno_tmp.Length > 0 ? pvno_tmp.Substring(2) : "";
                dicSB.Add(item.Bookingno + ",0", sb_noafcnt);   //排出表聯單編號及太陽能板光電序號筆數(有序號、無鋁框)
                dicSB.Add(item.Bookingno + ",1", sb_afcnt);   //排出表聯單編號及太陽能板光電序號筆數(有序號、有鋁框)
                sbPrintList.Add(new SBPrintmodel()
                {
                    Bookingno = item.Bookingno,
                    sAppdate = (sb.Appdate.Year - 1911) + "年" + sb.Appdate.ToString("MM月dd日HH時mm分"),
                    Pvno = pvno_tmp,
                    CompanyName = user.CompanyName,
                    Cle_Sch_No = item.Cle_Sch_No,
                    Cle_Name = sc.Cle_Name,
                    Tre_Name = sc.Tre_Name,
                    SPWeightSum = Convert.ToDecimal(_context.UserSpInfo.Where(x => x.Sbid == sb.Pid).Select(s => s.Spweight).Sum()),
                    Alframe1_hasno1 = Alframe1_hasno1,
                    Alframe1_hasno0 = Alframe1_hasno0,
                    Alframe0_hasno1 = Alframe0_hasno1,
                    Alframe0_hasno0 = Alframe0_hasno0,
                });

            }

            return Page();
        }
    }


    public class SBPrintmodel
    {
        public string Bookingno { get; set; }
        public string sAppdate { get; set; }
        public string Pvno { get; set; }
        public string CompanyName { get; set; }
        public string Cle_Sch_No { get; set; }
        public string Cle_Name { get; set; }
        public decimal SPWeightSum { get; set; }
        public string Tre_Name { get; set; }        
        /// <summary>有鋁框有序號</summary>
        public int Alframe1_hasno1 { get; set; }
        /// <summary>有鋁框無序號</summary>
        public int Alframe1_hasno0 { get; set; }
        /// <summary>無鋁框有序號</summary>
        public int Alframe0_hasno1 { get; set; }
        /// <summary>無鋁框無序號</summary>
        public int Alframe0_hasno0 { get; set; }

        
    }
}
