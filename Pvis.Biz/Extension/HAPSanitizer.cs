using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Web;

namespace Pvis.Biz.Extension
{
    /*
    參考資料:
        http://www.west-wind.com/weblog/posts/2012/Jul/19/NET-HTML-Sanitation-for-rich-HTML-Input
        http://www.blueshop.com.tw/board/show.asp?subcde=BRD20120307190858R4J
    */
    /// <summary>
    /// 處理 Html 輸出 XSS 問題.
    /// </summary>
    internal class HAPSanitizer
    {

        /// <summary>
        /// Cleans up an HTML string and removes HTML tags in blacklist
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string SanitizeHtml(string html)
        {
            HAPSanitizer sanitizer = new HAPSanitizer();
            return sanitizer.Sanitize(html);
        }

        #region <<-- 私有方法屬性.-->>

        #region <-- BlackList 排除的 Html Tage 清單. -->
        /// <summary>
        /// 排除的 Html Tage 清單.
        /// </summary>
        private static List<string> BlackList
        {
            get
            {
                if (_BlackList.Count <= 0)
                {
                    _BlackList.Add("script");
                    //_BlackList.Add("iframe");
                    _BlackList.Add("form");
                    _BlackList.Add("object");
                    _BlackList.Add("embed");
                    _BlackList.Add("link");
                    _BlackList.Add("head");
                    _BlackList.Add("meta");
                    _BlackList.Add("style");
                }
                return _BlackList;
            }
        }
        private static List<string> _BlackList = new List<string>();
        #endregion

        /*
         XSS 特殊資料樣板:
        1. 使用 date: base64 編碼資料.
        <a href="data:text/html;base64,PHNjcmlwdD5hbGVydCgnWFNTJyk8L3NjcmlwdD4K">test</a>
        2. 使用特殊字完 &colon;
        <A HreF= javascript&colon;prompt(912560)>test</a>
        3. 使用 number chart &#x3A; &#x52;.
        <A HreF= javascript&#x3A;prompt(982782)>test</a>
        <A HreF= j&#x61;v&#x41;sc&#x52;ipt&#x3A;prompt(955309)>test</a>
         */

        #region <-- Event Handlers 過濾條件. -->
        /// <summary>
        /// Event Handlers 過濾條件.
        /// </summary>
        public static readonly Regex ReEventHandler = new Regex("(^on|^(FSCommand|seekSegmentTime)$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        #endregion

        /// <summary>
        /// javscript vbscript 排除樣式.
        /// &colon; &#x3A; 結果等同於 :
        /// </summary>
        private static readonly Regex ReExcludePattern = new Regex("(javascript(:|&colon;)|vbscript(:|&colon;))|data(:|&colon;)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// CSS Expressions 排除比對樣式.
        /// </summary>
        private static readonly Regex ReCssExpPattern = new Regex("(expression|javascript(:|&colon;)|vbscript(:|&colon;))|data(:|&colon;)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// 節點 TagName 限制只能是英數,且不超過 20 個字
        /// </summary>
        private static readonly Regex ReNodeNameRorle = new Regex("^[A-Z0-9]{1,20}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// 節點 Attr 限制只能是英數,且不超過 20 個字
        /// </summary>
        private static readonly Regex ReAttrNameRorle = new Regex("^[A-Z0-9]{1,20}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// 比對任一個空白字元（White space character），等效於 [ \f\n\r\t\v]
        /// </summary>
        private static readonly Regex ReWhiteSpaceCharacter = new Regex(@"(\s|&NewLine;)", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Cleans up an HTML string by removing elements
        /// on the blacklist and all elements that start
        /// with onXXX .
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private string Sanitize(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            SanitizeHtmlNode(doc.DocumentNode);
            string output = doc.DocumentNode.InnerHtml;
            doc = null;
            return output;
        }

        #region <-- SanitizeHtmlNode 節點元素處理 -->
        /// <summary>
        /// SanitizeHtmlNode 節點元素處理
        /// </summary>
        /// <param name="node"></param>
        private void SanitizeHtmlNode(HtmlNode node)
        {

            bool _IsElement = (node.NodeType == HtmlNodeType.Element);

            //如果是註解型態的節點也移除掉以避免下列型態的攻擊模式
            //	<!--[if gte IE 4]>
            //		<SCRIPT>alert('XSS');</SCRIPT>
            //	<![endif]-->
            if (node.NodeType == HtmlNodeType.Comment)
            {
                node.Remove();
                return;
            }

            // 如果 Element 在 排除清單內，或 TagName 不是正常英數組合，移除掉該節點.
            if (_IsElement && (BlackList.Contains(node.Name) || !ReNodeNameRorle.IsMatch(node.Name)))
            {
                node.Remove();
                return;
            }

            // remove CSS Expressions and embedded script links
            if (_IsElement && node.Name == "style" && ReExcludePattern.IsMatch(node.InnerText))
            {
                node.ParentNode.RemoveChild(node);
            }

            // remove script attributes
            if (_IsElement && node.HasAttributes)
            {
                for (int i = node.Attributes.Count - 1; i >= 0; i--)
                {
                    HtmlAttribute currentAttribute = node.Attributes[i];
                    if (IsDangerAttributes(currentAttribute))
                    {
                        node.Attributes.Remove(currentAttribute);
                    }
                }
            }

            // 遞迴處理所有子元素.
            if (node.HasChildNodes)
            {
                for (int i = node.ChildNodes.Count - 1; i >= 0; i--)
                {
                    SanitizeHtmlNode(node.ChildNodes[i]);
                }
            }
        }
        #endregion

        #region <-- IsDangerAttributes 判定是否有危害的屬性標籤. -->
        /// <summary>
        /// IsDangerAttributes 判定是否有危害的屬性標籤.
        /// </summary>
        /// <param name="_Attribute"></param>
        /// <returns></returns>
        private static bool IsDangerAttributes(HtmlAttribute _Attribute)
        {

            string attr = _Attribute.Name.ToLower();
            string val = _Attribute.Value.ToLower();

            // remove event handlers ( 例如 onerror onclick ...這類 javascript 事件.)
            if (ReEventHandler.IsMatch(attr))
            {
                return true;
            }

            if (!ReAttrNameRorle.IsMatch(attr))
            {
                return true;
            }

            //先做 HtmlDecode 密免透過 , &colon;#x61;v&#x41;sc&#x52; 這類的編碼機制 
            //同時取代掉 比對任一個空白字元（White space character
            val = ReWhiteSpaceCharacter.Replace(HttpUtility.HtmlDecode(val), "");
            // remove script links
            // (attr == "href" || attr== "src" || attr == "dynsrc" || attr == "lowsrc") &&
            if (ReExcludePattern.IsMatch(val))
            {
                return true;
            }

            // Remove CSS Expressions
            if (attr == "style" && ReCssExpPattern.IsMatch(val))
            {
                return true;
            }

            return false;
        }
        #endregion

        #endregion
    }
}
