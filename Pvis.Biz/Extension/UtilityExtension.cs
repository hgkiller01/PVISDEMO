using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Text;
using System.Security.Cryptography;
using System.Data;
using System.Text.RegularExpressions;


namespace ERI.Utility.Extensions
{
    /// <summary>字串處理擴充功能</summary>
    public static class StringExtension
    {
        /// <summary>字串轉Int(轉換失敗則回傳0)</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int TryParseInt(this string value)
        {
            return ((object)value).TryParseInt(); 
        }

        /// <summary>字串轉Int(轉換失敗則回傳null)</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int? TryParseIntNullable(this string value)
        {
            int v = 0;
            if (int.TryParse(value, out v)) return v;
            return null;            
        }

        /// <summary>字串轉double(轉換失敗則回傳0)</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double TryParseDouble(this string value)
        {
            return  ((object)value).TryParseDouble(); 
        }

        /// <summary>字串轉decimal(轉換失敗則回傳null)</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal? TryParseDecimalNullable(this string value)
        {
            decimal v = 0;
            if (decimal.TryParse(value, out v)) return v;
            return null;
        }

        /// <summary>字串轉bool(轉換失敗則回傳false)</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryParseBoolean(this string value)
        {
            return ((object)value).TryParseBoolean(); 
        }

        /// <summary>民國年字串轉DateTime</summary>
        /// <param name="value">字串值</param>
        /// <param name="fromat">日期格式</param>
        /// <returns></returns>
        public static DateTime TryParseRepublicToAD(this string value, string fromat = "yyyy/MM/dd")
        {
            if (string.IsNullOrEmpty(value)) return DateTime.MinValue;
            System.Globalization.CultureInfo m_ciTaiwan = new System.Globalization.CultureInfo("zh-TW");
            m_ciTaiwan.DateTimeFormat.Calendar = new System.Globalization.TaiwanCalendar();            
            DateTime dtvalue = DateTime.ParseExact(value.PadLeft(fromat.Length, '0'), fromat, m_ciTaiwan);
            return dtvalue;
        }

        /// <summary>字串加密</summary>
        /// <param name="value">字串值</param>
        /// <param name="isDynamic">是否動態Key值</param>
        /// <returns></returns>
        public static string Encode(this string value, bool isDynamic = true)
        {
            throw new Exception("尚未實做");
        }

        /// <summary>字串解密</summary>
        /// <param name="value">字串值</param>
        /// /// <param name="isDynamic">是否動態Key值</param>
        /// <returns></returns>
        public static string Decode(this string value, bool isDynamic = true)
        {
            throw new Exception("尚未實做");
        }

        /// <summary>字串雜湊</summary>
        /// <param name="value">字串值</param>
        /// <param name="salted">變異值</param>
        /// <param name="HashMethod">演算法</param>
        /// <returns></returns>
        public static string HashString(this string value, string salted = "")
        {
            throw new Exception("尚未實做");
        }

        /// <summary>字串判定是否有全形文字或數字</summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool FullWidthWord(this String values)
        {
            bool result = false;
            string pattern = @"^[\u4E00-\u9fa5]+$";
            foreach (char item in values)
            {
                //以Regex判斷是否為中文字，中文字視為全形
                if (!Regex.IsMatch(item.ToString(), pattern))
                {
                    //以16進位值長度判斷是否為全形字
                    if (string.Format("{0:X}", Convert.ToInt32(item)).Length != 2)
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>數字轉國數字字典集</summary>
        private static Dictionary<string, string> _DicNumberToChNum = null;        
        /// <summary>數字轉國數字字典集</summary>
        public static Dictionary<string, string> DicNumberToChNum{
            get{
                if (_DicNumberToChNum != null) return _DicNumberToChNum;
                _DicNumberToChNum = new Dictionary<string, string>();
                _DicNumberToChNum["1"]="一";
                _DicNumberToChNum["2"]="二";
                _DicNumberToChNum["3"]="三";
                _DicNumberToChNum["4"]="四";
                _DicNumberToChNum["5"]="五";
                _DicNumberToChNum["6"]="六";
                _DicNumberToChNum["7"]="七";
                _DicNumberToChNum["8"]="八";
                _DicNumberToChNum["9"]="九";
                _DicNumberToChNum["0"]="○";
                _DicNumberToChNum["１"]="一";
                _DicNumberToChNum["２"]="二";
                _DicNumberToChNum["３"]="三";
                _DicNumberToChNum["４"]="四";
                _DicNumberToChNum["５"]="五";
                _DicNumberToChNum["６"]="六";
                _DicNumberToChNum["７"]="七";
                _DicNumberToChNum["８"]="八";
                _DicNumberToChNum["９"]="九";
                _DicNumberToChNum["０"]="○";
                return _DicNumberToChNum;
            }
        }
        /// <summary>數字轉國數字</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string TryParseNumberToChNum(this string value)
        {
            foreach (KeyValuePair<string, string> kvp in DicNumberToChNum)
            {
                value = value.Replace(kvp.Key, kvp.Value);            
            }
            return value;
        }

        /// <summary>字串是否包含特殊字元僅中英文數字下底線</summary>
        /// <param name="value">字串值</param>
        /// <returns></returns>
        public static bool XssCheck(this string value)
        {
            throw new Exception("尚未實做");
        }

        /// <summary>轉換網頁用折行</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToHTMLEnter(this string value)
        {
            return value.TryParseString().Replace("\r", "<br/>");        
        }

        /// <summary>產生虛擬路徑</summary>
        /// <param name="_S"></param>
        /// <returns></returns>
        public static String ToUrl(this String _S)
        {
            throw new Exception("尚未實做");
        }
    }

    /// <summary>位元組擴充</summary>
    public static class ByteExtension
    {
        /// <summary>轉16進制文字</summary>
        /// <param name="data">資料源</param>
        /// <returns></returns>
        public static string ToHexString(this byte[] data)
        {
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString().ToUpper();
        }

    }

    /// <summary>日期處理擴充功能</summary>
    public static class DateTimeExtension 
    {
        /// <summary>轉民國年</summary>
        /// <param name="value"></param>
        /// <param name="fromat">格式(不含年)</param>
        /// <returns></returns>
        public static string ToChYMD(this DateTime value, string fromat = "/MM/dd")
        {
            if (value <= new DateTime(1912,1,1)) return "";
            System.Globalization.TaiwanCalendar twC = new System.Globalization.TaiwanCalendar();
            return twC.GetYear(value) + value.ToString(fromat);
        }

        /// <summary>轉字串(規避DateTime.MinValue:回傳空字串)</summary>
        /// <param name="value"></param>
        /// <param name="fromat">格式</param>
        /// <returns></returns>
        public static string TryToString(this DateTime value, string fromat = "yyyy/MM/dd")
        {
            return TryToString(value,fromat);
        }

        /// <summary>轉字串(規避DateTime.MinValue與null:回傳空字串)</summary>
        /// <param name="value"></param>
        /// <param name="fromat">格式</param>
        /// <returns></returns>
        public static string TryToString(this DateTime? value, string fromat = "yyyy/MM/dd")
        {
            if (value == null) return "";
            if (value.Value == DateTime.MinValue) return "";
            return value.Value.ToString(fromat);
        }

    }
    
    /// <summary>物件處理擴充功能</summary>
    public static class ObjectExtension
    {
        /// <summary>物件轉字串(轉換失敗則回傳空字串)</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string TryParseString(this object value)
        {
            return string.Format("{0}", value);
        }

        /// <summary>物件轉Int(轉換失敗則回傳0)</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int TryParseInt(this object value)
        {
            return Trans.TryParse<int>(value);
        }

        /// <summary>物件轉double(轉換失敗則回傳0)</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double TryParseDouble(this object value)
        {
            return Trans.TryParse<double>(value);
        }

        /// <summary>物件轉decimal(轉換失敗則回傳0)</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal TryParseDecimal(this object value)
        {
            return Trans.TryParse<decimal>(value);
        }

        /// <summary>物件轉bool(轉換失敗則回傳false)</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryParseBoolean(this object value)
        {
            int i = 0;
            bool b=false;
            if (int.TryParse(value.TryParseString(), out i))
                b = Convert.ToBoolean(i);
            else
                bool.TryParse(value.TryParseString(), out b);
            return b;
        }

        /// <summary>物件轉DateTime(轉換失敗則回傳DateTime.MinValue)</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime TryParseDateTime(this object value)
        {
            return Trans.TryParse<DateTime>(value);
        }

        /// <summary>物件轉DateTime(轉換失敗則回傳null)</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime? TryParseDateTimeNullable(this object value)
        {
            DateTime v = DateTime.MinValue;
            if (DateTime.TryParse(value.ToString(), out v)) return v;
            return null;
        }


        /// <summary>物件轉自訂日期格式文字</summary>
        /// <param name="value"></param>
        /// <param name="fromat"></param>
        /// <returns></returns>
        public static string ToFormatDateTime(this object value, string fromat = "yyyy/MM/dd")
        {
            if (value.TryParseDateTime() <= new DateTime(1900, 1, 1)) return "";
            return string.Format("{0:" + fromat + "}", value);
        }
        
    }

    /// <summary>Nullable型別擴充</summary>
    public static class NullableExtension
    {
        public static bool TryParseValue(this bool? value)
        {
            return value.HasValue ? value.Value : false;
        
        }
    }

    /// <summary>布林擴充</summary>
    public static class BoolenExtension
    {

        /// <summary>Bool轉Int</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ToInt(this bool value)
        {
            return Convert.ToInt32(value);
        }

       
    }

    /// <summary>DataTable擴充</summary>
    public static class DataTableExtension 
    {
        /// <summary>DataTable轉指定型態List</summary>
        /// <typeparam name="T">指定型態</typeparam>
        /// <param name="table">資料來源</param>
        /// <returns></returns>
        public static IList<T> ToList<T>(this DataTable table) where T : new()
        {
            IList<T> result = new List<T>();

            foreach (DataRow dr in table.Rows)
            {
                T obj = new T();
                foreach (DataColumn dc in table.Columns)
                {
                    PropertyInfo Pinfo = obj.GetType().GetProperty(dc.ColumnName);
                    if (Pinfo != null)
                    {
                        object objData = dr[dc.ColumnName].TryParseString();
                        Pinfo.SetValue(obj, objData, null);
                    }
                }
                result.Add(obj);
            }

            return result;
        }
        /// <summary>
        /// SetOrdinal of DataTable columns based on the index of the columnNames array. Removes invalid column names first.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columnNames"></param>
        /// <remarks> http://stackoverflow.com/questions/3757997/how-to-change-datatable-colums-order</remarks>
        public static void SetColumnsOrder(this DataTable dtbl, params String[] columnNames)
        {
            List<string> listColNames = columnNames.ToList();

            //Remove invalid column names.
            foreach (string colName in columnNames)
            {
                if (!dtbl.Columns.Contains(colName))
                {
                    listColNames.Remove(colName);
                }
            }

            foreach (string colName in listColNames)
            {
                dtbl.Columns[colName].SetOrdinal(listColNames.IndexOf(colName));
            }
        }

        /// <summary>
        /// Remove of DataTable columns
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columnNames"></param>
        /// <remarks> http://stackoverflow.com/questions/3757997/how-to-change-datatable-colums-order</remarks>
        public static void RemoveColumns(this DataTable dtbl, params String[] columnNames)
        {
            List<string> listColNames = columnNames.ToList();

            //Remove invalid column names.
            foreach (string colName in columnNames)
            {
                dtbl.Columns.Remove(colName); 
            }
        }

        /// <summary>
        /// Set columns of DataTable 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columnNames"></param>
        public static void SetColumnsName(this DataTable dtbl, params String[] columnNames)
        {
            List<string> listColNames = columnNames.ToList();

            foreach (string colName in columnNames)
            {
                if (colName.IndexOf(":") > 0) {
                    string[] cols = colName.Split(':');
                    dtbl.Columns[cols[0]].ColumnName = cols[1];
                }  
                else continue; 
            }
        }

    }
    

    /// <summary>序列成員結構</summary>
    [Serializable]
    public class ListDataItem
    {
        public ListDataItem() 
        {
        
        }
        /// <summary>建構式</summary>
        /// <param name="_ID">名稱</param>
        /// <param name="_Text">描述</param>
        public ListDataItem(string _ID,string _Text)
        {
            ID = _ID;
            Text = _Text;
        }

        /// <summary>建構式</summary>
        /// <param name="_ID">名稱</param>
        /// <param name="_Text">描述</param>
        /// <param name="_Value">值</param>
        public ListDataItem(string _ID, string _Text,int _Value)
        {
            ID = _ID;
            Text = _Text;
            Value = _Value;
        }

        /// <summary>描述</summary>
        public string Text { get; set; }
        /// <summary>名稱</summary>
        public string ID { get; set; }
        /// <summary>值</summary>
        public int Value { get; set; }
        /// <summary>排序值</summary>
        public int Sort { get; set; }    
    }

    /// <summary>列舉處理擴充功能</summary>
    public static class EnumExtension
    {
        /// <summary>列舉轉Int</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ToInt32(this Enum value)
        {
            return Convert.ToInt32(value);
        }

        /// <summary>列舉轉Int</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int TryParseInt(this Enum value)
        {
            return ToInt32(value);
        }

        /// <summary>取得列舉值之自訂描述屬性</summary>
        /// <param name="EnumType">列舉擴充</param>
        /// <param name="Separator">分隔文字</param>
        /// <returns>自訂描述文字</returns>
        public static string GetText(this Enum EnumType, string Separator = "")
        {  
            List<string> val =  new List<string>();
            Int64 etvalue = Convert.ToInt64(EnumType);
            bool IsFlag = EnumType.GetType().GetCustomAttributes(typeof(FlagsAttribute), false).Any();
            foreach (Enum e in Enum.GetValues(EnumType.GetType()))
            {
                Int64 ev =Convert.ToInt64(e);
                if (IsFlag)
                {
                    if (ev == 0) continue;
                    if ((etvalue & ev) == ev) val.Add(GetEnumCustomAttribute(e).Description);
                }
                else if(etvalue == ev)
                {
                    val.Add(GetEnumCustomAttribute(e).Description);
                }                
            }
            return string.Join(Separator, val.ToArray());
        }

        
        /// <summary>取出列舉自訂標籤</summary>
        /// <param name="EnumType">列舉</param>
        /// <returns></returns>
        private static CustomAttribute GetEnumCustomAttribute(Enum EnumType) 
        {
            var EnumTypeField = EnumType.GetType().GetField(EnumType.ToString());            
            var attributes = EnumTypeField.GetCustomAttributes(typeof(CustomAttribute), false);
            var CustomAttr = ((CustomAttribute)attributes[0]);
            return CustomAttr;        
        }


        /// <summary>取得列舉值之自訂取得排序值</summary>
        /// <param name="EnumType">列舉擴充</param>
        /// <returns>取得排序值</returns>
        public static int GetSort(this Enum EnumType)
        {
            return GetEnumCustomAttribute(EnumType).Sort;
        }

        /// <summary>取得列舉序列</summary>
        /// <typeparam name="T">列舉型別</typeparam>
        /// <returns></returns>
        public static List<ListDataItem> GetEnumList<T>()
        {
            return GetEnumList<T>(null);
        }

        /// <summary>取得列舉序列</summary>
        /// <param name="RemoveItem">移除列舉項目</param>
        /// <typeparam name="T">列舉型別</typeparam>
        /// <returns></returns>
        public static List<ListDataItem> GetEnumList<T>(Enum RemoveItem)
        {
            List<ListDataItem> list = new List<ListDataItem>();
            
            if (typeof(T).IsEnum)
            {
                bool IsFlag = typeof(T).GetCustomAttributes(typeof(FlagsAttribute), false).Any();
                int RemoveValue = RemoveItem.ToInt32();
                foreach (Enum en in Enum.GetValues(typeof(T)))
                {
                    if (RemoveItem != null)
                    {
                        if (en.Equals(RemoveItem)) continue;
                        if (IsFlag && RemoveValue > 0 && en.HasFlag(RemoveItem)) continue;                        
                    }
                    ListDataItem ei = new ListDataItem();
                    ei.ID = en.ToString();
                    ei.Text = en.GetText();
                    ei.Value = en.ToInt32();
                    ei.Sort = en.GetSort();
                    list.Add(ei);
                }
                list = list.OrderBy(x => x.Sort).ThenBy(x=>x.Value).ToList();
            }
            return list;
        }

    }

    /// <summary>自訂轉型</summary>
    public static class Trans 
    {
        /// <summary>自訂轉型</summary>
        /// <typeparam name="T">轉換目標型態</typeparam>
        /// <param name="value">值</param>
        /// <returns></returns>
        public static T TryParse<T>(object value) where  T :new() 
        {
            return (T)TryParse<T>(value.TryParseString());
        }

        /// <summary>自訂轉型</summary>
        /// <typeparam name="T">轉換目標型態</typeparam>
        /// <param name="value">值</param>
        /// <returns></returns>
        public static T TryParse<T>(string value)  where  T :new()
        {
            T item = new T();
            MethodInfo mh = item.GetType().GetMethod("TryParse", new Type[] { typeof(string), typeof(T).MakeByRefType() });
            object[] parameters = new object[] { value, null };
            mh.Invoke(item, parameters);
            return (T)parameters[1];
        } 
    }

  
}
