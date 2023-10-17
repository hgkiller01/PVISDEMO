using System.Collections.Generic;

namespace Pvis.Web.ViewModel
{
    /// <summary>問答</summary>
    public class QA
    {
        /// <summary>問</summary>
        public string Q { get; set; }
        /// <summary>答</summary>
        public string A { get; set; }
    }

    public class QnAVM
    {
        public List<QA> C0 { get; set; }
        public List<QA> C1 { get; set; }
        public List<QA> C2 { get; set; }
    }
}
