using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Configuration;

namespace XSession.Modules
{
    internal static class Initializer
    {
        private static readonly object s_initLock = new object();
        private static bool s_inited = false;

        public static readonly string FlagFile = "__app.txt";

        public static SessionStateSection SessionConfig { get; private set; }

        public static string TempPath { get; private set; }

        public static bool IsProdEnvironment { get; private set; }

        public static bool Is64Bit { get; set; }

        private static Thread s_cleanThread;


        public static void Init()
        {
            if( s_inited == false ) {
                lock( s_initLock ) {
                    if( s_inited == false ) {

                        // 为【当前站点】准备【专属临时目录】，因为有可能这段代码运行在多个站点中
                        TempPath = InitTempRoot();

                        // 获取当前是否为生产环境
                        IsProdEnvironment = GetEnvironment();

                        // 判断当前进程是否为64位
                        Is64Bit = IntPtr.Size == 8;

                        // 获取 Session 相关配置
                        SessionConfig = ConfigurationManager.GetSection("system.web/sessionState") as SessionStateSection;

                        // 启动后台线程，定时清除过期的文件
                        StartCleanTask();

                        s_inited = true;
                    }
                }
            }
        }


        private static string InitTempRoot()
        {
            // 顶层根目录
            string tempPath = System.Configuration.ConfigurationManager.AppSettings["XSession.FileSessionStateStore.TempPath"];
            if( string.IsNullOrEmpty(tempPath) )
                tempPath = Path.GetTempPath();

            // 为当前站点再创建一个子目录
            string dirName = HashHelper.Sha1(HttpRuntime.AppDomainAppPath);

            // 计算当前站点的Session临时目录，并保存到静态变量中，然后创建目录
            string root = Path.Combine(tempPath, dirName);
            Directory.CreateDirectory(root);

            // 在临时目录中写入一个标志文件，记录当前站点的路径，便于人工识别
            File.WriteAllText(Path.Combine(root, FlagFile), HttpRuntime.AppDomainAppPath, Encoding.UTF8);

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
            s_cleanThread = new Thread(FileCleanTask.Start);
            s_cleanThread.IsBackground = true;
            s_cleanThread.Name = typeof(FileCleanTask).Name;
            s_cleanThread.Start();
        }

    }
}
