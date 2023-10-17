using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LinqKit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Extension;
using Pvis.Biz.Member;
using Pvis.Biz.Models;
using Pvis.Biz.ViewModels;

namespace Pvis.Biz.Services
{
    public class AccountAppBusinessLayer
    {
        public static Dictionary<byte, string> AppStatusCode
        {
            get
            {
                var code = new Dictionary<byte, string>();
                foreach (AppStatusList item in Enum.GetValues(typeof(AppStatusList)))
                {
                    code.Add((byte)item, EnumHelper<AppStatusList>.GetDisplayValue(item));
                }
                return code;
            }
        }

        public static Dictionary<byte, string> AppRoleCode
        {
            get
            {
                var code = new Dictionary<byte, string>();
                foreach (AppRoleList item in Enum.GetValues(typeof(AppRoleList)))
                {
                    code.Add((byte)item, EnumHelper<AppRoleList>.GetDisplayValue(item));
                }
                return code;
            }
        }

        public object FileCheck { get; private set; }

        private readonly DataDbContext context;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ApplicationDbContext appcontext;

        private static IMapper mapper;
        static AccountAppBusinessLayer()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AccountApp, AccountAppViewModel>();
                cfg.CreateMap<AccountAppViewModel, AccountApp>();
            });
            mapper = configuration.CreateMapper();
        }

        public AccountAppBusinessLayer(
            DataDbContext context,
            ApplicationDbContext appcontext,
            IHttpContextAccessor httpContextAccessor)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
            this.appcontext = appcontext;
        }

        public async Task<List<string>> SendReview(AccountAppViewModel rec)
        {
            List<string> errors = new List<string>();

            AccountApp _DbRec = new AccountApp();

             _DbRec = context.AccountApp.Where(x => x.Pid == rec.Pid).FirstOrDefault();

            if (_DbRec == null) errors.Add("案件不存在");

            if (_DbRec.Status == AppStatusList.accept) errors.Add("本案已通過無法變更審查結果");

            if (_DbRec.Status == AppStatusList.reject) errors.Add("本案已退件無法變更審查結果");

            if (rec.Status == AppStatusList.reject && (string.IsNullOrWhiteSpace(rec.RejectReason) || rec.RejectReason.Length <= 3))
            {
                errors.Add("不通過請務必填寫審查意見");
            }

            if (errors.Any()) return errors;

            //狀態為通過產生系統自動配號處理
            if (rec.Status == AppStatusList.accept)
            {
                GetNewControlNo(_DbRec);
                rec.ControlNo = _DbRec.ControlNo;
            }

            _DbRec.Status = rec.Status;
            _DbRec.ReviewDt = _DbRec.ModDt = DateTime.Now;
            _DbRec.RejectReason = rec.RejectReason;
            _DbRec.ModUid = _DbRec.ReviewUid = httpContextAccessor.HttpContext.User.GetUid();

            context.Attach(_DbRec).State = EntityState.Modified;

            await context.SaveChangesAsync();

            return errors;

        }

        /// <summary>
        /// 取得新控制編號
        /// </summary>
        /// <returns></returns>
        private void GetNewControlNo(AccountApp _DbRec)
        {
            if (!String.IsNullOrEmpty(_DbRec.ControlNo)) return;

            var CountyId = _DbRec.TownId.Substring(0, 1);
            var _MaxControlNo = context.AccountApp
                .Where(x => EF.Functions.Like(x.TownId, CountyId+"%") && x.ControlNo != null && x.ControlNo != String.Empty )
                .AsEnumerable()
                .Max(x => x.ControlNo.Substring(2));

            int.TryParse(_MaxControlNo, out int _MaxNo);
            _MaxNo += 1;

            var RoleCode = new Dictionary<AppRoleList, string>() {
                { AppRoleList.AppPersonal , "A" } ,
                { AppRoleList.AppCompany , "A" } ,
                { AppRoleList.AppStore , "B" } ,
                { AppRoleList.AppTreat , "C" }
            };

            if (!RoleCode.ContainsKey(_DbRec.UserRole)) throw new Exception("申請角色對應錯誤");

            _DbRec.ControlNo = RoleCode[_DbRec.UserRole] + CountyId + String.Format("{0:000000}", _MaxNo);
        }

        /// <summary>
        /// 未登入前使用者帳號申請表填寫
        /// </summary>
        /// <param name="rec"></param>
        /// <returns></returns>
        public async Task<List<string>> Apply(AccountAppViewModel rec, string IP = "")
        {

            rec.CreateDt = DateTime.Now;
            rec.Status = AppStatusList.none;
            rec.RejectReason = null;
            rec.IPAddress = IP;
            var errors = new List<string>();

            if (rec.UserRole == AppRoleList.AppTreat)
            {
                if (String.IsNullOrWhiteSpace(rec.EuicNo)) errors.Add("管制編號尚未填寫");
                if (String.IsNullOrWhiteSpace(rec.CompanyName)) errors.Add("機構名稱尚未填寫");
                
            }

            if (new List<AppRoleList>() { AppRoleList.AppCompany, AppRoleList.AppStore }.Contains(rec.UserRole))
            {
                if (String.IsNullOrWhiteSpace(rec.CompanyName)) errors.Add("機構名稱尚未填寫");
            }
            var RepeatCompanys = appcontext.Users.Where(x => x.CompanyName == rec.CompanyName && x.Address == rec.Address).ToList();
            switch (rec.UserRole)
            {
                case AppRoleList.AppCompany:
                    RepeatCompanys = RepeatCompanys.Where(x => x.Role == RoleList.Company).ToList();
                    if (RepeatCompanys.Count > 0)
                        errors.Add("已經有此案場業者名稱");
                    break;
                case AppRoleList.AppStore:
                    RepeatCompanys = RepeatCompanys.Where(x => x.Role == RoleList.Store).ToList();
                    if (RepeatCompanys.Count > 0)
                        errors.Add("已經有此貯存業者名稱");
                    break;
                case AppRoleList.AppTreat:
                    RepeatCompanys = RepeatCompanys.Where(x => x.Role == RoleList.Teart).ToList();
                    if (RepeatCompanys.Count > 0)
                        errors.Add("已經有此處理業者名稱");
                    break;
            }
            //if ( context.AccountApp.Where(x => x.Email == rec.Email).Any() || appcontext.Users.Where(x => x.Email == rec.Email).Any() ) {
            //    errors.Add("Email已重複");
            //}

            var Tows = await context.Town.Where(x =>
                 rec.Address.StartsWith(x.CountyName) &&
                 rec.Address.StartsWith(x.CountyName + x.TownName))
                .Take(2).ToListAsync();

            if (rec.Attachment == null) rec.Attachment = new Dictionary<eItemType, AttachmentViewModel>();

            if (Tows.Count() != 1) errors.Add("輸入地址不正確");

            if (rec.Attachment.Any(x => x.Value.FileExtName != "pdf") ) errors.Add("相關作證附件請用pdf格式");

            if (new AppRoleList[] { AppRoleList.AppCompany, AppRoleList.AppPersonal }.Contains(rec.UserRole) &&
                 rec.IsNotOwner && !rec.Attachment.ContainsKey(eItemType.AccountApp_LetterOfProxy)) {
                errors.Add("『授權委託書』未提供");
            }

            if (new AppRoleList[] { AppRoleList.AppCompany, AppRoleList.AppPersonal }.Contains(rec.UserRole) &&
                 !rec.Attachment.ContainsKey(eItemType.AccountApp_LetterOfAgreement))
            {
                errors.Add("『設備登記同意函』未提供");
            }

            if (new AppRoleList[] { AppRoleList.AppStore }.Contains(rec.UserRole) &&
                 !rec.Attachment.ContainsKey(eItemType.AccountApp_ApprovalOfInstallations))
            {
                errors.Add("『貯存場設置核准函』未提供");
            }

            if (rec.UserRole == AppRoleList.AppTreat && String.IsNullOrWhiteSpace(rec.EuicNo)) {
                errors.Add("『事業廢棄物管制編號』未提供");
            }

            foreach(var item in rec.Attachment)
            {
                var temp = IsAllowedExtension(item.Value);
                if (temp != FileExtension.PDF)
                {
                    FileUploadErrorLog log = new FileUploadErrorLog()
                    {
                        DocType = eDocType.AccountAppDoc,
                        ItemType = item.Key,
                        ErrorDate = DateTime.Now,
                        ErrorMessage = "上傳非PDF檔",
                        IPAddress = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                        OriginalFileName = item.Value.name + item.Value.FileExtName,
                        UploadFunction = "申請帳號"
                    };
                    context.FileUploadErrorLog.Add(log);
                    context.SaveChanges();
                    errors.Add("只能上傳PDF檔");
                    continue;
                }
            }

            if (errors.Any()) return errors;

            rec.TownId = Tows[0].TownId;

            var _Rec = mapper.Map<AccountAppViewModel, AccountApp>(rec, new AccountApp()
            {
                IsGetEuicNo = String.IsNullOrEmpty(rec.EuicNo)
            });

            using (var transaction = context.Database.BeginTransaction()) {

                context.Add(_Rec).State = EntityState.Added;

                await context.SaveChangesAsync();

                foreach (var item in rec.Attachment)
                {

                    var F = new FormFileUpload()
                    {
                        AppId = _Rec.Pid.ToString(),
                        ItemType = item.Key,
                        DocType = eDocType.AccountAppDoc,
                        FileExtName = item.Value.FileExtName,
                        OriginalFileName = item.Value.name,
                        FileSize = item.Value.size,
                        CreateUid = -1,
                        CreateDt = DateTime.Now,
                        ModUid = -1,
                        ModDt = DateTime.Now
                    };
                    try
                    {
                        FormFileUploadBusinessLayer.SetFilePath(F);
                        Directory.CreateDirectory(Path.GetDirectoryName(FormFileUploadBusinessLayer.GetSavePath(F)));
                        File.WriteAllBytes(FormFileUploadBusinessLayer.GetSavePath(F), item.Value.GetContent());
                    }catch(Exception ex)
                    {
                        FileUploadErrorLog log = new FileUploadErrorLog()
                        {
                            DocType = eDocType.AccountAppDoc,
                            ItemType = item.Key,
                            ErrorDate = DateTime.Now,
                            ErrorMessage = ex.Message,
                            IPAddress = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                            OriginalFileName = item.Value.name,
                            UploadFunction = "申請帳號"
                        };
                        context.FileUploadErrorLog.Add(log);
                        context.SaveChanges();
                    }

                    context.Add(F).State = EntityState.Added;
                }
                await context.SaveChangesAsync();
                transaction.Commit();
            }

            return errors;
        }
        public FileExtension IsAllowedExtension(AttachmentViewModel attachment)
        {
            Stream stream = new MemoryStream(attachment.GetContent());
            BinaryWriter binWriter = new BinaryWriter(stream);
            BinaryReader binReader =
                new BinaryReader(binWriter.BaseStream);
            //BinaryReader r = new BinaryReader();
            string fileclass = "";
            //這裡的位長要具體判斷.
            byte buffer;
            try
            {
                buffer = binReader.ReadByte();
                fileclass = buffer.ToString();
                buffer = binReader.ReadByte();
                fileclass += buffer.ToString();
            }
            catch
            {
            }
            binReader.Close();
            return (FileExtension)Enum.Parse(typeof(FileExtension), fileclass);
        }
        public async Task<AccountAppViewModel> GetItem(AccountAppQry qry)
        {
            if (qry.Pid <= 0) return null;
            var _item = (await GetList(qry)).FirstOrDefault();
            await _item.GetAttListAsync(context);
            return _item;
        }

        public async Task<IEnumerable<AccountAppViewModel>> GetList(AccountAppQry qry)
        {
            var pred = PredicateBuilder.New<AccountApp>(true);

            if (qry.Pid > 0) pred.And(x => x.Pid == qry.Pid);

            if (qry.UserRole.HasValue) pred.And(x => x.UserRole == qry.UserRole);

            if (qry.Status.HasValue) pred.And(x => x.Status == qry.Status);

            if (qry.StartDt.HasValue) pred.And(x => x.CreateDt >= qry.StartDt.Value);

            if (qry.EndDt.HasValue) pred.And(x => x.CreateDt <= qry.EndDt.Value);

            if (!String.IsNullOrWhiteSpace(qry.KeyWord)) pred.And(x => x.CompanyName.Contains(qry.KeyWord) || x.UserName.Contains(qry.KeyWord));

            return await context.AccountApp.Where(pred)
                .OrderBy(x => x.CreateDt)
                .Select(x => mapper.Map<AccountAppViewModel>(x))
                .Take(qry.TopN)
                .ToArrayAsync();

        }
    }

    /// <summary>
    /// 查詢條件
    /// </summary>
    public class AccountAppQry
    {

        public int Pid { get; set; }
        [Range(0, 2000, ErrorMessage = "超出單次查詢筆數上線")]
        public int TopN { get; set; } = 1000;

        public AppRoleList? UserRole { get; set; }

        public AppStatusList? Status { get; set; }

        /// <summary>
        /// 申請時間(起)
        /// </summary>
        public DateTime? StartDt { get; set; }

        /// <summary>
        /// 申請時間(迄)
        /// </summary>
        public DateTime? EndDt { get; set; }

        /// <summary>
        /// 關鍵字查詢
        /// </summary>
        public String KeyWord { get; set; }
    }


    public class AccountAppViewModel
    {
        public string IPAddress { get; set; }
        public int Pid { get; set; }
        [Required(ErrorMessage = "帳號申請者姓名未填寫")]
        [StringLength(250, ErrorMessage = "帳號申請者姓名最多250字元")]
        public string UserName { get; set; }
        [StringLength(250, ErrorMessage = "公司名稱最多250字元")]
        public string CompanyName { get; set; } = string.Empty;
        [Required(ErrorMessage = "地址資料尚未填寫")]
        [StringLength(250, ErrorMessage = "地址最多250字元")]
        public string Address { get; set; }
        [Required(ErrorMessage = "聯絡電話未填寫")]
        [StringLength(120)]
        [RegularExpression("^[0-9]{9,10}#{0,1}[0-9]{0,20}", ErrorMessage = "電話格式不正確(EX:0277548822#124)")]
        public string Tel { get; set; }
        [Required(ErrorMessage = "Email未填寫")]
        [StringLength(120, ErrorMessage = "Email最多120字元")]
        [EmailAddress(ErrorMessage = "Email格式錯誤")]
        public string Email { get; set; }
        [RegularExpression("^([A-Z][0-9]{2}[A-Z0-9]{5}|)$", ErrorMessage = "管制編號格式錯誤")]
        public string EuicNo { get; set; }
        [Range(1, 8, ErrorMessage = "")]
        public AppRoleList UserRole { get; set; }
        public AppStatusList Status { get; set; }
        public DateTime CreateDt { get; set; }
        public string TownId { get; internal set; }
        public string CaptchaCode { get; set; }
        public string RejectReason { get; set; }

        [RegularExpression("^[ABDEF][A-Z][0-9]{6}$")]
        public string ControlNo { get; internal set; }

        public DateTime? ReviewDt { get; internal set; }

        [StringLength(250, ErrorMessage = "案場所有人姓名最多250字元")]
        public string CaseName { get; set; }

        [StringLength(120, ErrorMessage = "案場所有人Email最多120字元")]
        public string CaseEmail { get; set; }
        /// <summary>
        /// 接收使用者上傳用
        /// </summary>
        public Dictionary<eItemType, AttachmentViewModel> Attachment { internal get; set; }

        /// <summary>
        /// 案件是否已鎖定
        /// </summary>
        public bool IsLock
        {
            get
            {
                if (this.Status == AppStatusList.accept) return true;
                if (this.Status == AppStatusList.reject) return true;
                return false;
            }
        }

        public bool IsNotOwner { get; set; }

        /// <summary>
        /// 附件前端呈現使用
        /// </summary>
        public List<object> AttList { get; set; }

        /// <summary>
        /// 取得預設的角色密碼
        /// </summary>
        /// <returns></returns>
        public IList<string> GetDefualtRoles()
        {
            List<string> _Roles = new List<string>();

            if (this.UserRole == AppRoleList.AppCompany || this.UserRole == AppRoleList.AppPersonal) _Roles.Add(RoleList.Company.ToString());

            if (this.UserRole == AppRoleList.AppTreat) _Roles.Add(RoleList.Teart.ToString());

            if (this.UserRole == AppRoleList.AppStore) _Roles.Add(RoleList.Store.ToString());

            return _Roles;
        }

        /// <summary>
        /// 取得帳號開通通知信件類容
        /// </summary>
        /// <param name="newPassWord"></param>
        /// <returns></returns>
        public string GetNofityMailBoy(string newPassWord, string siteName)
        {
            return $@"<div style=""font-family: 'Microsoft JhengHei', sans-serif;color: #555;font-size: 12pt;"">
親愛的 {this.UserName} 先生/小姐 您好：<br/><br/>
您帳號申請已審核通過<br/>
　　帳號是： <b style='color:#F66'>{ControlNo}</b><br/>
　　密碼是： <b style='color:#F66'>{newPassWord}</b><br/>
提醒您，此密碼為系統自動產生，<span style='color:#F66'>建議儘速更新密碼</span>，請牢記並妥善保管帳號及密碼，謹防他人冒用。<br/>
如欲修改個人資料或密碼，請至　密碼變更/個人資料維護 修改！<br/>
{siteName} 敬啟<br/>
【此郵件為系統自動發送，請勿回信!】
            </ div>";
        }

        public string GetNofityMailBoyForApply(string siteName)
        {
            return $@"<div style=""font-family: 'Microsoft JhengHei', sans-serif;color: #555;font-size: 12pt;"">
親愛的 {this.UserName} 先生/小姐 您好：<br/><br/>
已收到您的帳號申請，我們將儘速確認與審核。<br/>
如您有任何建議，歡迎隨時至 {siteName}，感謝您<br/>
【此郵件為系統自動發送，請勿回信!】
            </ div>";
        }

        public string GetRejectMailBoy(string siteName)
        {
            return $@"<div style=""font-family: 'Microsoft JhengHei', sans-serif;color: #555;font-size: 12pt;"">
親愛的 {this.UserName} 先生/小姐 您好：<br/>
您的帳號申請已被駁回。<br/>
駁回原因:<br/>
<pre>{this.RejectReason}</pre>
請您依審查意見重新填寫帳號申請表並提出申請，
如您有任何建議，歡迎隨時至{siteName},感謝您<br/>
【此郵件為系統自動發送，請勿回信!】<br/>
            </ div>";
        }

        public async Task<IEnumerable<FormFileUpload>> GetAttListAsync(DataDbContext _context)
        {
            if (this.Pid <= 0) return new List<FormFileUpload>();
            return await _context.FormFileUpload.Where(x =>
                x.AppId == this.Pid.ToString() &&
                x.DocType == eDocType.AccountAppDoc
            ).ToListAsync();
        }
    }
}
