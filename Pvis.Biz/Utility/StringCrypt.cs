using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Security.Cryptography;

namespace Pvis.Biz.Utility
{
#pragma warning disable SYSLIB0021
    public class StringCrypt
    {

        /// <summary>系統預設編碼用金鑰 key</summary>
        private const string Default_key = "XASQKJHY";

        /// <summary>系統預設編碼用金鑰 key</summary>
        private static string _default_IV_64_Key = "AC564ACJ";

        public static void ChangeDefaulIV64Key(string key)
        {
            _default_IV_64_Key = key;
        }

        /// <summary>系統預設編碼用向量初始值</summary>
        private static string IV_64_Key
        {
            get { return _default_IV_64_Key; }
        }



        /// <summary>
        /// Encrypt 字串加密
        /// </summary>
        /// <param name="data">須加密字串</param>
        /// <param name="key">自訂KEY(需為8位數字或英文)</param>
        public static string Encrypt(string data, string KEY_64 = Default_key)
        {
            if (data == null || data == string.Empty) return "";

            //加入時戳
            data = DateTime.Now.Ticks.ToString() + "," + data;

            byte[] byKey = Encoding.ASCII.GetBytes(KEY_64??Default_key);
            byte[] byIV = Encoding.ASCII.GetBytes(IV_64_Key);

            try
            {

                DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();

                int i = cryptoProvider.KeySize;
                MemoryStream ms = new MemoryStream();
                CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateEncryptor(byKey, byIV), CryptoStreamMode.Write);

                StreamWriter sw = new StreamWriter(cst);
                sw.Write(data);
                sw.Flush();
                cst.FlushFinalBlock();
                sw.Flush();
                return Convert.ToBase64String(ms.ToArray());
            }
            catch
            {
                return null;
            }

        }

        /// <summary>
        /// Decrypt 字串解密
        /// </summary>
        /// <param name="data"></param>
        /// <param name="Exp"></param>
        /// <returns></returns>

        public static string Decrypt(string data, TimeSpan? Exp = null) {
            return Decrypt(data, Default_key, Exp);
        }

        /// <summary>
        /// Decrypt 字串解密
        /// </summary>
        /// <param name="data">須加解字串</param>
        /// <param name="key">自訂KEY(需為8位數字或英文)</param>
        public static string Decrypt( string data , string KEY_64 = Default_key , TimeSpan? Exp = null   )
        {
            if (data == null || data == string.Empty) return null;

            byte[] byKey = Encoding.ASCII.GetBytes(KEY_64??Default_key);
            byte[] byIV = Encoding.ASCII.GetBytes(IV_64_Key);

            try
            {
                byte[] byEnc = Convert.FromBase64String(data);
                DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
                MemoryStream ms = new MemoryStream(byEnc);
                CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateDecryptor(byKey, byIV), CryptoStreamMode.Read);
                StreamReader sr = new StreamReader(cst);
                var _ResultArray = sr.ReadToEnd().Split(new char[] { ',' }, 2);
                if (_ResultArray.Length != 2) return null;
                if (long.TryParse(_ResultArray[0], out long _ticks) == false) return null;
                if ( IsExpired(_ticks, Exp) ) return null;
                return _ResultArray[1];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 計算是否已經過期
        /// </summary>
        /// <param name="ticks">加密時戳</param>
        /// <param name="Exp">有效期限</param>
        /// <returns></returns>
        private static bool IsExpired(long ticks, TimeSpan? Exp)
        {
            if (!Exp.HasValue) return false;
            return DateTime.Now - new DateTime(ticks) > Exp;
        }


        #region <--MD5 處理 -->
        /// <summary>
        /// 計算MD5雜湊
        /// </summary>
        /// <param name="_Txt"></param>
        /// <returns></returns>
        public static byte[] ToMD5(String _Txt) {
            MD5 md5 = MD5.Create();
            byte[] crypto = md5.ComputeHash(Encoding.UTF8.GetBytes(_Txt));//進行MD5加密
            return crypto;
        }

        /// <summary>
        /// 計算MD5雜湊
        /// </summary>
        /// <param name="_Txt"></param>
        /// <returns></returns>
        public static string ToMD5String(String _Txt)
        {
            return string.Join("", ToMD5(_Txt).Select(x => x.ToString("x2")));
        }
        #endregion

        #region <-- SHA處理. -->
        /// <summary>取得 Share256</summary>
        /// <param name="Txt">加密文字</param>
        /// <param name="salted">混淆值</param>
        /// <returns></returns>
        public static byte[] ToSHA256(String _Txt, string salted = null)
        {
            salted = salted + IV_64_Key;
            byte[] crypto;
            using (SHA256 sha256 = new SHA256CryptoServiceProvider())
            {
                crypto = sha256.ComputeHash(Encoding.UTF8.GetBytes(_Txt + salted));//進行SHA256加密
            }
            return crypto;
        }
        /// <summary>取得 Share256</summary>
        /// <param name="Txt">加密文字</param>
        /// <param name="salted">混淆值</param>
        /// <returns></returns>
        public static byte[] ToSHA512(String _Txt, string salted = null)
        {
            salted = salted + IV_64_Key;
            byte[] crypto;
            using (SHA512 sha512 = new SHA512CryptoServiceProvider())
            {
                crypto = sha512.ComputeHash(Encoding.UTF8.GetBytes(_Txt + salted));//進行SHA256加密
            };
            return crypto;
        }

        /// <summary>轉SHA256字串</summary>
        /// <param name="txt">加密文字</param>
        /// <param name="salted">混淆值</param>
        /// <returns></returns>
        public static string ToSHA256String(string txt, string salted = null)
        {
            return Convert.ToBase64String(ToSHA256(txt, salted));
        }

        /// <summary>轉SHA512字串</summary>
        /// <param name="txt">加密文字</param>
        /// <param name="salted">混淆值</param>
        /// <returns></returns>
        public static string ToSHA512String(string txt, string salted = null)
        {
            return Convert.ToBase64String(ToSHA512(txt, salted));
        }
        #endregion


    }
#pragma warning restore SYSLIB0021
}
