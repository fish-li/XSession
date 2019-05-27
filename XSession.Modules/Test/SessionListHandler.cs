using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace XSession.Modules.Test
{
    internal sealed class SessionListHandler : IHttpHandler
    {
        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            if( FileHelper.IsProdEnvironment ) {
                context.Response.Write("当前站点为生产环境，没有调试记录。");
                return;
            }


            
            StringBuilder s = new StringBuilder();

            s.AppendLine(FileHelper.TempPath);
            s.AppendLine(new string('-', 60));

            List<DebugInfo> list = DataQueue.GetAll().OrderByDescending(x => x.Time).ToList();

            int i = 1;
            foreach(var x in list ) {
                s.AppendLine($"#{i++}");
                s.AppendLine($"Time: {x.Time.ToString("yyyy-MM-dd HH:mm:ss")}");
                s.AppendLine($"Url: {x.Url}");
                s.AppendLine($"Session Id: {x.SessionId}, Size: {x.Size}");

                int j = 1;
                foreach( var n in x.Items )
                    s.AppendLine($"  #{j++}, {n}");

                s.AppendLine(new string('-', 60));
            }

            context.Response.Write(s.ToString());
        }
    }
}
