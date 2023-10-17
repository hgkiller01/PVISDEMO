using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pvis.Biz.Member;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Models;
using Pvis.Biz.Extension;
using Microsoft.EntityFrameworkCore;
using Pvis.Web.Helper;

namespace Pvis.Web.Controller
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [ValidateAntiForgeryToken]
    [PvisAuthorize(RoleList.Admin, RoleList.Epa, RoleList.Teart, RoleList.Store)]
    public class PvController : ControllerBase
    {
        private readonly DataDbContext _context;

        public PvController(DataDbContext context)
        {
            _context = context;
        }

        public List<PvImportVM> GetFirstLoadData(ItemNo itemNo)
        {
            List<PvImportVM> m = _context.PvImport.Where(o => o.Aud_Sch_No == itemNo.No && o.State != 9).Select(o => new PvImportVM()
            {
                Id = o.Id,
                Aud_Sch_No = o.Aud_Sch_No,
                BookingNoId = o.BookingNoId,
                AL_O_SN_O = o.AL_O_SN_O,
                AL_O_SN_X = o.AL_O_SN_X,
                AL_O_SN_N = o.AL_O_SN_N,
                AL_X_SN_O = o.AL_X_SN_O,
                AL_X_SN_X = o.AL_X_SN_X,
                AL_X_SN_N = o.AL_X_SN_N,
                PV_Weight = o.PV_Weight,
                Pkg_Weight = o.Pkg_Weight,
                CompanyName = o.CompanyName,
                Creator = o.Creator,
                CreateDate = o.CreateDate,
                Modifier = o.Modifier,
                ModifyDate = o.ModifyDate,
                State = o.State,
                Pvcount = (from booking in _context.ScrapBooking join sppv in _context.UserSpInfo on booking.Pid equals sppv.Sbid
                           where booking.Bookingno == o.BookingNoId
                          select sppv).Count()
                
            }).ToList();

            if (m != null)
            {
                return m;
            }
            else
            {
                return null;
            }
        }

        public List<PvTreatmentVM> GetPvTreatmentData(ItemNo itemNo)
        {
            List<PvTreatmentVM> m = _context.PvTreatment.Where(o => o.Aud_Sch_No == itemNo.No && o.State != 9).Select(o => new PvTreatmentVM()
            {
                Id = o.Id,
                Aud_Sch_No = o.Aud_Sch_No,
                Pv_Imp_Id = o.Pv_Imp_Id,
                TreatmentDate = o.TreatmentDate,
                AL_O_SN_O = o.AL_O_SN_O,
                AL_O_SN_X = o.AL_O_SN_X,
                AL_O_SN_N = o.AL_O_SN_N,
                AL_X_SN_O = o.AL_X_SN_O,
                AL_X_SN_X = o.AL_X_SN_X,
                AL_X_SN_N = o.AL_X_SN_N,
                Weight = o.Weight,
                CompanyName = o.CompanyName,
                Creator = o.Creator,
                CreateDate = o.CreateDate,
                Modifier = o.Modifier,
                ModifyDate = o.ModifyDate,
                State = o.State
            }).ToList();

            if (m != null)
            {
                return m;
            }
            else
            {
                return null;
            }
        }

        public IActionResult GetWorkDuration(ItemNo itemNo)
        {
            List<DateTime> a = new List<DateTime>();
            a = _context.PvTreatment.Where(o => o.Aud_Sch_No == itemNo.No && o.State == 1).OrderBy(o => o.TreatmentDate).Select(o => o.TreatmentDate).ToList();
            string sdate = a.Count  == 0 ? a[0].ToString("yyyy年MM月dd日") : "________年____月____日";
            string edate = a.Count > 1 ? a[a.Count - 1].ToString("yyyy年MM月dd日") : "________年____月____日";
            string WD = sdate + " ~ " + edate;
            return Ok(new { wd = WD });
        }

        public List<BookingNo> GetBookingNoIdList(ItemNo itemNo)
        {
            List<BookingNo> m = _context.ScheduleAudit_SB.Where(o => o.Aud_Sch_No == itemNo.No).Select(s => new BookingNo() {BNo = s.Bookingno}).ToList();
            return m;
        }

        public IActionResult AddTreatmentData(PvTreatment s)
        {
            s.CompanyName = User.GetCompanyName();
            s.CreateDate = DateTime.Now;
            s.Creator = User.GetUid().ToString();
            s.ModifyDate = DateTime.Now;
            s.Modifier = User.GetUid().ToString();
            s.State = 1;

            _context.PvTreatment.Add(s);
            _context.SaveChanges();

            return Ok();
        }

        public IActionResult Insert(PvImport s)
        {
            if (CheckBookingNoId(s.BookingNoId, s.Aud_Sch_No) == true)
            {
                s.CompanyName = User.GetCompanyName();
                s.CreateDate = DateTime.Now;
                s.Creator = User.GetUid().ToString();
                s.ModifyDate = DateTime.Now;
                s.Modifier = User.GetUid().ToString();
                s.State = 1;

                _context.PvImport.Add(s);
                _context.SaveChanges();
                var msg = new { res = "OK" };
                return Ok(msg);
            }
            else
            {
                var msg = new { res = "此案廠排出登記編號已有資料，如需異動，請使用編輯功能。" };
                return Ok(msg);
            }
        }

        public bool CheckBookingNoId(string BookingNoId, string Aud_Sch_No)
        {
            var a = _context.PvImport.Where(o => o.Aud_Sch_No == Aud_Sch_No && o.BookingNoId == BookingNoId && o.State != 9).ToList();
            if (a.Count != 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public IActionResult EditTreatmentData(PvTreatment s)
        {
            var ss = _context.PvTreatment.Where(o => o.Id == s.Id && o.State == 1).FirstOrDefault();
            if (ss != null)
            {
                ss.TreatmentDate = s.TreatmentDate;
                ss.AL_O_SN_O = s.AL_O_SN_O;
                ss.AL_O_SN_X = s.AL_O_SN_X;
                ss.AL_O_SN_N = s.AL_O_SN_N;
                ss.AL_X_SN_O = s.AL_X_SN_O;
                ss.AL_X_SN_X = s.AL_X_SN_X;
                ss.AL_X_SN_N = s.AL_X_SN_N;
                ss.Weight = s.Weight;
                ss.Modifier = User.GetUid().ToString();
                ss.ModifyDate = DateTime.Now;
                _context.SaveChanges();
                return Ok();
            }
            return null;
        }

        public IActionResult Edit(PvImport s)
        {
            var ss = _context.PvImport.Where(o => o.Id == s.Id && o.State == 1).FirstOrDefault();
            if (ss != null)
            {
                ss.BookingNoId = s.BookingNoId;
                ss.AL_O_SN_O = s.AL_O_SN_O;
                ss.AL_O_SN_X = s.AL_O_SN_X;
                ss.AL_O_SN_N = s.AL_O_SN_N;
                ss.AL_X_SN_O = s.AL_X_SN_O;
                ss.AL_X_SN_X = s.AL_X_SN_X;
                ss.AL_X_SN_N = s.AL_X_SN_N;
                ss.PV_Weight = s.PV_Weight;
                ss.Pkg_Weight = s.Pkg_Weight;
                ss.Modifier = User.GetUid().ToString();
                ss.ModifyDate = DateTime.Now;
                _context.SaveChanges();
                return Ok();
            }
            return null;
        }

        public IActionResult DeleteTreatmentData(PvTreatment s)
        {
            var ss = _context.PvTreatment.Where(o => o.Id == s.Id && o.State == 1).FirstOrDefault();
            if (ss != null)
            {
                ss.State = 9;
                _context.SaveChanges();
                return Ok();
            }
            return null;
        }

        public IActionResult Delete(PvImport s)
        {
            var ss = _context.PvImport.Where(o => o.Id == s.Id && o.State == 1).FirstOrDefault();
            if (ss != null)
            {
                ss.State = 9;
                _context.SaveChanges();
                return Ok();
            }
            return null;
        }

        public class ItemNo
        {
            public string No { get; set; }
        }

        public class BookingNo
        {
            public string BNo { get; set; }
        }

        public class PvImportVM
        {
            public int Id { get; set; }
            public string Aud_Sch_No { get; set; }
            public string BookingNoId { get; set; }
            public DateTime TreatmentDate { get; set; }
            public int AL_O_SN_O { get; set; }
            public int AL_O_SN_X { get; set; }
            public int AL_O_SN_N { get; set; }
            public int AL_X_SN_O { get; set; }
            public int AL_X_SN_X { get; set; }
            public int AL_X_SN_N { get; set; }
            public double PV_Weight { get; set; }
            public double Pkg_Weight { get; set; }
            public string CompanyName { get; set; }
            public string Creator { get; set; }
            public DateTime CreateDate { get; set; }
            public string Modifier { get; set; }
            public DateTime ModifyDate { get; set; }
            public byte State { get; set; }
            public int Pvcount { get; set; }
        }

        public class PvTreatmentVM
        {
            public int Id { get; set; }
            public int Pv_Imp_Id { get; set; }
            public string Aud_Sch_No { get; set; }
            public DateTime TreatmentDate { get; set; }
            public int AL_O_SN_O { get; set; }
            public int AL_O_SN_X { get; set; }
            public int AL_O_SN_N { get; set; }
            public int AL_X_SN_O { get; set; }
            public int AL_X_SN_X { get; set; }
            public int AL_X_SN_N { get; set; }
            public double Weight { get; set; }
            public string CompanyName { get; set; }
            public string Creator { get; set; }
            public DateTime CreateDate { get; set; }
            public string Modifier { get; set; }
            public DateTime ModifyDate { get; set; }
            public byte State { get; set; }
        }
    }
}
