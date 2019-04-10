using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace JT809.PubSub.Abstractions
{
    public class JT809HashAlgorithm
    {
        /// <summary>
        /// 使用Ketama
        /// </summary>
        /// <param name="digest"></param>
        /// <param name="nTime"></param>
        /// <returns></returns>
        public static long Hash(byte[] digest, int nTime=1)
        {
            long rv = ((long)(digest[3 + nTime * 4] & 0xFF) << 24)
                    | ((long)(digest[2 + nTime * 4] & 0xFF) << 16)
                    | ((long)(digest[1 + nTime * 4] & 0xFF) << 8)
                    | ((long)digest[0 + nTime * 4] & 0xFF);
            return rv & 0xffffffffL;
        }

        public static byte[] ComputeMd5(string key)
        {
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                byte[] keyBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(key));
                md5.Clear();
                return keyBytes;
            }
        }
    }
}
