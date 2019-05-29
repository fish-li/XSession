using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;

namespace XSession.Modules
{
    internal static class FileStore
    {
        private static readonly object s_lock = new object();
        private static Hashtable s_hashtable = new Hashtable(5300);

        
        private static object GetLock(string key)
        {
            object value = s_hashtable[key];

            if( value == null ) {

                // 只允许一个线程写入
                lock( s_lock ) {

                    // 检查【前面】是否有线程已经插入
                    value = s_hashtable[key];

                    // 确实没有插入，这里执行插入
                    if( value == null ) {
                        value = new object();
                        s_hashtable[key] = value;
                    }
                }
            }

            return value;
        }

        public static string GetSessionFilePath(string id)
        {
            string filename = id + ".dat";
            return Path.Combine(Initializer.TempPath, filename);
        }


        public static void SaveToFile(byte[] bytes,  string id)
        {
            if( bytes == null )
                return;

            //if( s_isProdEnvironment == false ) {
            //    if( bytes.Length > 4 * 1024 * 1024 )
            //        throw new InvalidOperationException($"Session数据量过大，当前已达到 {bytes.Length}");
            //}

            string filePath = GetSessionFilePath(id);

            object lockObject = GetLock(id);

            lock( lockObject ) {
                RetryFile.Write(filePath, bytes);
                File.SetLastAccessTime(filePath, DateTime.Now);
            }
        }


        public static byte[] ReadFile(string id)
        {
            string filePath = GetSessionFilePath(id);

            object lockObject = GetLock(id);

            lock( lockObject ) {
                if( File.Exists(filePath) ) {
                    byte[] bytes = RetryFile.Read(filePath);
                    File.SetLastAccessTime(filePath, DateTime.Now);

                    return bytes;
                }
                else
                    return null;
            }
        }

        public static void DeleteFile(string id)
        {
            string filePath = GetSessionFilePath(id);

            object lockObject = GetLock(id);

            lock( lockObject ) {
                if( File.Exists(filePath) ) {
                    RetryFile.Delete(filePath);
                }
            }
        }






    }
}
