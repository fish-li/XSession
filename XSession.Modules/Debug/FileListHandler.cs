using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace XSession.Modules.Debug
{
    internal sealed class FileListHandler : IHttpHandler
    {
        private static string s_template;

        static FileListHandler()
        {
            Stream stream = typeof(FileListHandler).Assembly.GetManifestResourceStream("XSession.Modules.Debug.FileListTemplate.html");
            using( StreamReader reader = new StreamReader(stream, Encoding.UTF8) ) {
                s_template = reader.ReadToEnd();
            }
        }

        public bool IsReusable => false;


        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/html";

            StringBuilder html = new StringBuilder();

            DirectoryInfo dir = new DirectoryInfo(Initializer.TempPath);

            int index = 1;
            string rowFormat2 = "<tr><td>{0}</td><td>{1}</td><td class=\"right\">{2}</td><td class=\"right\">{3}</td><td class=\"right\">{4}</td></tr>\r\n";

            FileInfo[] files = (from x in dir.GetFiles("*.dat")
                                  where File.Exists(x.FullName)
                                  orderby x.LastWriteTime descending
                                  select x).ToArray();

            foreach( FileInfo file in files ) {

                string link = $"<a href='/XSession/Detail.aspx?sid={Path.GetFileNameWithoutExtension(file.FullName)}' target='blank'>{file.Name}</a>";
                string line = string.Format(rowFormat2,
                    index++,
                    link,
                    file.Length.ToString("N0"),
                    file.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"), 
                    file.LastAccessTime.ToString("yyyy-MM-dd HH:mm:ss"));

                html.AppendLine(line);
            }

            string result = s_template
                        .Replace("<!--{current-path}-->", Initializer.TempPath)
                        .Replace("<!--{data-row}-->", html.ToString());

            context.Response.Write(result);
        }
    }
}
