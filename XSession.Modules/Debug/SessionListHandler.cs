using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace XSession.Modules.Debug
{
    internal sealed class SessionListHandler : IHttpHandler
    {
        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            
            StringBuilder s = new StringBuilder();

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

            if( s.Length == 0 )
                s.AppendLine("None");

            context.Response.Write(s.ToString());
        }
    }
}
