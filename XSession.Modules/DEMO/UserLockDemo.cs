//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace XSession.Modules.DEMO
//{
//    internal class UserLockDemo
//    {
//        public void 现有代码样例()
//        {
//            // 获取当前用户的 UserGuid 。  注意：请确保当前请求的Session对象不为NULL
//            string userGuid = (string)System.Web.HttpContext.Current.Session["UserGuid"];

//            // 计算与用户相关的文件，用于读写操作
//            string filePath = $@"c:\temp\xx_{userGuid}.txt";

//            // 写文件
//            System.IO.File.WriteAllText(filePath, "xxxxxxxxxxxxx", Encoding.UTF8);

//            // 读文件
//            string text = System.IO.File.ReadAllText(filePath, Encoding.UTF8);
//        }


//        /// <summary>
//        /// 演示读写与用户关联的临时文件。
//        /// 
//        /// 可解决【同一用户】多请求的并发执行问题。
//        /// </summary>
//        public void 调整后的代码()
//        {
//            // 获取当前用户的 UserGuid 。  注意：请确保当前请求的Session对象不为NULL
//            string userGuid = (string)System.Web.HttpContext.Current.Session["UserGuid"];

//            // 计算与用户相关的文件，用于读写操作
//            string filePath = $@"c:\temp\xx_{userGuid}.txt";


//            // 获取当前用户关联的锁对象。
//            object lockObject = XSession.Modules.UserLock.Instance.GetLock(userGuid);

//            lock( lockObject ) {

//                // 写文件
//                System.IO.File.WriteAllText(filePath, "xxxxxxxxxxxxx", Encoding.UTF8);

//                // 读文件
//                string text = System.IO.File.ReadAllText(filePath, Encoding.UTF8);
//            }
//        }
//    }
//}
