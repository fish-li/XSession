using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XSession.Modules
{
    internal class DemoCode
    {
        /// <summary>
        /// 演示读写与用户关联的临时文件。
        /// 
        /// 可解决【同一用户】多请求的并发执行问题。
        /// </summary>
        public void FileReadWrite()
        {
            // 获取当前用户的SessionId 。  注意：请确保当前请求的Session对象不为NULL
            string sessionId = System.Web.HttpContext.Current.Session.SessionID;

            // 计算与用户相关的文件，用于读写操作
            string filePath = $@"c:\temp\xx_{sessionId}.txt";


            // 获取当前用户关联的锁对象。
            object lockObject = XSession.Modules.UserLock.Instance.GetLock(sessionId);

            lock( lockObject ) {

                // 写文件
                System.IO.File.WriteAllText(filePath, "xxxxxxxxxxxxx", Encoding.UTF8);

                // 读文件
                string text = System.IO.File.ReadAllText(filePath, Encoding.UTF8);
            }
        }
    }
}
