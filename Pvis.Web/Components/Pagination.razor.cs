using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Pvis.Web.Components
{
    public partial class Pagination
    {
        [Parameter]
        public int PageSize { get; set; }
        [Parameter]
        public int TotalCount { get; set; }
        [Parameter]
        public int Page { get; set; } = 1;
        [Parameter]
        public EventCallback<int> ChangeResult { get; set; }
        //[Parameter]
        //public EventCallback ChangeTotalCount { get; set; }
        public int TotalPage { get; set; }
        protected override void OnInitialized()
        {
            TotalPage = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(TotalCount) / Convert.ToDecimal(PageSize)));
        }
        protected override void OnParametersSet()
        {
            TotalPage = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(TotalCount) / Convert.ToDecimal(PageSize)));
        }
        //public async Task ChangeCount (int count)
        //{
        //    TotalPage = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(count) / Convert.ToDecimal(PageSize)));
        //    await ChangeTotalCount.InvokeAsync(count);
        //}
        public async Task Change(int page)
        {
            Page = page;
            await ChangeResult.InvokeAsync(Page);
        }
        public async Task Next()
        {
            if (Page < TotalPage)
                Page++;
            else
                Page = 1;
            await ChangeResult.InvokeAsync(Page);
        }
        public async Task Pre()
        {
            if (Page > 1)
                Page--;
            else
                Page = TotalPage;
            await ChangeResult.InvokeAsync(Page);
        }
        public async Task PreFive()
        {
            if (Page > 1)
            {
                Page-= 5;
                if (Page < 1)
                    Page = 1;
            }               
            else
            {
                Page = TotalPage;
            }               
            await ChangeResult.InvokeAsync(Page);
        }
        public async Task NextFive()
        {
            if (Page < TotalPage)
            {
                Page += 5;
                if (Page > TotalPage)
                     Page = TotalPage;
            }                
            else
            {
                Page = 1;
            }
                
            await ChangeResult.InvokeAsync(Page);
        }
    }
}
