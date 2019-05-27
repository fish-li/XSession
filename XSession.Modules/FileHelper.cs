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
    internal static class FileHelper
    {
        private static readonly object s_initLock = new object();
        private static bool s_inited = false;

        public static string TempPath { get; private set; }
        public static bool IsProdEnvironment { get; private set; }
        private static Thread s_cleanTask;

        private static readonly object s_lock = new object();
        private static Hashtable s_hashtable = new Hashtable(5300);

        public static void Init()
        {
            if( s_inited == false ) {
                lock( s_initLock ) {
                    if( s_inited == false ) {

                        // 为【当前站点】准备【专属临时目录】，因为有可能这段代码运行在多个站点中
                        TempPath = InitTempRoot();

                        // 获取当前是否为生产环境
                        IsProdEnvironment = GetEnvironment();

                        // 启动后台线程，定时清除过期的文件
                        //StartCleanTask();

                        s_inited = true;
                    }
                }
            }
        }


        private static string InitTempRoot()
        {
            // 顶层根目录
            string tempPath = System.Configuration.ConfigurationManager.AppSettings["FileSessionStateStore.TempPath"];
            if( string.IsNullOrEmpty(tempPath) )
                tempPath = Path.GetTempPath();

            // 为当前站点再创建一个子目录
            string dirName = HashHelper.Sha1(HttpRuntime.AppDomainAppPath);

            // 计算当前站点的Session临时目录，并保存到静态变量中，然后创建目录
            string root = Path.Combine(tempPath, dirName);
            Directory.CreateDirectory(root);

            // 在临时目录中写入一个标志文件，记录当前站点的路径，便于人工识别
            File.WriteAllText(Path.Combine(root, "__app.txt"), HttpRuntime.AppDomainAppPath, Encoding.UTF8);

            return root;
        }

        private static bool GetEnvironment()
        {
            // 获取狗类型，用于识别环境
            string dogType = System.Configuration.ConfigurationManager.AppSettings["SoftDogType"];
            return (string.Compare(dogType, "ProductDog", true) == 0
                    || string.Compare(dogType, "ProductBakDog", true) == 0
                    || string.Compare(dogType, "CloudDog", true) == 0
                    || string.Compare(dogType, "CloudDog2", true) == 0
                    || string.Compare(dogType, "Aladdin", true) == 0
                    || string.Compare(dogType, "AladdinHL", true) == 0
                    );
        }


        private static void StartCleanTask()
        {
            s_cleanTask = new Thread(FileCleanTask.Start);
            s_cleanTask.IsBackground = true;
            s_cleanTask.Start();
        }

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

        public static string GetSessionFilePath(HttpContext context, string id)
        {
            string filename = id + ".dat";
            return Path.Combine(TempPath, filename);
        }


        public static void SaveToFile(byte[] bytes, HttpContext context, string id)
        {
            if( bytes == null )
                return;

            //if( s_isProdEnvironment == false ) {
            //    if( bytes.Length > 4 * 1024 * 1024 )
            //        throw new InvalidOperationException($"Session数据量过大，当前已达到 {bytes.Length}");
            //}

            string filePath = GetSessionFilePath(context, id);

            object lockObject = GetLock(id);

            lock( lockObject ) {
                RetryFile.WriteFile(filePath, bytes);
                File.SetLastAccessTime(filePath, DateTime.Now);
            }
        }


        public static byte[] ReadFile(HttpContext context, string id)
        {
            string filePath = GetSessionFilePath(context, id);

            object lockObject = GetLock(id);

            lock( lockObject ) {
                if( File.Exists(filePath) ) {
                    byte[] bytes = RetryFile.ReadFile(filePath);
                    File.SetLastAccessTime(filePath, DateTime.Now);

                    return bytes;
                }
                else
                    return null;
            }
        }

        public static void DeleteFile(HttpContext context, string id)
        {
            string filePath = GetSessionFilePath(context, id);

            if( File.Exists(filePath) ) {
                RetryFile.DeleteFile(filePath);
            }
        }






    }
}
