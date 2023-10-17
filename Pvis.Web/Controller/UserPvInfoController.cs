using System;
using System.Collections.Generic;
using System.Linq;
using LinqKit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Extension;
using Pvis.Biz.Models;
using Pvis.Biz.Member;
using System.IO;
using Pvis.Biz.Services;
using AutoMapper;
using Pvis.Web.Helper;
using Pvis.Biz.ViewModels;
using ClosedXML.Excel;

namespace Pvis.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [PvisAuthorize(RoleList.Admin, RoleList.Company)]
    public class UserPvInfoController : ControllerBase
    {
        private readonly DataDbContext _context;

        public UserPvInfoController(DataDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("GetList")]
        public async Task<ActionResult<IEnumerable<UserPvInfo>>> GetList(SPQry Qry)
        {
            var pred = PredicateBuilder.New<UserPvInfo>(true);
            if (User.HasRole(RoleList.Company))
                pred = pred.And(x => x.Uid == User.GetUid());
            if (!String.IsNullOrWhiteSpace(Qry.KeyWord))
            {
                Qry.KeyWord = Qry.KeyWord.Trim();
                pred = pred.And(x => x.Pvno.Contains(Qry.KeyWord));
            }
            if (!string.IsNullOrEmpty(Qry.Status))
            {
                pred = pred.And(x => x.Status == Qry.Status);
            }
            return await _context.UserPvInfo.Where(pred).ToListAsync();
        }

        [HttpPost]
        [Route("Load")]
        public async Task<ActionResult<IEnumerable<UserPvInfo>>> Load()
        {
            if (User.HasRole(RoleList.Company))
                return await _context.UserPvInfo.Where(x => x.Uid == User.GetUid()).ToListAsync();
            else
                return await _context.UserPvInfo.ToListAsync();
        }

        [HttpPost]
        [Route("GetItem")]
        public async Task<ActionResult<UserPvInfo>> GetItemAsync(SPQry PV)
        {
            var _Rec = _context.UserPvInfo.Where(x => x.Pid == PV.Pid).FirstOrDefault();
            if (User.HasRole(RoleList.Company)) _Rec = _context.UserPvInfo.Where(x => x.Pid == PV.Pid && x.Uid == User.GetUid()).FirstOrDefault();
            List<FormFileUpload> Att = null;
            if (_Rec != null)
            {
                Att = await _context.FormFileUpload.Where(x =>
                    x.AppId == _Rec.Pid.ToString() &&
                    x.DocType == eDocType.UserPvInfoDoc &&
                    (x.ItemType == eItemType.ApplyDoc || x.ItemType == eItemType.ProvDoc || x.ItemType == eItemType.AccountApp_LetterOfAgreement
                    || x.ItemType == eItemType.PvSnDoc)
                ).ToListAsync();
            }

            return Ok(new
            {
                IsSuccess = (_Rec != null),
                Rec = _Rec,
                AttList = Att.Select(x => new
                {
                    url = Url.Content(x.FilePath),
                    x.OriginalFileName,
                    x.ItemType
                })
            });
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IActionResult> Save(ViewPVmodel UPVA)
        {
            UserPvInfo userpvinfo = UPVA.Rec;
            var errors = new List<string>();
            var _EntityState = (userpvinfo.Pid <= 0) ? EntityState.Added : EntityState.Modified;
            var Tows = await _context.Town.Where(x =>
            userpvinfo.Pvaddr.StartsWith(x.CountyName) &&
            userpvinfo.Pvaddr.StartsWith(x.CountyName + x.TownName))
           .Take(2).ToListAsync();
            if (userpvinfo.Startdate.Year < 1950) errors.Add("輸入日期不正確");
            if (Tows.Count() != 1) errors.Add("輸入地址不正確");
            if (errors.Count > 0) { return BadRequest(new { IsSuccess = false, errors }); }
            if (_EntityState == EntityState.Added)
            {
                if (UserPvInfoExists(userpvinfo.Pvno)) errors.Add("此設備登記編號已設定過");
                if (errors.Count > 0) { return BadRequest(new { IsSuccess = false, errors }); }
                userpvinfo.Createdate = DateTime.Now;
            }

            userpvinfo.Uid = User.GetUid();
            //userpvinfo.AddrType = "1";
            //userpvinfo.Status = "1";



            using (var transaction = _context.Database.BeginTransaction())
            {
                _context.Entry(userpvinfo).State = _EntityState;
                await _context.SaveChangesAsync();


                foreach (var item in UPVA.Attachment)
                {
                    if (_EntityState != EntityState.Added) { await DeleteAttached(new SPQry { Pid = UPVA.Rec.Pid,eItemType = item.Key }); }
                    var F = FormFileUploadBusinessLayer.FillFileData(item.Value, new FormFileUpload() { AppId = userpvinfo.Pid.ToString(), DocType = eDocType.UserPvInfoDoc, ItemType = item.Key }, User.GetUid());
                    if (FileCheck.IsAllowedExtension(item.Value, F,_context, "pdf", "設備登記資料維護", Request.HttpContext.Connection.RemoteIpAddress.ToString()))
                    {
                        try
                        {
                            FormFileUploadBusinessLayer.SetFilePath(F);
                            Directory.CreateDirectory(Path.GetDirectoryName(FormFileUploadBusinessLayer.GetSavePath(F)));
                            System.IO.File.WriteAllBytes(FormFileUploadBusinessLayer.GetSavePath(F), item.Value.GetContent());
                            _context.Add(F).State = EntityState.Added;
                        }
                        catch (Exception ex)
                        {
                            FileCheck.WriteErrorFile(F,_context ,ex.Message, "設備登記資料維護", Request.HttpContext.Connection.RemoteIpAddress.ToString());
                        }
                        await _context.SaveChangesAsync();
                    }

                }

                await _context.SaveChangesAsync();
                transaction.Commit();

            }

            return Ok(new
            {
                Rec = userpvinfo,
                IsAdded = (_EntityState == EntityState.Added)
            });
        }

        [HttpPost]
        [Route("DeleteAttached")]
        public async Task<IActionResult> DeleteAttached(SPQry item)
        {
            List<FormFileUpload> FS = new List<FormFileUpload>();
            if (item.eItemType != eItemType.None)
            {
                FS = _context.FormFileUpload.Where(x =>
                    x.AppId == item.Pid.ToString() &&
                    x.DocType == eDocType.UserPvInfoDoc && x.ItemType == item.eItemType
                ).ToList();
            }
            else
            {
                FS = _context.FormFileUpload.Where(x =>
                    x.AppId == item.Pid.ToString() &&
                    x.DocType == eDocType.UserPvInfoDoc).ToList();
            }


            if (FS.Count() == 0)
            {
                return Ok();
            }

            foreach (FormFileUpload F in FS)
            {
                var _SavePath = FormFileUploadBusinessLayer.GetSavePath(F);
                if (System.IO.File.Exists(_SavePath))
                {
                    System.IO.File.Delete(_SavePath);
                }
                _context.Entry(F).State = EntityState.Deleted;
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> Delete(UserPvInfo userpvinfo)
        {
            var errors = new List<string>();
            var _userpvinfo = await _context.UserPvInfo.FindAsync(userpvinfo.Pid);
            if (_userpvinfo == null)
            {
                return NotFound();
            }
            //移除附件
            SPQry _SQ = new SPQry { Pid = userpvinfo.Pid,eItemType = eItemType.None };
            await DeleteAttached(_SQ);

            if (PvUsed(userpvinfo.Pid)) errors.Add("此設備登記編號已被使用，無法刪除!");
            if (errors.Count > 0) { return BadRequest(new { IsSuccess = false, errors }); }

            _context.UserPvInfo.Remove(_userpvinfo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet]
        [Route("DownLoadExcel")]
        public IActionResult DownLoadExcel([FromQuery]SPQry PV)
        {
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = "設備登記資料.xlsx";
            var workbook = new XLWorkbook();
            IXLWorksheet worksheet = workbook.Worksheets.Add("設備登記資料維護");
            var result = GetList(PV).Result.Value.ToList();
            if (result != null)
            {
                worksheet.Cell(1, 1).Value = "設備登記編號";
                worksheet.Cell(1, 2).Value = "型態";
                worksheet.Cell(1, 3).Value = "設置地址";
                worksheet.Cell(1, 4).Value = "併網日期";
                worksheet.Cell(1, 5).Value = "單一設備裝置容量(瓩)";
                worksheet.Cell(1, 6).Value = "狀態";
                worksheet.Cell(1, 7).Value = "總裝置容量(瓩)";
                worksheet.Cell(1, 8).Value = "設備數量(片)";
                worksheet.Cell(1, 9).Value = "備案編號";
                worksheet.Cell(1, 10).Value = "再生能源發電設備型別及使用能源編號";
                for (int index = 1; index <= result.Count(); index++)
                {
                    worksheet.Cell(index + 1, 1).Value = result[index - 1].Pvno;
                    worksheet.Cell(index + 1, 2).Value = ChangeAddrType(result[index - 1].AddrType);
                    worksheet.Cell(index + 1, 3).Value = result[index - 1].Pvaddr;
                    worksheet.Cell(index + 1, 4).Value = result[index - 1].Startdate.ToShortDateString();
                    worksheet.Cell(index + 1, 5).Value = result[index - 1].Kilowatt;
                    worksheet.Cell(index + 1, 6).Value = ChangeStatus(result[index - 1].Status);
                    worksheet.Cell(index + 1, 7).Value = result[index - 1].Allkilowatt;
                    worksheet.Cell(index + 1, 8).Value = result[index - 1].SpQty;
                    worksheet.Cell(index + 1, 9).Value = result[index - 1].Bno;
                    worksheet.Cell(index + 1, 10).Value = ChangePeType(result[index - 1].PETypeID);
                }

            }
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                return File(content, contentType, fileName);
            }
        }
        private string ChangePeType(byte PEtype)
        {
            if(PEtype == 1)
            {
                return "第一型再生能源發電設備－太陽光電發電設備";
            }
            else if(PEtype == 2)
            {
                return "第二型再生能源發電設備－太陽光電發電設備";
            }
            else
            {
                return "第三型再生能源發電設備－太陽光電發電設備";
            }
        }
        private string ChangeStatus(string Status)
        {
            if(Status == "1")
            {
                return "使用中";
            }
            else
            {
                return "停用中";
            }
        }
        private string ChangeAddrType(string addrType)
        {
            if(addrType == "1")
            {
                return "地址";
            }
            else
            {
                return "地號";
            }
        }
        private bool UserPvInfoExists(int id)
        {
            return _context.UserPvInfo.Any(e => e.Pid == id);
        }

        private bool UserPvInfoExists(string _pvno)
        {
            return _context.UserPvInfo.Any(e => e.Pvno.Equals(_pvno));
        }

        private bool PvUsed(int _pid)
        {
            return _context.UserSpInfo.Any(e => e.Pvid == _pid);
        }

        [HttpPost]
        [Route("GetUserPvidOptions")]
        public async Task<ActionResult<List<UserPvInfo>>> GetUserPvidOptions()
        {
            return (await _context.UserPvInfo.Where(y => y.Uid == User.GetUid() && y.Status == "1").Select(x => new UserPvInfo() { Pid = x.Pid, Pvno = x.Pvno }).ToListAsync());
        }

        public class ViewPVmodel
        {
            public UserPvInfo Rec { get; set; }
            public Dictionary<eItemType, Pvis.Biz.ViewModels.AttachmentViewModel> Attachment { internal get; set; }
        }
       
    }

}
