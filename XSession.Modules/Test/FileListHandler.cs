using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace XSession.Modules.Test
{
    internal sealed class FileListHandler : IHttpHandler
    {
        private static string s_template;

        static FileListHandler()
        {
            Stream stream = typeof(FileListHandler).Assembly.GetManifestResourceStream("XSession.Modules.Test.FileListTemplate.html");
            using( StreamReader reader = new StreamReader(stream, Encoding.UTF8) ) {
                s_template = reader.ReadToEnd();
            }
        }

        public bool IsReusable => false;


        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/html";

            StringBuilder html = new StringBuilder();

            string tempPath = FileHelper.TempPath;
            DirectoryInfo dir = new DirectoryInfo(tempPath);

            int index = 1;
            string rowFormat2 = "<tr><td>{0}</td><td>{1}</td><td class=\"right\">{2}</td><td class=\"right\">{3}</td><td class=\"right\">{4}</td></tr>\r\n";

            foreach( FileInfo f in new DirectoryInfo(tempPath).GetFiles()) {
                f.Refresh();
                html.AppendFormat(rowFormat2, 
                    index++,  
                    f.Name, 
                    f.Length.ToString("N0"), 
                    f.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"), f.LastAccessTime.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            string result = s_template
                        .Replace("<!--{current-path}-->", tempPath)
                        .Replace("<!--{data-row}-->", html.ToString());

            context.Response.Write(result);
        }
    }
}
