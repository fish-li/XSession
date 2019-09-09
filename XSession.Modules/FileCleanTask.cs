using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XSession.Modules
{
    internal static class FileCleanTask
    {
        public static void Start()
        {
            while( true ) {

                // 10 分钟启动一次清理工作
                for( int i = 0; i < 60; i++ )
                    System.Threading.Thread.Sleep(10 * 1000);  // 一次休眠 10 秒


                try {
                    Execute();
                }
                catch (Exception ex) {
                    string message = "XSession.Modules.FileCleanTask ERROR:\r\n" + ex.ToString();
                    WinLogger.WriteError(message);
                }
            }
        }


        private static void Execute()
        {
            DateTime now = DateTime.Now;
            TimeSpan timeout = Initializer.SessionConfig.Timeout.Add(TimeSpan.FromMinutes(5)); // 多加 5 分钟

            StringBuilder s = new StringBuilder();

            foreach(string filePath in Directory.GetFiles(Initializer.TempPath, "*.dat") ) {

                if( File.Exists(filePath) == false )
                    continue;

                FileInfo file = new FileInfo(filePath);

                bool delete = file.LastAccessTime.Add(timeout) < now;

                if( delete ) {
                    s.AppendLine(filePath);
                    RetryFile.Delete(filePath);

                    // 删除内存中的用户锁对象，以免内存泄露
                    string sessionId = FileStore.GetSessionId(filePath);
                    SidLock.Instance.RemoveLock(sessionId);
                }
            }

            if( s.Length > 0 ) {
                string message = "XSession.Modules.FileCleanTask 已清理文件:\r\n" + s.ToString();
                WinLogger.WriteInfo(message);
            }
        }


    }
}
