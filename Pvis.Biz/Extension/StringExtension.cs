using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Pvis.Biz.Utility;

namespace Pvis.Biz.Extension
{
    public static class StringExtension
    {
        /// <summary>
        /// 移除 html 碼中可能的惡意資訊 , For Html 編輯器輸入資料使用
        /// </summary>
        /// <param name="_S"></param>
        /// <returns></returns>
        public static String ToSafehtml(this String _S)
        {
            if (_S == null)
            {
                return null;
            }
            _S = _S.Trim();
            return HAPSanitizer.SanitizeHtml(_S);
        }

        #region 西元
        /// <summary>日期格式字串（yyyy/MM/dd HH:mm:ss）</summary>
        /// <param name="dt">日期</param>
        /// <param name="delimiter">分隔符號（預設為"/"）</param>
        /// <param name="clearDelimiter">清除分隔符號（包含空白）</param>
        /// <returns></returns>
        public static string yyyyMMddHHmmssfff(this DateTime? dt, string delimiter = "/", bool clearDelimiter = false)
        {
            if (!dt.HasValue) return string.Empty;

            return Output(dt.Value, "yyyy/MM/dd HH:mm:ss.fff", delimiter, clearDelimiter);
        }

        /// <summary>日期格式字串（yyyy/MM/dd HH:mm:ss）</summary>
        /// <param name="dt">日期</param>
        /// <param name="delimiter">分隔符號（預設為"/"）</param>
        /// <param name="clearDelimiter">清除分隔符號（包含空白）</param>
        /// <returns></returns>
        public static string yyyyMMddHHmmssfff(this DateTime dt, string delimiter = "/", bool clearDelimiter = false)
        {
            return Output(dt, "yyyy/MM/dd HH:mm:ss.fff", delimiter, clearDelimiter);
        }

        /// <summary>日期格式字串（yyyy/MM/dd HH:mm:ss） </summary>
        /// <param name="dt">日期</param>
        /// <param name="delimiter">分隔符號（預設為"/"）</param>
        /// <param name="clearDelimiter">清除分隔符號（包含空白）</param>
        /// <returns></returns>
        public static string yyyyMMddHHmmss(this DateTime? dt, string delimiter = "/", bool clearDelimiter = false)
        {
            if (!dt.HasValue) return string.Empty;

            return Output(dt.Value, "yyyy/MM/dd HH:mm:ss", delimiter, clearDelimiter);
        }

        /// <summary>日期格式字串（yyyy/MM/dd HH:mm:ss） </summary>
        /// <param name="dt">日期</param>
        /// <param name="delimiter">分隔符號（預設為"/"）</param>
        /// <param name="clearDelimiter">清除分隔符號（包含空白）</param>
        /// <returns></returns>
        public static string yyyyMMddHHmmss(this DateTime dt, string delimiter = "/", bool clearDelimiter = false)
        {
            return Output(dt, "yyyy/MM/dd HH:mm:ss", delimiter, clearDelimiter);
        }

        /// <summary>日期格式字串（yyyy/MM/dd）</summary>
        /// <param name="dt">日期</param>
        /// <param name="delimiter">分隔符號（預設為"/"）</param>
        /// <param name="clearDelimiter">清除分隔符號（包含空白）</param>
        /// <returns></returns>
        public static string yyyyMMdd(this DateTime? dt, string delimiter = "/", bool clearDelimiter = false)
        {
            if (!dt.HasValue) return string.Empty;

            return Output(dt.Value, "yyyy/MM/dd", delimiter, clearDelimiter);
        }

        /// <summary>日期格式字串（yyyy/MM/dd）</summary>
        /// <param name="dt">日期</param>
        /// <param name="delimiter">分隔符號（預設為"/"）</param>
        /// <param name="clearDelimiter">清除分隔符號（包含空白）</param>
        /// <returns></returns>
        public static string yyyyMMdd(this DateTime dt, string delimiter = "/", bool clearDelimiter = false)
        {
            return Output(dt, "yyyy/MM/dd", delimiter, clearDelimiter);
        }

        /// <summary>日期格式字串（yyyy/MM/dd）</summary>
        /// <param name="dt">日期</param>
        /// <param name="format">格式</param>
        /// <returns></returns>
        public static string CustomFormat(this DateTime? dt, string format = "yyyy/MM/dd HH:mm:ss")
        {
            if (!dt.HasValue) return string.Empty;

            return Output(dt.Value, format, "/", false);
        }

        /// <summary>日期格式字串（yyyy/MM/dd）</summary>
        /// <param name="dt">日期</param>
        /// <param name="format">格式</param>
        /// <returns></returns>
        public static string CustomFormat(this DateTime dt, string format = "yyyy/MM/dd HH:mm:ss")
        {
            return Output(dt, format, "/", false);
        }
        #endregion

        #region 民國
        /// <summary>轉換民國年</summary>
        /// <param name="dt">日期</param>
        /// <param name="format">格式（預設為yyyyMMdd）</param>
        /// <returns></returns>
        public static string ToTaiwanDate(this DateTime? dt, string format = null)
        {
            if (!dt.HasValue) return string.Empty;

            return ToTaiwanDate(dt.Value, format);
        }

        /// <summary>轉換民國年</summary>
        /// <param name="dt">日期</param>
        /// <param name="format">格式（預設為yyyyMMdd）</param>
        /// <returns></returns>
        public static string ToTaiwanDate(this DateTime dt, string format = null)
        {
            if (string.IsNullOrEmpty(format))
            {
                format = "yyyyMMdd";
            }

            var tc = new TaiwanCalendar();
            var regex = new Regex(@"[yY]+");

            format = regex.Replace(format, tc.GetYear(dt).ToString("00"));

            return dt.ToString(format);
        }
        #endregion

        /// <summary>指定日期格式字串</summary>
        /// <param name="dt">日期</param>
        /// <param name="format">格式</param>
        /// <param name="delimiter">分隔符號（預設為"/"）</param>
        /// <param name="clearDelimiter">清除分隔符號（包含空白）</param>
        /// <returns></returns>
        private static string Output(DateTime dt, string format, string delimiter, bool clearDelimiter)
        {
            var output = dt.ToString(format);

            if (delimiter != "/")
            {
                output = output.Replace("/", delimiter);
            }

            if (clearDelimiter)
            {
                output = output.Replace(delimiter, "").Replace(":", "").Replace(".", "").Replace(" ", "");
            }

            return output;
        }

        /// <summary>
        /// 字串加密
        /// </summary>
        /// <param name="value"></param>
        /// <param name="EncKey"></param>
        /// <returns></returns>
        public static string Encode(this string value, string EncKey = null )
        {
            return StringCrypt.Encrypt(value, EncKey);
        }

        /// <summary>
        /// 字串解密
        /// </summary>
        /// <param name="value"></param>
        /// <param name="isDynamic"></param>
        /// <returns></returns>
        public static string Decode(this string value, TimeSpan? Exp = null )
        {
            return value.Decode(null, Exp);
        }

        /// <summary>
        /// 字串解密
        /// </summary>
        /// <param name="value"></param>
        /// <param name="isDynamic"></param>
        /// <returns></returns>
        public static string Decode(this string value, string EncKey = null , TimeSpan? Exp = null)
        {
            return StringCrypt.Decrypt(value, EncKey , Exp);
        }


        /// <summary>
        /// 計算 MD5
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToMD5String(this string value) {
            return StringCrypt.ToMD5String(value);
        }

        public static string ToSha256String(this string value, string salted)
        {
            return StringCrypt.ToSHA256String(value, salted);
        }
    }
}
