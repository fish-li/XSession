using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XSession.Modules
{
    internal static class RetryFile
    {
        public static void WriteFile(string filePath, byte[] bytes)
        {
            Exception exception = null;

            for( int i = 0; i < 5; i++ ) {
                try {
                    File.WriteAllBytes(filePath, bytes);
                    return;
                }
                catch( IOException ex ) {  // 这类异常要重试
                    exception = ex;
                    System.Threading.Thread.Sleep(500);
                }
            }

            throw new InvalidOperationException($"写文件 {filePath} 失败：{exception.Message}", exception);
        }


        public static byte[] ReadFile(string filePath)
        {
            Exception exception = null;

            for( int i = 0; i < 5; i++ ) {
                try {
                    return File.ReadAllBytes(filePath);
                }
                catch( IOException ex ) {  // 这类异常要重试
                    exception = ex;
                    System.Threading.Thread.Sleep(500);
                }
            }

            throw new InvalidOperationException($"读文件 {filePath} 失败：{exception.Message}", exception);
        }


        public static void DeleteFile(string filePath)
        {
            Exception exception = null;

            for( int i = 0; i < 5; i++ ) {
                try {
                    File.Delete(filePath);
                    return;
                }
                catch( IOException ex ) {  // 这类异常要重试
                    exception = ex;
                    System.Threading.Thread.Sleep(500);
                }
            }

            throw new InvalidOperationException($"删除文件 {filePath} 失败：{exception.Message}", exception);
        }


    }
}
