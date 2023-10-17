using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Models;

namespace Pvis.Biz.Services
{
    public class NewsBusinessLayer
    {
        private DataDbContext _context;

        private static IMapper mapper;
        static NewsBusinessLayer() {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<News, NewsFrontend>();
            });
            mapper = configuration.CreateMapper();
        }

        public NewsBusinessLayer(DataDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// For 前端顯示最新消息資訊取得
        /// </summary>
        /// <param name="id">取得訂定 ID </param>
        /// <param name="TopN">限制回傳筆數</param>
        /// <returns></returns>
        public async Task<IList<NewsFrontend>> GetListForFrontendAsync(int? id = null , int TopN = 250)
        {
            var List = await _context.News
                .Where(x => x.IsEnable
                    && (id == null || x.Pid == id.Value)
                    && x.PostDt <= DateTime.Today
                    && (x.ExpireDt == null || x.ExpireDt > DateTime.Today)
                )
                .OrderByDescending(x => x.PostDt)
                .Select(x=>mapper.Map<NewsFrontend>(x))
                .Take(TopN)
                .ToListAsync();

            if (id.HasValue && id.Value > 0 && List.Count > 0 )
            {
                foreach (var item in List)
                {
                    await item.GetAttListAsync(_context);
                }
            }
            return List;
        }
    }

    /// <summary>
    /// For 前端最新消息資訊呈現
    /// </summary>
    public class NewsFrontend : News
    {
        /// <summary>
        /// 發布日期是否小於兩周
        /// </summary>
        public Boolean IsHot {
            get {
                if (!this.PostDt.HasValue) return false;
                return (DateTime.Now - this.PostDt.Value).TotalDays <= 14;
            }
        }

        /// <summary>
        /// 附件資訊
        /// </summary>
        public List<FormFileUpload> AttList { get; internal set; }

        internal async Task GetAttListAsync(DataDbContext _context)
        {
            this.AttList = await _context.FormFileUpload.Where(x =>
                x.AppId == this.Pid.ToString() &&
                x.DocType == eDocType.NewsDoc &&
                x.ItemType == eItemType.None
            ).ToListAsync();
        }
    }
}
