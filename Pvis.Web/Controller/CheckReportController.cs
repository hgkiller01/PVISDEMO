using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Member;
using Pvis.Biz.Models;

namespace Pvis.Web.Controller
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [ValidateAntiForgeryToken]
    [Authorize]
    public class CheckReportController : ControllerBase
    {
        private readonly DataDbContext _context;
        public CheckReportController(DataDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public HeadData GetData(Sch_No No)
        {
            var tempData = (from sa in _context.ScheduleAudit
                           join ss in _context.ScheduleAudit_SB
                           on sa.Aud_Sch_No equals ss.Aud_Sch_No
                           where sa.Aud_Sch_No == No.Aud_Sch_No
                           select new { ss, sa }).ToList();
            DateTime? startDate = tempData.Select(x => x.sa.Pre_Date).FirstOrDefault();
            DateTime? endDate = _context.Record.Where(x => x.Aud_Sch_No == No.Aud_Sch_No)
                .Select(x => x.ShipmentDate).OrderByDescending(x => x).FirstOrDefault();
            HeadData hd = new HeadData()
            {
                StartDate = startDate?.ToString("yyyy年MM月dd日") ?? "" ,
                EndDate = endDate?.ToString("yyyy年MM月dd日") ?? "",
                SchCount = tempData.Count(),
                CompanyName = tempData.Select(x => x.sa.CompanyName).FirstOrDefault()
            };
            hd.Details = _context.PvImport.Where(x => x.Aud_Sch_No == No.Aud_Sch_No).Where(w => w.State == 1).Select(x => new HeadDetail
            {
                BookingNo = x.BookingNoId,
                PvCount = x.AL_O_SN_N + x.AL_O_SN_O + x.AL_O_SN_X + x.AL_X_SN_N + x.AL_X_SN_O + x.AL_X_SN_X,
                Weight = x.PV_Weight,
                Pkg_Weight = x.Pkg_Weight
            }).ToList();
            hd.TotalCount = hd.Details.Sum(x => x.PvCount);
            hd.TotalWeight = hd.Details.Sum(x => x.Weight);
            hd.PkgTotalWeight = hd.Details.Sum(x => x.Pkg_Weight);
            hd.itemTotals = _context.RecordItem.Where(x => x.Aud_Sch_No == No.Aud_Sch_No)
            .GroupBy(x => new { x.Code_no, x.Code_name, x.Process }).Select(g => new ItemTotal()
            {
                Code_name = g.Key.Code_name,
                ItemWeight = g.Sum(s => s.ItemWeight.Value),
                Process = g.Key.Process
            }).ToList();
            hd.recordLists = (from rd in _context.Record
                             join ri in _context.RecordItem
                             on rd.RecordItemID equals ri.RecordIemID
                             where rd.Aud_Sch_No == No.Aud_Sch_No
                             select new RecordList()
                             {
                                 Code_name = ri.Code_name,
                                 ItemName = ri.ItemName,
                                 Code_no = ri.Code_no,
                                 VendorName = rd.VendorName,
                                 UseFor = rd.UseFor,
                                 ExportNumber = rd.ExportNumber,
                                 CountryName = rd.CountryName
                             }).ToList();
            return hd;
        }
    }
    public class RecordList
    {
        public string Code_name { get; set; }
        public string ItemName { get; set; }
        public string Code_no { get; set; }
        public string VendorName { get; set; }
        public string UseFor { get; set; }
        public string ExportNumber { get; set; }
        public string CountryName { get; set; }
    }
    public class Sch_No
    {
        public string Aud_Sch_No { get; set; }
    }
    public class ItemTotal
    {
        public string Code_name { get; set; }
        public double ItemWeight { get; set; }
        public int Process { get; set; }
    }
    public class HeadData
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public List<HeadDetail> Details { get; set; }
        public int SchCount { get; set; }
        public string CompanyName { get; set; }
        public int TotalCount { get; set; }
        public double TotalWeight { get; set; }
        public double PkgTotalWeight { get; set; }
        public List<ItemTotal> itemTotals { get; set; }
        public List<RecordList> recordLists { get; set; }
    }
    public class HeadDetail
    {
        public string BookingNo { get; set; }
        public int PvCount { get; set; }
        public double Weight { get; set; }
        public double Pkg_Weight { get; set; }
    }
}
