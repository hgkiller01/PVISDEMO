using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Member;
using Pvis.Biz.Models;
using Pvis.Web.Helper;
using Microsoft.EntityFrameworkCore;
using Pvis.Biz.Services;
using System.IO;
using Pvis.Biz.Extension;

namespace Pvis.Web.Controller
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ValidateAntiForgeryToken]
    [PvisAuthorize(RoleList.Admin, RoleList.Epa, RoleList.Auditor)]
    public class ProofSingleController : ControllerBase
    {
        private readonly DataDbContext _context;
        public ProofSingleController(DataDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IEnumerable<ProofViewModel> GetProofList()
        {
            var allUserData = AuthHelper.GetUserList().Where(x => x.Role == RoleList.Store || x.Role == RoleList.Teart);
            //所有已經通過的稽查
            var dataComplete = (_context.ScheduleAudit
                .Where(x => x.Aud_State == "Y1" && x.Check_State == "Y1")
                .GroupBy(g => g.Uid)
                .Select(x => new FormModel
                {
                    Uid = x.Key,
                    CompleteCount = x.Count()
                }));
            //所有已經列印過的稽核資料
            var dataFinish = (from Proofs in _context.ProofSingle
                              join Scau in _context.ScheduleAudit
                              on Proofs.Aud_Sch_No equals Scau.Aud_Sch_No
                              where Proofs.IsOutput
                              select new
                              {
                                  Uid = Scau.Uid
                              }).GroupBy(g => g.Uid).Select(x => new FormModel { Uid = x.Key, FinishCount = x.Count() });
            //所有己經列印過的証明單
            var TempFinData = (from Proofs in _context.ProofSingle
                               join Scau in _context.ScheduleAudit
                               on Proofs.Aud_Sch_No equals Scau.Aud_Sch_No
                               where Proofs.IsOutput
                               select new
                               {
                                   Uid = Scau.Uid
                               }).GroupBy(g => g.Uid).Select(x => new FormModel { Uid = x.Key, FinCount = x.Count() });
            //總合
            var ViewData = (from UData in allUserData
                            join Comp in dataComplete on UData.Uid equals Comp.Uid into DL
                            from CompleteData in DL.DefaultIfEmpty()
                            join Fin in dataFinish on (CompleteData == null ? 0 : CompleteData.Uid) equals Fin.Uid into FL
                            from FinshData in FL.DefaultIfEmpty()
                            join TFinData in TempFinData on (FinshData == null ? 0 : FinshData.Uid) equals TFinData.Uid into FT
                            from TFD in FT.DefaultIfEmpty()
                            select new ProofViewModel
                            {
                                Uid = UData.Uid,
                                CompanyName = UData.CompanyName,
                                RoleKind = UData.Role,
                                CompleteCount = CompleteData != null ? CompleteData.CompleteCount : 0,
                                UnCompleteCount = (CompleteData != null ? CompleteData.CompleteCount : 0) -
                                (FinshData != null ? FinshData.FinishCount : 0),
                                FinishCount = TFD == null ? 0 : TFD.FinCount
                            }).Where(x => x.CompleteCount > 0).ToList();
            return ViewData;
        }
        [HttpPost]
        public async Task<AudListModel> GetAudList(IDdata ddata)
        {
            var CompanyUser = await AuthHelper.GetUserQuery().Where(x => x.Uid == ddata.Uid).FirstOrDefaultAsync();
            var WhereQuery = _context.ProofSingle.Where(p => p.Uid == ddata.Uid).Select(x => x.Aud_Sch_No).ToList();
            var allQuery = _context.ScheduleAudit.Where(x => x.Uid == ddata.Uid && x.Aud_State == "Y1" && x.Check_State == "Y1")
                .Where(w => !(WhereQuery.Contains(w.Aud_Sch_No)))
                .Select(x => x.Aud_Sch_No);
            AudListModel audList = new AudListModel()
            {
                Aud_Sch_No = await allQuery.ToListAsync(),
                CompanyName = CompanyUser.CompanyName,
                RoleKind = CompanyUser.Role
            };
            return audList;
        }
        [HttpPost]
        public async Task<ProofSingle> GetDataEditing(ProofData proofData)
        {
            ProofSingle proofSingle = await _context.ProofSingle.Where(x => x.ProofNo == proofData.ProofNo).FirstOrDefaultAsync();
            FormFileUpload upload = await _context.FormFileUpload.Where(x => x.AppId == proofSingle.PsId.ToString()
            && x.DocType == eDocType.ProofSingle && x.ItemType == eItemType.AuditDoc).FirstOrDefaultAsync();
            if (upload != null)
            {
                proofSingle.UrlContent = upload.FilePath;
                proofSingle.OriginalFileName = upload.OriginalFileName;
            }
            else
            {
                proofSingle.UrlContent = "";
                proofSingle.OriginalFileName = "";
            }
            return proofSingle;
        }
        [HttpPost]
        public async Task<SbViewModel> GetProofSBList(IDdata model)
        {
            string CompanyName = AuthHelper.GetUserQuery().Where(x => x.Uid == model.Uid).Select(x => x.CompanyName).FirstOrDefault();
            var data = _context.ProofSingle.Select(x => new SBListModel()
            {
                DownLoadUrl = _context.FormFileUpload.Where(p => p.AppId == x.PsId.ToString()
                && p.DocType == eDocType.ProofSingle
                && p.ItemType == eItemType.AuditDoc).FirstOrDefault().FilePath,
                Proof = x,
                Bookings = _context.ScheduleAudit_SB.Where(s => s.Aud_Sch_No == x.Aud_Sch_No).Select(b => b.Bookingno),
                Uid = x.Uid
            }).Where(w => w.Uid == model.Uid);
            var modeldata = new SbViewModel()
            {
                CompanyName = CompanyName,
                models = await data.ToListAsync()
            };
            return modeldata;
        }
        [HttpPost]
        public async Task<ResultData> GetData(AudNo audNo)
        {
            var BookingNos = _context.ScheduleAudit_SB.Where(x => x.Aud_Sch_No == audNo.Id).Select(s => s.Bookingno).ToList();
            //清除稽核證明
            var cleData = await (from Scdule in _context.Schedule
                                 join ScduleSB in _context.Schedule_SB on Scdule.Cle_Sch_No equals ScduleSB.Cle_Sch_No
                                 where BookingNos.Contains(ScduleSB.Bookingno)
                                 select new
                                 {
                                     Cle_Name = Scdule.Cle_Name,
                                     Month = _context.ProofSingle.Where(x => x.ProofNo == audNo.ProofNo).FirstOrDefault().Month,
                                     Year = _context.ProofSingle.Where(x => x.ProofNo == audNo.ProofNo).FirstOrDefault().Year,
                                     bookingno = ScduleSB.Bookingno,
                                     EmptyCar_Weight = Scdule.EmptyCar_Weight ?? 0,
                                     OutPutDate = _context.ProofSingle.Where(x => x.ProofNo == audNo.ProofNo).FirstOrDefault().OutPutDate,
                                     Pvimport = _context.PvImport.Where(x => x.Aud_Sch_No == audNo.Id).ToList(),
                                     Cle_Date = Scdule.Cle_Date,
                                     CleDesc = _context.ProofSingle.Where(x => x.ProofNo == audNo.ProofNo).FirstOrDefault().CleDesc
                                 }).ToListAsync();
            //清除稽核證明->案場排出申請登記編號
            var ViewDatas = (cleData.GroupBy(x => x.bookingno).Select(s => new CleSubData()
            {
                Bookingno = s.Key,
                CarWeight = s.Sum(x => x.EmptyCar_Weight),
                PvCount = s.Sum(x => x.Pvimport.Where(x => x.BookingNoId == s.Key)
                .Sum(p => p.AL_O_SN_N + p.AL_O_SN_O + p.AL_O_SN_X + p.AL_X_SN_N + p.AL_X_SN_O + p.AL_X_SN_X)),
                PvWeight = s.Sum(x => x.Pvimport.Where(x => x.BookingNoId == s.Key).Sum(p => p.PV_Weight + p.Pkg_Weight)),
            })).ToList();
            ResultData result = new ResultData()
            {
                //清除稽核證明
                cleViewData = new CleViewData()
                {
                    Cle_Name = cleData.FirstOrDefault().Cle_Name,
                    Month = cleData.FirstOrDefault().Month,
                    Year = cleData.FirstOrDefault().Year,
                    OutPutDate = cleData.FirstOrDefault().OutPutDate,
                    CleDesc = cleData.FirstOrDefault().CleDesc,
                    SubDatas = ViewDatas
                }
            };
            List<MyAppUser> myAppUsers = AuthHelper.GetUserList();
            var scheduleAudits = _context.ScheduleAudit.Where(x => x.Aud_Sch_No == audNo.Id).FirstOrDefault();
            var CompanyData = myAppUsers.Where(x => x.Uid == scheduleAudits.Uid).FirstOrDefault();
            //處理稽核證明
            AuditData auditData = new AuditData()
            {
                CompanyName = CompanyData.CompanyName,
                CompanyRole = CompanyData.Role,
                Month = result.cleViewData.Month,
                Year = result.cleViewData.Year,
                OutPutDate = result.cleViewData.OutPutDate,
                OtherDesc = _context.ProofSingle.Where(x => x.ProofNo == audNo.ProofNo).FirstOrDefault()?.OtherDesc
            };
            //處理稽核證明->案場排出申請登記,稽核處理量
            var pvimport = _context.PvImport.Where(x => x.Aud_Sch_No == audNo.Id).GroupBy(g => g.BookingNoId)
                .Select(s => new AuditSubData()
                {
                    Bookingno = s.Key,
                    AL_O_SN_O = s.Sum(x => x.AL_O_SN_O),
                    AL_O_SN_X = s.Sum(x => x.AL_O_SN_X),
                    AL_O_SN_N = s.Sum(x => x.AL_O_SN_N),
                    AL_X_SN_O = s.Sum(x => x.AL_X_SN_O),
                    AL_X_SN_X = s.Sum(x => x.AL_X_SN_X),
                    AL_X_SN_N = s.Sum(x => x.AL_X_SN_N),
                    PvTotalWeight = s.Sum(x => x.PV_Weight)
                });
            auditData.auditSubData = pvimport.ToList();
            result.auditData = auditData;
            try
            {
                //案場排出允收稽核證明
                var preData = (from sa in _context.ScheduleAudit
                               join ssb in _context.ScheduleAudit_SB on sa.Aud_Sch_No equals ssb.Aud_Sch_No
                               join scb in _context.ScrapBooking on ssb.Bookingno equals scb.Bookingno
                               let CompleteDate = _context.PvTreatment.Where(x => x.Aud_Sch_No == sa.Aud_Sch_No)
                               .OrderByDescending(o => o.TreatmentDate).Select(s => s.TreatmentDate).FirstOrDefault()
                               join shb in _context.Schedule_SB on scb.Bookingno equals shb.Bookingno
                               join sl in _context.Schedule on shb.Cle_Sch_No equals sl.Cle_Sch_No
                               join spinfo in _context.UserSpInfo on scb.Pid equals spinfo.Sbid
                               let SpQty = _context.UserPvInfo.Where(w => w.Pid == spinfo.Pvid).FirstOrDefault().SpQty
                               let pvno = _context.UserPvInfo.Where(w => w.Pid == spinfo.Pvid).FirstOrDefault().Pvno
                               where sa.Aud_Sch_No == audNo.Id
                               select new
                               {
                                   CaseUid = spinfo.Uid,
                                   AuditUid = sa.Uid,
                                   BookingNo = scb.Bookingno,
                                   Cle_Name = sl.Cle_Name,
                                   Cle_Date = sl.Cle_Date,
                                   CompleteDate = CompleteDate,
                                   SpQty = SpQty,
                                   NowQty = _context.PvTreatment.Where(x => x.Aud_Sch_No == sa.Aud_Sch_No)
                                   .Sum(s => s.AL_O_SN_N + s.AL_O_SN_O + s.AL_O_SN_X + s.AL_X_SN_N + s.AL_X_SN_O + s.AL_X_SN_X),
                                   pvno = pvno,
                                   Haveframe = _context.PvTreatment.Where(x => x.Aud_Sch_No == sa.Aud_Sch_No)
                                   .Sum(s => s.AL_O_SN_O + s.AL_O_SN_N + s.AL_O_SN_X),
                                   NoFrame = _context.PvTreatment.Where(x => x.Aud_Sch_No == sa.Aud_Sch_No)
                                   .Sum(s => s.AL_X_SN_N + s.AL_X_SN_O + s.AL_X_SN_X),
                               }).Distinct().ToList();
                var preResult = (preData.Select(p => new UserPvData()
                {
                    CaseName = myAppUsers.Where(x => x.Uid == p.CaseUid).FirstOrDefault().CompanyName,
                    CleanName = p.Cle_Name,
                    AuditName = myAppUsers.Where(x => x.Uid == p.AuditUid).FirstOrDefault().CompanyName,
                    Cle_Date = p.Cle_Date,
                    BookingNo = p.BookingNo,
                    CompleteDate = p.CompleteDate,
                    SpQty = p.SpQty,
                    NowQty = p.NowQty,
                    Pvno = p.pvno,
                    Haveframe = p.Haveframe,
                    NoFrame = p.NoFrame,
                    PreDesc = _context.ProofSingle.Where(x => x.ProofNo == audNo.ProofNo).FirstOrDefault()?.PreDesc
                })).Distinct().ToList();
                result.userPvData = preResult;
            }
            catch (Exception e)
            {
                string error = e.ToString();
            }

            return result;

        }
        [HttpPost]
        public ProofSingle SaveProof(ProofSBmodel sBmodel)
        {
            var OpUser = AuthHelper.GetUserQuery().Where(x => x.Uid == sBmodel.Data.Uid).FirstOrDefault();
            string HeadNumber = OpUser.UserName + (DateTime.Now.Year - 1911) + DateTime.Now.ToString("MM");
            string CurrentString = _context.ProofSingle.Where(x => x.ProofNo.StartsWith(HeadNumber))
                .ToList().OrderByDescending(o => int.Parse(o.ProofNo.Substring(HeadNumber.Length, 2)))
                .FirstOrDefault()?.ProofNo.Substring(HeadNumber.Length, 2);
            int CurrentNumber = 1;
            if (!string.IsNullOrEmpty(CurrentString))
            {
                CurrentNumber = Convert.ToInt32(CurrentString) + 1;
            }
            string ProofNo = HeadNumber + CurrentNumber.ToString("00");

            if (sBmodel.Action == "ADD")
            {
                sBmodel.Data.CreateUserId = User.GetUid();
                sBmodel.Data.CreateDate = DateTime.Now;
                sBmodel.Data.IsOutput = false;
                sBmodel.Data.ProofNo = ProofNo;
                _context.Add(sBmodel.Data).State = EntityState.Added;
                _context.SaveChanges();
            }
            else
            {
                var proofData = _context.ProofSingle.Where(x => x.PsId == sBmodel.Data.PsId).FirstOrDefault();
                proofData.Aud_Sch_No = sBmodel.Data.Aud_Sch_No;
                proofData.CleDesc = sBmodel.Data.CleDesc;
                proofData.IsOutput = sBmodel.Data.IsOutput;
                proofData.PreDesc = sBmodel.Data.PreDesc;
                proofData.Month = sBmodel.Data.Month;
                proofData.OtherDesc = sBmodel.Data.OtherDesc;
                proofData.OutPutDate = sBmodel.Data.OutPutDate;
                proofData.Year = sBmodel.Data.Year;
                _context.SaveChanges();
                var hasfile = _context.FormFileUpload.Where(x =>
                x.AppId == sBmodel.Data.PsId.ToString() &&
                x.DocType == eDocType.ProofSingle &&
                x.ItemType == eItemType.AuditDoc
                ).FirstOrDefault();
                if (hasfile != null)
                {
                    var _SavePath = FormFileUploadBusinessLayer.GetSavePath(hasfile);
                    if (System.IO.File.Exists(_SavePath))
                    {
                        System.IO.File.Delete(_SavePath);
                    }
                    _context.Entry(hasfile).State = EntityState.Deleted;
                    _context.SaveChanges();
                }
            }
            FormFileUpload File = null;
            var NowFile = sBmodel.Attachment.FirstOrDefault();
            var defaultFile = default(KeyValuePair<eItemType, Pvis.Biz.ViewModels.AttachmentViewModel>);
            if (!NowFile.Equals(defaultFile))
            {
                File = FormFileUploadBusinessLayer.FillFileData(NowFile.Value,
                new FormFileUpload()
                {
                    AppId = sBmodel.Data.PsId.ToString(),
                    DocType = eDocType.ProofSingle,
                    ItemType = NowFile.Key
                }, User.GetUid());
                if (FileCheck.IsAllowedExtension(NowFile.Value, File,_context, "pdf", "稽核量", Request.HttpContext.Connection.RemoteIpAddress.ToString()))
                {
                    try
                    {
                        FormFileUploadBusinessLayer.SetFilePath(File);
                        Directory.CreateDirectory(Path.GetDirectoryName(FormFileUploadBusinessLayer.GetSavePath(File)));
                        System.IO.File.WriteAllBytes(FormFileUploadBusinessLayer.GetSavePath(File), NowFile.Value.GetContent());
                        _context.Add(File).State = EntityState.Added;
                    }
                    catch (Exception ex)
                    {
                        FileCheck.WriteErrorFile(File,_context, ex.Message, "稽核量", Request.HttpContext.Connection.RemoteIpAddress.ToString());
                    }
                }


            }
            _context.SaveChanges();
            return sBmodel.Data;
        }
        [HttpPost]
        public ProofSingle ChangeProof(ProofSingle proofSingle)
        {
            proofSingle.OutPutDate = DateTime.Now;
            proofSingle.IsOutput = true;
            _context.Entry(proofSingle).State = EntityState.Modified;
            _context.SaveChanges();
            return proofSingle;
        }
        [HttpPost]
        public int DeleteProof(ProofData proofData)
        {
            var deleteItem = _context.ProofSingle.Where(x => x.ProofNo == proofData.ProofNo).FirstOrDefault();
            int Uid = deleteItem.Uid;
            _context.Entry(deleteItem).State = EntityState.Deleted;
            _context.SaveChanges();
            return Uid;
        }
    }
    public class ResultData
    {
        public CleViewData cleViewData { get; set; }
        public AuditData auditData { get; set; }
        public List<UserPvData> userPvData { get; set; }
    }
    public class UserPvData
    {
        public string CaseName { get; set; }
        public string CleanName { get; set; }
        public string AuditName { get; set; }
        public DateTime Cle_Date { get; set; }
        public DateTime CompleteDate { get; set; }
        public int SpQty { get; set; }
        public string Pvno { get; set; }
        public int Haveframe { get; set; }
        public int NoFrame { get; set; }
        public string BookingNo { get; set; }
        public int NowQty { get; set; }
        public string PreDesc { get; set; }
    }
    public class AuditData
    {
        public string CompanyName { get; set; }
        public RoleList CompanyRole { get; set; }
        public int Month { get; set; }
        public int? Year { get; set; }
        public string OtherDesc { get; set; }
        public DateTime? OutPutDate { get; set; }
        public List<AuditSubData> auditSubData { get; set; }
    }
    public class AuditSubData
    {
        public string Bookingno { get; set; }
        public int AL_O_SN_O { get; set; }
        public int AL_O_SN_X { get; set; }
        public int AL_O_SN_N { get; set; }
        public int AL_X_SN_O { get; set; }
        public int AL_X_SN_X { get; set; }
        public int AL_X_SN_N { get; set; }
        public double PvTotalWeight { get; set; }
    }
    public class CleViewData
    {
        public string Cle_Name { get; set; }
        public int Month { get; set; }
        public int? Year { get; set; }
        public List<CleSubData> SubDatas { get; set; }
        public DateTime? OutPutDate { get; set; }
        public string CleDesc { get; set; }
    }
    public class CleSubData
    {
        public string Bookingno { get; set; }
        public decimal CarWeight { get; set; }
        public int PvCount { get; set; }
        public double PvWeight { get; set; }
    }

    public class AudListModel
    {
        public IEnumerable<string> Aud_Sch_No { get; set; }
        public string CompanyName { get; set; }
        public RoleList RoleKind { get; set; }
    }
    public class SbViewModel
    {
        public string CompanyName { get; set; }
        public List<SBListModel> models { get; set; }
    }
    public class SBListModel
    {
        public ProofSingle Proof { get; set; }
        public string DownLoadUrl { get; set; }
        public IEnumerable<string> Bookings { get; set; }
        public int Uid { get; set; }
    }
    public class AudNo
    {
        public string Id { get; set; }
        public string ProofNo { get; set; }
    }
    public class ProofData
    {
        public string ProofNo { get; set; }
    }
    public class IDdata
    {
        public int Uid { get; set; }
    }
    public class ProofSBmodel
    {
        public ProofSingle Data { get; set; }
        public string Action { get; set; }
        public Dictionary<eItemType, Pvis.Biz.ViewModels.AttachmentViewModel> Attachment { internal get; set; }
    }
    public class SingleViewModel
    {
        public string ProofNo { get; set; }
        public string MyProperty { get; set; }
    }
    public class FormModel
    {
        public int Uid { get; set; }
        public int CompleteCount { get; set; }
        public int FinishCount { get; set; }
        public int FinCount { get; set; }
    }
    public class ProofViewModel
    {
        public int Uid { get; set; }
        public RoleList RoleKind { get; set; }
        public string CompanyName { get; set; }
        public int? CompleteCount { get; set; }
        public int? UnCompleteCount { get; set; }
        public int? FinishCount { get; set; }
    }
}
