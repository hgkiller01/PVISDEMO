using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERI.Utility.Extensions;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Extension;
using Pvis.Biz.Member;
using Pvis.Biz.Models;
using Pvis.Biz.Services;
using AutoMapper;
using Pvis.Web;
using Pvis.Web.Helper;
using System.Data;

namespace Pvis.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [PvisAuthorize(RoleList.Admin, RoleList.Epa, RoleList.Auditor, RoleList.Teart, RoleList.Store)]
    public class ScheduleController : ControllerBase
    {

        private readonly DataDbContext _context;

        public ScheduleController(DataDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("GetList")]
        public IEnumerable<ViewSRmodel> GetList(SchQry Qry)
        {
            var query = from a in _context.Set<Schedule>()
                        join b in _context.Set<Schedule_SB>()
                            on a.Cle_Sch_No equals b.Cle_Sch_No
                        join c in _context.Set<ScrapBooking>()
                            on b.Bookingno equals c.Bookingno
                        join d in _context.Set<UserSpInfo>()
                            on c.Pid equals d.Sbid
                        orderby a.Cle_Sch_No descending
                        group new { a } by new { a.Pid, a.Cle_Sch_No, a.Cle_Date, a.Cle_Name, a.Tre_Name, a.Cle_State,c.Uid } into g
                        select new ViewSRmodel
                        {
                            Pid = g.Key.Pid,
                            Cle_Sch_No = g.Key.Cle_Sch_No,
                            Cle_Date = g.Key.Cle_Date,
                            Cle_Name = g.Key.Cle_Name,
                            Tre_Name = g.Key.Tre_Name,
                            Cle_State = g.Key.Cle_State,
                            Cle_Qty = g.Count(),
                            Uid = g.Key.Uid
                        };

            var pred = PredicateBuilder.New<ViewSRmodel>(true);

            if (Qry.StartDt.HasValue) pred.And(x => x.Cle_Date >= Qry.StartDt.Value);

            if (Qry.EndDt.HasValue) pred.And(x => x.Cle_Date <= Qry.EndDt.Value);

            if (!String.IsNullOrWhiteSpace(Qry.Cle_State)) pred.And(x => x.Cle_State == Qry.Cle_State);

            var ViewModel = query.ToList();
            var UserList = AuthHelper.GetUserList();

            var Result = from r in ViewModel
                         join d in UserList
                         on r.Uid equals d.Uid
                         select new ViewSRmodel
                         {
                             Pid = r.Pid,
                             Cle_Date = r.Cle_Date,
                             Cle_Name = r.Cle_Name,
                             Tre_Name = r.Tre_Name,
                             Cle_State = r.Cle_State,
                             CompanyName = d.CompanyName,
                             Cle_Qty = r.Cle_Qty,
                             Cle_Sch_No = r.Cle_Sch_No,
                             Uid = d.Uid
                         };

            return Result.Where(pred).OrderByDescending(x => x.Cle_Sch_No).ToList();
        }

        [HttpPost]
        [Route("GetSBList")]
        public async Task<ActionResult<IEnumerable<ScrapBooking>>> GetSBList(SchQry Qry)
        {
            var pred = PredicateBuilder.New<ScrapBooking>(true);

            var sb = await _context.ScrapBooking.ToListAsync();
            var usa = await _context.UserStoreAddress.ToListAsync();
            foreach (var item in sb)
            {
                item.USAddr1 = usa.FirstOrDefault(x => x.Pid == item.Uspid).Storeaddr;
                item.Qty = await _context.UserSpInfo.Where(x => x.Sbid == item.Pid).CountAsync();
                var sp = await _context.UserSpInfo.Where(x => x.Sbid == item.Pid).ToListAsync();
                item.SPWeight1 = sp.Select(x => x.Spweight).Sum();
            }

            pred = pred.And(x => x.Status == "Y1");
            pred = pred.And(x => x.isSch == null);

            if (!String.IsNullOrWhiteSpace(Qry.City))
            {
                pred = pred.And(x => x.USAddr1.Contains(Qry.City));
            }

            if (Qry.StartDt.HasValue) pred.And(x => x.Appdate >= Qry.StartDt.Value);

            if (Qry.EndDt.HasValue) pred.And(x => x.Appdate <= Qry.EndDt.Value);

            return sb.Where(pred).ToList();
        }

        [HttpPost]
        [Route("GetItem")]
        public async Task<ActionResult<IEnumerable<Schedule>>> GetItem(SchQry Qry)
        {
            var _Rec = await _context.Schedule.Where(x => x.Pid == Qry.Pid).FirstOrDefaultAsync();
            Qry.Cle_Sch_No = _Rec.Cle_Sch_No;
            List<Schedule_SB> ListSS = null;
            List<ScrapBooking> ListSB = new List<ScrapBooking>();
            List<ScrapBooking> ListA = null;

            if (_Rec != null && _Rec.Pid > 0)
            {

                (int Sum_Qty, decimal Sum_Weight, ListSS, ListSB, ListA) = GetViewItem(ListSS, ListSB, ListA, Qry);
                _Rec.Sum_Qty = Sum_Qty;
                _Rec.Sum_Weight = Sum_Weight;

            }

            return Ok(new
            {
                IsSuccess = (_Rec != null),
                Rec = _Rec,
                RecSchSBList = ListSB
            });
        }

        [HttpPost]
        [Route("GetView")]
        public async Task<ActionResult<Schedule>> View(SchQry Qry)
        {
            var _Rec = await _context.Schedule.Where(x => x.Pid == Qry.Pid).FirstOrDefaultAsync();
            Qry.Cle_Sch_No = _Rec.Cle_Sch_No;
            List<Schedule_SB> ListSS = new List<Schedule_SB>();
            List<ScrapBooking> ListSB = new List<ScrapBooking>();
            List<ScrapBooking> ListA = new List<ScrapBooking>();
            List<Schedule_Review> ListSCR = new List<Schedule_Review>();
            if (_Rec != null && _Rec.Pid > 0)
            {
                (int Sum_Qty, decimal Sum_Weight, ListSS, ListSB, ListA) = GetViewItem(ListSS, ListSB, ListA, Qry);
                _Rec.Sum_Qty = Sum_Qty;
                _Rec.Sum_Weight = Sum_Weight;
                _Rec.FileCleDoc = await _context.FormFileUpload.Where(x => x.AppId == Qry.Pid.ToString()
                                    && x.DocType == eDocType.ScheduleDoc
                                    && x.ItemType == eItemType.CleDoc
                                  ).ToListAsync();
                _Rec.FileTreDoc = await _context.FormFileUpload.Where(x => x.AppId == Qry.Pid.ToString()
                                    && x.DocType == eDocType.ScheduleDoc
                                    && x.ItemType == eItemType.TreDoc
                                  ).ToListAsync();
                ListSCR = await _context.Schedule_Review.Where(x => x.Cle_Sch_No == Qry.Cle_Sch_No).OrderByDescending(x => x.Check_Date).ThenByDescending(x => x.Pid).ToListAsync();

            }
            return Ok(new
            {
                IsSuccess = (_Rec != null),
                Rec = _Rec,
                RecSchSBList = ListSB,
                ReviewList = ListSCR
            });
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IActionResult> Save(ViewSCHmodel SCH)
        {
            var SC = SCH.Rec;
            
            var _EntityState = SC.Pid > 0 ? EntityState.Modified : EntityState.Added;

            if (_EntityState == EntityState.Added)
            {
                var ymd = (DateTime.Now.Year - 1911) + DateTime.Now.ToString("MMdd");
                var cnt = "001";
                var schno = "";
                if (_context.Schedule.Count() > 0)
                {
                    schno = _context.Schedule.AsEnumerable().Last().Cle_Sch_No;

                    if (schno.Substring(1, 7) == ymd)
                    {
                        cnt = (schno.Substring(8, 3).TryParseInt() + 1).ToString("000");
                    }
                }
                
                SC.App_Date = DateTime.Now;
                SC.Cle_Sch_No = "C" + ymd + cnt;
                SC.Cle_State = "1";
            }

            var errors = new List<string>();

            if (errors.Count > 0) { return BadRequest(new { IsSuccess = false, errors }); }

            SC.Uid = User.GetUid();
            SC.Bno = SCH.Bno;

            _context.Entry(SC).State = _EntityState;
            try
            {
                await _context.SaveChangesAsync();

                var SB = new Schedule_SB();
                if (_EntityState == EntityState.Added)
                { 
                    foreach (var item in SC.Bno)
                    {
                        SB.Pid = 0; //important
                        SB.Bookingno = item;
                        SB.App_Date = DateTime.Now;
                        SB.Cle_Sch_No = SC.Cle_Sch_No;
                        SB.Uid = User.GetUid();
                        _context.Schedule_SB.Add(SB);
                        await _context.SaveChangesAsync();

                        //當排出登記編號已被安排清運日期，註記 isSch = Y
                        var _Rec = _context.ScrapBooking.Where(x => x.Bookingno == SB.Bookingno).FirstOrDefault();
                        _Rec.isSch = "Y";
                        _EntityState = EntityState.Modified;
                        await _context.SaveChangesAsync();
                    }
                }

            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            return Ok(new
            {
                Rec = SC,
                IsAdded = (_EntityState == EntityState.Added)
            });
        }

        [HttpPost]
        [Route("SaveReview")]
        public async Task<IActionResult> SaveReview(Schedule_Review SCR)
        {
            #region 表單驗證
            var errors = new List<string>();

            if (SCR.Status == "DM")
            {
                if (string.IsNullOrWhiteSpace(SCR.Check_opinion))
                {
                    errors.Add("『意見』未填寫");
                }
            }

            if (errors.Count > 0)
            {
                return BadRequest(new { IsSuccess = false, errors });
            }
            #endregion

            var State = SCR.Pid > 0 ? EntityState.Modified : EntityState.Added;

            if (State == EntityState.Added)
            {
                SCR.App_Date = DateTime.Now;
                SCR.Uid = User.GetUid();
                SCR.Check_Date = DateTime.Now;
                SCR.Check_Name = User.GetDisplayName();
            }
            var SC = _context.Schedule.Where(x => x.Cle_Sch_No == SCR.Cle_Sch_No).FirstOrDefault();;
            SC.Cle_State = SCR.Status;

            using (var transaction = _context.Database.BeginTransaction())
            {
                _context.Entry(SCR).State = State;
                await _context.SaveChangesAsync();

                #region 更新清理行程表
                _context.Add(SC).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                #endregion


                transaction.Commit();
            }

            //SendMail(SC, SCR);

            return Ok(new { Rec = SC });
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> Delete(Schedule SC)
        {
            var _Schedule = await _context.Schedule.FindAsync(SC.Pid);
            if (_Schedule == null)
            {
                return NotFound();
            }

            //移除選取的案場排出登記
            var _Schedule_SB = _context.Schedule_SB.Where(x => x.Cle_Sch_No == SC.Cle_Sch_No).ToList();
            foreach(var Sch in _Schedule_SB)
            {
                var changeItem = _context.ScrapBooking.Where(x => x.Bookingno == Sch.Bookingno).FirstOrDefault();
                changeItem.isSch = null;
            }
            
            _context.Schedule.Remove(_Schedule);
            _context.Schedule_SB.RemoveRange(_Schedule_SB);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost]
        [Route("GetCityOptions")]
        public async Task<ActionResult<List<County>>> GetCityOptions()
        {
            return await _context.County.ToListAsync();
        }
        [NonAction]
        public (int ,decimal, List<Schedule_SB>, List<ScrapBooking>, List<ScrapBooking>) 
            GetViewItem(List<Schedule_SB> ListSS, List<ScrapBooking> ListSB, 
            List<ScrapBooking> ListA, SchQry Qry)
        {
            int Sum_Qty = 0;
            decimal Sum_Weight = 0;
            ListSS =  _context.Schedule_SB.Where(x => x.Cle_Sch_No == Qry.Cle_Sch_No).ToList();
            foreach (var item in ListSS)
            {
                ListSB = _context.ScrapBooking.Where(x => x.Bookingno == item.Bookingno).ToList();
                if (ListA == null)
                    ListA = ListSB;
                else
                    ListSB.AddRange(ListA);
            }
            var usa = _context.UserStoreAddress.ToList();
            foreach (var item in ListSB)
            {
                item.USAddr1 = usa.FirstOrDefault(x => x.Pid == item.Uspid).Storeaddr;
                item.Qty = _context.UserSpInfo.Where(x => x.Sbid == item.Pid).Count();
                var sp =  _context.UserSpInfo.Where(x => x.Sbid == item.Pid).ToList();
                item.SPWeight1 = sp.Select(x => x.Spweight).Sum();
                item.Al_frameYQty =  _context.UserSpInfo.Where(x => x.Sbid == item.Pid && x.AlFrame == "1").Count();
                item.Al_frameNQty =  _context.UserSpInfo.Where(x => x.Sbid == item.Pid && x.AlFrame == "0").Count();
                Sum_Qty += Convert.ToInt32(item.Qty);
                Sum_Weight += Convert.ToDecimal(item.SPWeight1);
            }
            return (Sum_Qty, Sum_Weight,ListSS,ListSB,ListA);
        }
        public class ViewSCHmodel
        {
            public Schedule Rec { get; set; }
            public List<string> Bno { get; set; }
            public Dictionary<eItemType, Pvis.Biz.ViewModels.AttachmentViewModel> Attachment { internal get; set; }
        }

        public class ViewSRmodel
        {
            public int Pid { get; set; }
            public string Cle_Sch_No { get; set; }
            public DateTime? Cle_Date { get; set; }
            public string Cle_Name { get; set; }
            public string Tre_Name { get; set; }
            public string Cle_State { get; set; }
            public int Uid { get; set; }
            public int? Cle_Qty { get; set; }
            public string CompanyName { get; set; }
        }

        public class SchQry
        {
            public int Pid { get; set; }
            public string City { get; set; }
            public int Uspid { get; set; }
            public DateTime? StartDt { get; set; }
            public DateTime? EndDt { get; set; }
            public string Cle_Sch_No { get; set; }
            public string Bookingno { get; set; }
            public string Cle_State { get; set; }
        }
    }
}
