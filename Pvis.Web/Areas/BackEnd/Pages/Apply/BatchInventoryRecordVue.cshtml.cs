using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pvis.Web.Helper;
using Pvis.Biz.Models;
using Pvis.Biz.Member;
using Pvis.Biz.CommEnum;
using Microsoft.AspNetCore.Authorization;

namespace Pvis.Web.Areas.BackEnd.Pages.Apply
{
    [Authorize]
    public class BatchInventoryRecordVueModel : PageModel
    {
        private readonly DataDbContext _context;

        public BatchInventoryRecordVueModel(DataDbContext context)
        {
            _context = context;
        }

        public void OnGet(string No = "")
        {
            if (!string.IsNullOrEmpty(No))
            {
                //表頭內容
                ViewData["layout"] = "_LayoutIframe";
                ViewData["No"] = No;
                
                ViewData["CompanyName"] = _context.RecordItem.Where(q => q.Aud_Sch_No == No).ToList().Count != 0 ? _context.RecordItem.Where(q => q.Aud_Sch_No == No).Select(q => q.CompanyName).First().ToString() : "";

                //廢PV資料
                var ImportData = _context.PvImport.Where(o => o.Aud_Sch_No == No && o.State != 9).ToList();
                ViewData["UserName"] = ImportData.FirstOrDefault()?.CompanyName;
                var Pre_Date = _context.ScheduleAudit.Where(x => x.Aud_Sch_No == No).FirstOrDefault()?.Pre_Date;
                ViewData["Pre_Date"] = Pre_Date?.ToString("yyyy年MM月dd日");
                var TreatmentData = _context.PvTreatment.Where(o => o.Aud_Sch_No == No && o.State != 9).ToList();
                int TotalPiece = ImportData.Select(o => o.AL_O_SN_O + o.AL_O_SN_X + o.AL_O_SN_N + o.AL_X_SN_O + o.AL_X_SN_X + o.AL_X_SN_N).Sum();
                ViewData["PvImportPiece"] = TotalPiece;
                decimal TotalWeight = Convert.ToDecimal(ImportData.Select(o => o.PV_Weight).Sum());
                ViewData["PvWeightTotal"] = TotalWeight;
                ViewData["PkgWeightTotal"] = ImportData.Select(o => o.Pkg_Weight).Sum();

                var AllPvResult = _context.PvTreatment.Where(x => x.Aud_Sch_No == No && x.State == 1).OrderBy(o => o.TreatmentDate).ToList();
                List<PvVM> pvVMs = new List<PvVM>();
                foreach (var item in AllPvResult)
                {
                    var PvVM = new PvVM()
                    {
                        Date = item.TreatmentDate,
                        DealPiece = item.AL_O_SN_O + item.AL_O_SN_X + item.AL_O_SN_N + item.AL_X_SN_O + item.AL_X_SN_X + item.AL_X_SN_N,
                        DealWeight = item.Weight,
                        RemainingPiece = TotalPiece - (item.AL_O_SN_O + item.AL_O_SN_X + item.AL_O_SN_N + item.AL_X_SN_O + item.AL_X_SN_X + item.AL_X_SN_N),
                        RemainingWeight = Convert.ToDecimal(TotalWeight) - Convert.ToDecimal(item.Weight)                    };
                    TotalPiece = PvVM.RemainingPiece;
                    TotalWeight = PvVM.RemainingWeight;
                    pvVMs.Add(PvVM);
                }
                ViewData["WorkDuration"] = pvVMs.Count != 0 ? Pre_Date?.ToString("yyyy年MM月dd日") + " ～ " + pvVMs[pvVMs.Count - 1].Date.ToString("yyyy年MM月dd日") : "";
                ViewData["DailyData"] = pvVMs;

                //廢棄物處裡
                var RecordItemList = _context.RecordItem
                    .Where(q => q.Aud_Sch_No == No)
                    .GroupBy(q => new
                    {
                        q.Code_Type,
                        q.Code_no,
                        q.Code_name,
                    })
                    .Select(q => new
                    {
                        CodeNo = q.Key.Code_no,
                        CodeType = q.Key.Code_Type,
                        CodeName = q.Key.Code_name,
                        Count = q.Count()
                    }).ToList();

                var NewRecordItem = _context.RecordItem
                    .Where(q => q.Aud_Sch_No == No)
                    .Select(q => new
                    {
                        Aud_Sch_No = q.Aud_Sch_No,
                        CodeNo = q.Code_no,
                        CodeType = q.Code_Type,
                        CodeName = q.Code_name + (q.ItemName != null ? "(" + q.ItemName + ")" : ""),
                        Date = q.MakeDate,
                        Weight = q.ItemWeight,
                        Fn = "D"
                    }).ToList();

                var NewRecord = _context.Record
                    .Where(q => q.Aud_Sch_No == No)
                    .Join(_context.RecordItem, q => q.RecordItemID, i => i.RecordIemID, (q, i) => new
                    {
                        Aud_Sch_No = q.Aud_Sch_No,
                        CodeNo = i.Code_no,
                        CodeType = i.Code_Type,
                        CodeName = i.Code_name + (i.Code_no.Substring(4, 2) == "99" && i.Code_Type == "Prod" ? "(" + i.ItemName + ")" : ""),
                        Date = Convert.ToDateTime(q.ShipmentDate),
                        Weight = Convert.ToDouble(q.RecordWeight),
                        Fn = "O"
                    })
                    .ToList();

                List<CountVM> c = new List<CountVM>();
                if (RecordItemList.Count != 0)
                {
                    for (int i = 0; i < RecordItemList.Count; i++)
                    {
                        var a = new CountVM();
                        a.CodeNo = RecordItemList[i].CodeNo.ToString();
                        a.CodeType = RecordItemList[i].CodeType.ToString();
                        a.CodeName = RecordItemList[i].CodeName.ToString();
                        a.Count = RecordItemList[i].Count;

                        c.Add(a);
                    }
                }

                List<RecordVM> r = new List<RecordVM>();

                if (NewRecordItem.Count != 0)
                {
                    for (int i = 0; i < NewRecordItem.Count; i++)
                    {
                        var a = new RecordVM();
                        a.Aud_Sch_No = NewRecordItem[i].Aud_Sch_No.ToString();
                        a.CodeNo = NewRecordItem[i].CodeNo.ToString();
                        a.CodeType = NewRecordItem[i].CodeType.ToString();
                        a.CodeName = NewRecordItem[i].CodeName.ToString();
                        a.Date = NewRecordItem[i].Date;
                        a.Weight = NewRecordItem[i].Weight;
                        a.Fn = NewRecordItem[i].Fn.ToString();

                        r.Add(a);
                    }
                }

                if (NewRecord.Count != 0)
                {
                    for (int i = 0; i < NewRecord.Count; i++)
                    {
                        var a = new RecordVM();
                        a.Aud_Sch_No = NewRecord[i].Aud_Sch_No.ToString();
                        a.CodeNo = NewRecord[i].CodeNo.ToString();
                        a.CodeType = NewRecord[i].CodeType.ToString();
                        a.CodeName = NewRecord[i].CodeName.ToString();
                        a.Date = NewRecord[i].Date;
                        a.Weight = NewRecord[i].Weight;
                        a.Fn = NewRecord[i].Fn.ToString();

                        r.Add(a);
                    }
                }

                ViewData["RecordItemLists"] = c.OrderBy(q => q.CodeNo).ToList();
                ViewData["DailyRecordData"] = r.OrderBy(q => q.Date).ToList();
            }
                

        }

        public class PvVM
        {
            public DateTime Date { get; set; }
            public int DealPiece { get; set; }
            public double DealWeight { get; set; }
            public int RemainingPiece { get; set; }
            public decimal RemainingWeight { get; set; }
        }

        public class CountVM
        {
            public string CodeNo { get; set; }
            public string CodeType { get; set; }
            public string CodeName { get; set; }
            public int Count { get; set; }
        }

        public class RecordVM
        {
            public string Aud_Sch_No { get; set; }
            public string CodeNo { get; set; }
            public string CodeType { get; set; }
            public string CodeName { get; set; }
            public DateTime? Date { get; set; }
            public double? Weight { get; set; }
            public string Fn { get; set; }
        }
    }
}
