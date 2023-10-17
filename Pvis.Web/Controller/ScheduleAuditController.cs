using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ERI.Utility.Extensions;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Extension;
using Pvis.Biz.Member;
using Pvis.Biz.Models;
using Pvis.Web.Helper;
using Pvis.Biz.ViewModels;

namespace Pvis.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [PvisAuthorize(RoleList.Admin, RoleList.Epa, RoleList.Auditor)]
    public class ScheduleAuditController : ControllerBase
    {
        public DataDbContext _context;
        public ScheduleAuditController(DataDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("GetList")]
        public async Task<ActionResult<IEnumerable<ViewARmodel>>> GetList(SchQry Qry)
        {
            var query2 = _context.ScheduleAudit.Select(s => new ViewARmodel
            {
                Uid = s.Uid,
                Pid = s.Pid,
                Aud_Sch_No = s.Aud_Sch_No,
                Pre_Date = s.Pre_Date,
                Tre_Pre_Date = s.Tre_Pre_Date,
                Pre_Minute = _context.ScheduleAudit_Review.Where(x => x.Aud_Sch_No == s.Aud_Sch_No).FirstOrDefault().Pre_Minute,
                Tre_Pre_Minute = _context.ScheduleAudit_Review.Where(x => x.Aud_Sch_No == s.Aud_Sch_No).FirstOrDefault().Tre_Pre_Minute,
                Aud_State = s.Aud_State,
                Pre_Qry = (from sb in _context.ScheduleAudit_SB.Where(x => x.Aud_Sch_No == s.Aud_Sch_No)
                           join booking in _context.ScrapBooking on sb.Bookingno equals booking.Bookingno
                           join sp in _context.UserSpInfo on booking.Pid equals sp.Sbid
                           group booking by new { booking.Bookingno } into g
                           select g.Count()).Sum(),
                CompanyName = s.CompanyName,
                Aud_Man = _context.ScheduleAudit_Review.Where(x => x.Aud_Sch_No == s.Aud_Sch_No).FirstOrDefault().Aud_Man,
                Tre_Aud_Man = _context.ScheduleAudit_Review.Where(x => x.Aud_Sch_No == s.Aud_Sch_No).FirstOrDefault().Tre_Aud_Man
            });
            if (User.HasRole(RoleList.Admin, RoleList.Auditor))
            {
                query2 = query2.Where(x => x.Aud_State != "1");
            }
            else
            {
                query2 = query2.Where(x => x.Uid == User.GetUid());
            }
            var pred = PredicateBuilder.New<ViewARmodel>(true);

            if (Qry.StartDt.HasValue) pred.And(x => x.Pre_Date >= Qry.StartDt.Value);

            if (Qry.EndDt.HasValue) pred.And(x => x.Pre_Date <= Qry.EndDt.Value);

            if (!String.IsNullOrWhiteSpace(Qry.Aud_State)) pred.And(x => x.Aud_State == Qry.Aud_State);

            return await query2.Where(pred).ToListAsync();
        }

        [HttpPost]
        [Route("GetSBList")]
        public async Task<ActionResult<IEnumerable<ScrapBooking>>> GetSBList(SchQry Qry)
        {
            var pred = PredicateBuilder.New<ScrapBooking>(true);
            if (!String.IsNullOrWhiteSpace(Qry.City))
            {
                pred = pred.And(x => x.USAddr1.Contains(Qry.City));
            }

            if (Qry.StartDt.HasValue) pred.And(x => x.Appdate >= Qry.StartDt.Value);

            if (Qry.EndDt.HasValue) pred.And(x => x.Appdate <= Qry.EndDt.Value);

            pred = pred.And(x => x.Status == "Y1");
            pred = pred.And(x => x.isSch == "D");

            var sb = await _context.ScrapBooking.ToListAsync();
            var usa = await _context.UserStoreAddress.ToListAsync();
            foreach (var item in sb)
            {
                item.USAddr1 = usa.FirstOrDefault(x => x.Pid == item.Uspid).Storeaddr;
                item.Enter_Date = _context.ScrapBooking.Where(x => x.Bookingno == item.Bookingno).FirstOrDefault().Enter_Date;
                item.Qty = await _context.UserSpInfo.Where(x => x.Sbid == item.Pid).CountAsync();
                var sp = await _context.UserSpInfo.Where(x => x.Sbid == item.Pid).ToListAsync();
                item.SPWeight1 = sp.Select(x => x.Spweight).Sum();
            }

            return sb.Where(pred).ToList();
        }

        [HttpPost]
        [Route("GetView")]
        public async Task<ActionResult<ScheduleAudit>> View(SchQry Qry)
        {
            var _Rec = _context.ScheduleAudit.Where(x => x.Pid == Qry.Pid).FirstOrDefault();
            Qry.Aud_Sch_No = _Rec.Aud_Sch_No;
            List<ScheduleAudit_SB> ListSS = null;
            ScrapBooking ListSB = null;
            List<ScrapBooking> ListA = new List<ScrapBooking>();
            List<ScheduleAudit_Review> ListSCR = null;
            if (_Rec != null && _Rec.Pid > 0)
            {

                ListSS = await _context.ScheduleAudit_SB.Where(x => x.Aud_Sch_No == Qry.Aud_Sch_No).ToListAsync();
                foreach (var item in ListSS)
                {

                    ListSB = _context.ScrapBooking.Where(x => x.Bookingno == item.Bookingno).FirstOrDefault();
                    ListSB.Enter_Date = (from sch in _context.Schedule
                                         join scsb in _context.Schedule_SB on sch.Cle_Sch_No equals scsb.Cle_Sch_No
                                         where scsb.Bookingno == item.Bookingno
                                         select sch.Enter_Date).FirstOrDefault();
                    ListSB.CompanyName = _Rec.CompanyName;
                    ListA.Add(ListSB);
                }
                foreach (var item in ListA)
                {
                    item.Qty = await _context.UserSpInfo.Where(x => x.Sbid == item.Pid).CountAsync();
                }
                ListSCR = await _context.ScheduleAudit_Review.Where(x => x.Aud_Sch_No == Qry.Aud_Sch_No).OrderByDescending(x => x.Check_Date).ThenByDescending(x => x.Pid).ToListAsync();

            }
            return Ok(new
            {
                IsSuccess = (_Rec != null),
                Rec = _Rec,
                RecSchSBList = ListA,
                ReviewList = ListSCR
            });
        }



 
        [NonAction]
        public void GetViewItem(List<ScrapBooking> ListA, ScrapBooking ListSB, SchQry Qry)
        {
            var ListSS = _context.ScheduleAudit_SB.Where(x => x.Aud_Sch_No == Qry.Aud_Sch_No).ToList();
            foreach (var item in ListSS)
            {
                ListSB = _context.ScrapBooking.Where(x => x.Bookingno == item.Bookingno).FirstOrDefault();
                var UserName = AuthHelper.GetUserQuery().Where(x => x.Uid == ListSB.Uid).FirstOrDefault().DisplayName;
                ListSB.UserName = UserName;
                var enterDate = from scsb in _context.Schedule_SB
                                join sce in _context.Schedule on scsb.Cle_Sch_No equals sce.Cle_Sch_No
                                where scsb.Bookingno == item.Bookingno && (sce.Cle_State == "C1" || sce.Cle_State == "D1")
                                select sce.Enter_Date;
                ListSB.Enter_Date = enterDate.FirstOrDefault();
                ListA.Add(ListSB);

            }
        }
        //[HttpPost]
        //[Route("GetAuditAccount")]
        //public async Task<ActionResult<IEnumerable<MyAppUserForm>>> GetAuditAccount()
        //{
        //    MemberQry Qry = new MemberQry();
        //    Qry.Role = "Auditor";

        //    var Result = (await _MemberBiz.GetListAsync(Qry)).Select(x => mapper.Map<MyAppUserForm>(x));
        //    return Ok(Result);
        //}
        [HttpPost]
        [Route("SaveReview")]
        public async Task<IActionResult> SaveReview(ScheduleAudit_Review SAR)
        {
            #region 表單驗證
            var errors = new List<string>();

            if (SAR.Status == "M" || SAR.Status == "N")
            {
                if (string.IsNullOrWhiteSpace(SAR.Check_opinion))
                {
                    errors.Add("『意見』未填寫");
                }
            }

            if (errors.Count > 0)
            {
                return BadRequest(new { IsSuccess = false, errors });
            }
            #endregion

            var State = SAR.Pid > 0 ? EntityState.Modified : EntityState.Added;

            if (State == EntityState.Added)
            {
                SAR.App_Date = DateTime.Now;
                SAR.Uid = User.GetUid();
                SAR.Check_Date = DateTime.Now;
                SAR.Check_Name = User.GetDisplayName();
            }
            var SA = _context.ScheduleAudit.Where(x => x.Aud_Sch_No == SAR.Aud_Sch_No).FirstOrDefault(); ;
            SA.Aud_State = SAR.Status;
            if (SA.Aud_State == "Y1")
            {
                SA.Check_State = "1";
            }
            using (var transaction = _context.Database.BeginTransaction())
            {
                _context.Entry(SAR).State = State;
                await _context.SaveChangesAsync();

                #region 更新稽核行程表
                _context.Add(SA).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                #endregion


                transaction.Commit();
            }

            //SendMail(SC, SCR);

            return Ok(new { Rec = SA });
        }
        [HttpPost]
        [Route("Save")]
        public async Task<IActionResult> Save(ViewSAmodel SCH)
        {
            var SC = SCH.Rec;

            var _EntityState = SC.Pid > 0 ? EntityState.Modified : EntityState.Added;

            if (_EntityState == EntityState.Added)
            {
                var ymd = (DateTime.Now.Year - 1911) + DateTime.Now.ToString("MMdd");
                var cnt = "001";
                var schno = "";
                if (_context.ScheduleAudit.Count() > 0)
                {
                    schno = _context.ScheduleAudit.AsEnumerable().Last().Aud_Sch_No;

                    if (schno.Substring(1, 7) == ymd)
                    {
                        cnt = (schno.Substring(8, 3).TryParseInt() + 1).ToString("000");
                    }
                }

                SC.App_Date = DateTime.Now;
                SC.Aud_Sch_No = "A" + ymd + cnt;
                SC.Aud_State = "1";
                SC.CompanyName = User.GetCompanyName();
            }

            var errors = new List<string>();

            if (errors.Count > 0) { return BadRequest(new { IsSuccess = false, errors }); }

            SC.Uid = User.GetUid();
            SC.Bno = SCH.Bno;

            _context.Entry(SC).State = _EntityState;
            try
            {
                await _context.SaveChangesAsync();

                var SB = new ScheduleAudit_SB();
                foreach (var item in SC.Bno)
                {
                    SB.Pid = 0; //important
                    SB.Bookingno = item;
                    SB.App_Date = DateTime.Now;
                    SB.Aud_Sch_No = SC.Aud_Sch_No;
                    SB.Uid = User.GetUid();
                    SB.Compare_State = false;
                    _context.ScheduleAudit_SB.Add(SB);
                    await _context.SaveChangesAsync();
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
        public class ViewARmodel
        {
            public int Uid { get; set; }
            public int Pid { get; set; }
            public string Aud_Sch_No { get; set; }
            public DateTime? Pre_Date { get; set; }
            public int? Pre_Minute { get; set; }
            public DateTime? Tre_Pre_Date { get; set; }
            public int? Tre_Pre_Minute { get; set; }
            public int? Pre_Qry{ get; set; }
            public string Aud_State { get; set; }
            public string Aud_Man { get; set; }
            public string Tre_Aud_Man { get; set; }
            public string Tre_Name { get; set; }
            public string CompanyName { get; set; }
        }
    }
}
