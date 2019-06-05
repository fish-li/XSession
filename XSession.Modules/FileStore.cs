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
        public static string GetSessionFilePath(string sessionId)
        {
            string filename = sessionId + ".dat";
            return Path.Combine(Initializer.TempPath, filename);
        }

        public static string GetSessionId(string sessionFilePath)
        {
            return Path.GetFileNameWithoutExtension(sessionFilePath);
        }

        public static void SaveToFile(byte[] bytes,  string sessionId)
        {
            if( bytes == null )
                return;

            string filePath = GetSessionFilePath(sessionId);

            object lockObject = UserLock.XInstance.GetLock(sessionId);

            lock( lockObject ) {
                RetryFile.Write(filePath, bytes);
                File.SetLastAccessTime(filePath, DateTime.Now);
            }
        }


        public static byte[] ReadFile(string sessionId, bool checkTimeout)
        {
            string filePath = GetSessionFilePath(sessionId);

            object lockObject = UserLock.XInstance.GetLock(sessionId);

            lock( lockObject ) {
                if( File.Exists(filePath) ) {
                    DateTime now = DateTime.Now;

                    if( checkTimeout ) {
                        // 从文件加载时，检查数据是否已过期
                        DateTime time = File.GetLastAccessTime(filePath);

                        // Session数据已过期
                        if( time.Add(Initializer.SessionConfig.Timeout) < now )
                            return null;
                    }
                    
                    byte[] bytes = RetryFile.Read(filePath);

                    if( checkTimeout ) {
                        File.SetLastAccessTime(filePath, now);
                    }

                    return bytes;
                }
                else
                    return null;
            }
        }

        public static void DeleteFile(string sessionId)
        {
            string filePath = GetSessionFilePath(sessionId);

            object lockObject = UserLock.XInstance.GetLock(sessionId);

            lock( lockObject ) {
                if( File.Exists(filePath) ) {
                    RetryFile.Delete(filePath);
                }
            }
            
            UserLock.XInstance.RemoveLock(sessionId);
        }



        public static void SetLastAccessTime(string sessionId, DateTime time)
        {
            string filePath = GetSessionFilePath(sessionId);

            object lockObject = UserLock.XInstance.GetLock(sessionId);

            lock( lockObject ) {
                File.SetLastAccessTime(filePath, time);
            }
        }



    }
}
