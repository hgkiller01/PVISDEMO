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
using Pvis.Web.Helper;
using Pvis.Biz.ViewModels;

namespace Pvis.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [PvisAuthorize(RoleList.Admin, RoleList.Epa, RoleList.Store, RoleList.Teart)]
    public class ScheduleAuditApplyController : ControllerBase
    {
        private readonly DataDbContext _context;
        public ScheduleAuditApplyController(DataDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("GetList")]
        public async Task<ActionResult<IEnumerable<ScheduleAuditController.ViewARmodel>>> GetList(SchQry Qry)
        {
            var query2 = _context.ScheduleAudit.Select(s => new ScheduleAuditController.ViewARmodel
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
                           select g.Count()).Sum()

            });
            var pred = PredicateBuilder.New<ScheduleAuditController.ViewARmodel>(true);
            if (!(User.HasRole(RoleList.Admin) || User.HasRole(RoleList.Epa)))
            {
                pred.And(x => x.Uid == User.GetUid());
            }
            if (Qry.StartDt.HasValue) pred.And(x => x.Pre_Date >= Qry.StartDt.Value);

            if (Qry.EndDt.HasValue) pred.And(x => x.Pre_Date <= Qry.EndDt.Value.AddDays(1));

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

            if (Qry.StartDt.HasValue) pred.And(x => x.Enter_Date >= Qry.StartDt.Value);

            if (Qry.EndDt.HasValue) pred.And(x => x.Enter_Date <= Qry.EndDt.Value.AddDays(1));

            pred = pred.And(x => x.Status == "Y1");
            pred = pred.And(x => x.isSch == "D");


            var sb = await (from Scrap in _context.ScrapBooking join
                           ScheduleSb in _context.Schedule_SB on Scrap.Bookingno equals ScheduleSb.Bookingno
                           join scheDule in _context.Schedule on ScheduleSb.Cle_Sch_No equals scheDule.Cle_Sch_No
                           where !(_context.ScheduleAudit_SB.Select(x => x.Bookingno)).Contains(ScheduleSb.Bookingno)
                           && (scheDule.Cle_State == "C1" || scheDule.Cle_State == "D1")
                           select new ScrapBooking
                           {
                               Pid = Scrap.Pid,
                               Bookingno = Scrap.Bookingno,
                               Enter_Date = scheDule.Enter_Date,
                               Tre_Name = scheDule.Tre_Name,
                               Uspid = Scrap.Uspid,
                               Status = Scrap.Status,
                               isSch = Scrap.isSch,
                               Uid = Scrap.Uid
                           }).ToListAsync();
            var Users = AuthHelper.GetUserList();
            foreach (var item in sb)
            {
                item.UserName = Users.Where(x => x.Uid == item.Uid).FirstOrDefault().DisplayName;
            }
            var usa = await _context.UserStoreAddress.ToListAsync();
            foreach (var item in sb)
            {
                item.USAddr1 = usa.FirstOrDefault(x => x.Pid == item.Uspid).Storeaddr;
                item.Qty = await _context.UserSpInfo.Where(x => x.Sbid == item.Pid).CountAsync();
                var sp = await _context.UserSpInfo.Where(x => x.Sbid == item.Pid).ToListAsync();
                item.SPWeight1 = sp.Select(x => x.Spweight).Sum();
            }
            int uid = User.GetUid();
            var usersm = User.GetCurrentUser();
            if (!(User.HasRole(RoleList.Admin) || User.HasRole(RoleList.Epa)))
            {
                sb = sb.Where(x => x.Tre_Name == User.GetCompanyName()).ToList();
            }

            return sb.Where(pred).ToList();
        }

        [HttpPost]
        [Route("GetItem")]
        public async Task<ActionResult<IEnumerable<ScheduleAudit>>> GetItem(SchQry Qry)
        {
            var _Rec = await _context.ScheduleAudit.Where(x => x.Pid == Qry.Pid).FirstOrDefaultAsync();
            Qry.Aud_Sch_No = _Rec.Aud_Sch_No;
            ScrapBooking ListSB = new ScrapBooking();
            List<ScrapBooking> ListA = new List<ScrapBooking>();

            if (_Rec != null && _Rec.Pid > 0)
            {
                GetViewItem( ListA, ListSB, Qry);
                foreach (var item in ListA)
                {
                    item.Qty = await _context.UserSpInfo.Where(x => x.Sbid == item.Pid).CountAsync();
                    var sp = await _context.UserSpInfo.Where(x => x.Sbid == item.Pid).ToListAsync();
                    item.SPWeight1 = sp.Select(x => x.Spweight).Sum();

                }
            }

            return Ok(new
            {
                IsSuccess = (_Rec != null),
                Rec = _Rec,
                RecSchSBList = ListA
            });
        }

        [HttpPost]
        [Route("View")]
        public async Task<ActionResult<ScheduleAudit>> View(SchQry Qry)
        {
            var _Rec = _context.ScheduleAudit.Where(x => x.Pid == Qry.Pid).FirstOrDefault();
            Qry.Aud_Sch_No = _Rec.Aud_Sch_No;
            ScrapBooking ListSB = new ScrapBooking();
            List<ScrapBooking> ListA = new List<ScrapBooking>();
            List<ScheduleAudit_Review> ListSCR = new List<ScheduleAudit_Review>();
            if (_Rec != null && _Rec.Pid > 0)
            {
               (ListA, ListSB) =  GetViewItem(ListA, ListSB, Qry);

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
                if (_EntityState == EntityState.Added)
                {
                    foreach (var item in SC.Bno)
                    {
                        SB.Pid = 0; //important
                        SB.Bookingno = item;
                        SB.App_Date = DateTime.Now;
                        SB.Aud_Sch_No = SC.Aud_Sch_No;
                        SB.Uid = User.GetUid();
                        _context.ScheduleAudit_SB.Add(SB);
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
        [Route("DeleteItem")]
        public IActionResult DeleteItem(Aud_Sch aud_Sch)
        {
            var main = _context.ScheduleAudit.Where(x => x.Aud_Sch_No == aud_Sch.Aud_Sch_No);
            var sub = _context.ScheduleAudit_SB.Where(x => x.Aud_Sch_No == aud_Sch.Aud_Sch_No);
            var review = _context.ScheduleAudit_Review.Where(x => x.Aud_Sch_No == aud_Sch.Aud_Sch_No);
            _context.ScheduleAudit.RemoveRange(main);
            _context.ScheduleAudit_SB.RemoveRange(sub);
            _context.ScheduleAudit_Review.RemoveRange(review);
            _context.SaveChanges();
            return Ok();
        }
        [NonAction]
        public (List<ScrapBooking>, ScrapBooking) GetViewItem(List<ScrapBooking> ListA, ScrapBooking ListSB, SchQry Qry)
        {
            var ListSS =  _context.ScheduleAudit_SB.Where(x => x.Aud_Sch_No == Qry.Aud_Sch_No).ToList();
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
            return (ListA, ListSB);
        }
        public class Aud_Sch
        {
            public string Aud_Sch_No { get; set; }
        }




    }
}
