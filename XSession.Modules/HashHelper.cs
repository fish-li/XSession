using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace XSession.Modules
{
    /// <summary>
    /// 封装常用的HASH算法
    /// </summary>
    internal static class HashHelper
    {
        private static string HashString(HashAlgorithm hash, string text, Encoding encoding)
        {
            if( text == null )
                throw new ArgumentNullException(nameof(text));

            if( encoding == null )
                encoding = Encoding.UTF8;

            byte[] bb = encoding.GetBytes(text);
            byte[] buffer = hash.ComputeHash(bb);
            return buffer.ToHexString();
        }

        /// <summary>
		/// 将byte[]按十六进制转换成字符串，BitConverter.ToString(bytes).Replace("-", "");
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static string ToHexString(this byte[] bytes)
        {
            if( bytes == null || bytes.Length == 0 )
                return string.Empty;

            return BitConverter.ToString(bytes).Replace("-", "");
        }


        /// <summary>
        /// 计算字符串的 SHA1 签名
        /// </summary>
        /// <param name="text"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string Sha1(this string text, Encoding encoding = null)
        {
            using( SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider() ) {
                return HashString(sha1, text, encoding);
            }
        }




        /// <summary>
        /// 计算字符串的 MD5 签名
        /// </summary>
        /// <param name="text"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string Md5(this string text, Encoding encoding = null)
        {
            using( MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider() ) {
                return HashString(md5, text, encoding);
            }
        }




    }
}
