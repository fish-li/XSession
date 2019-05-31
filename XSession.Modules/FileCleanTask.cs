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
            int timeout = Initializer.SessionConfig.Timeout.Minutes + 5; // 多加 5 分钟

            bool flag = Initializer.SessionConfig.CustomProvider == "FastSessionStateStore" && Initializer.Is64Bit;

            StringBuilder s = new StringBuilder();

            foreach(string filePath in Directory.GetFiles(Initializer.TempPath, "*.dat") ) {

                if( File.Exists(filePath) == false )
                    continue;

                FileInfo file = new FileInfo(filePath);

                // 注意：
                // FileSessionStateStore：每次加载Session都会更新文件的LastAccessTime

                // FastSessionStateStore：加载Session数据时不会更新文件的LastAccessTime，只会在Session修改后写入文件（更新LastWriteTime）
                // 如果Session过期，会产缓存过期事件，主动删除Session数据文件，所以这里为了保险起见，只删除2天前没有更新的文件。

                bool delete = flag
                                ? (file.LastWriteTime.AddDays(2) < now)
                                : (file.LastAccessTime.AddMinutes(timeout) < now);

                if( delete ) {
                    s.AppendLine(filePath);
                    RetryFile.Delete(filePath);

                    // 删除内存中的用户锁对象，以免内存泄露
                    string sessionId = FileStore.GetSessionId(filePath);
                    UserLock.XInstance.RemoveLock(sessionId);
                }
            }

            if( s.Length > 0 ) {
                string message = "XSession.Modules.FileCleanTask 已清理文件:\r\n" + s.ToString();
                WinLogger.WriteInfo(message);
            }
        }


    }
}
