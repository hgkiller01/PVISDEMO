using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Pvis.Biz.CustomizedAttr
{
    /// <summary>
    /// 設定欄位只新增不修改處理
    /// </summary>
    /// <example>
    ///    使用說明:
    ///       建立 Models 對於不需要更新的欄位加上 例如.
    ///     <code>
    ///           [NeverUpdate]
    ///           public DateTime CreateDt { get; set; }
    ///     </code>
    ///       建立 DbContext 時 , 註冊 StateChanged 事件. 例如
    ///     <code>
    ///           public partial class DataDbContext : DbContext {
    ///               public DataDbContext(DbContextOptions<DataDbContext> options): base(options)
    ///               {
    ///                   ChangeTracker.StateChanged += NeverUpdateAttribute.ChangeTracker_StateChanged;
    ///               }
    ///           }
    ///     </code>
    /// </example>
    public class NeverUpdateAttribute : Attribute
    {
        /// <summary>
        /// 處理變更時 , 依據 Models Attribute 停用特定欄位更新動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void ChangeTracker_StateChanged(object sender, EntityStateChangedEventArgs e)
        {
            if (e.NewState != EntityState.Modified) return;
            foreach (var property in e.Entry.Entity.GetType().GetProperties())
            {
                var attributes = property.GetCustomAttributes(typeof(NeverUpdateAttribute), false);
                if (attributes.Any())
                {
                    e.Entry.Property(property.Name).IsModified = false;
                }
            }

            return;
        }
    }
}
