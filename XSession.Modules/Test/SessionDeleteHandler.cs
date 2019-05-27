using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace XSession.Modules.Test
{
    internal sealed class SessionDeleteHandler : IHttpHandler
    {
        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            string name = context.Request.QueryString["file"];

            StringBuilder s = new StringBuilder();

             foreach( string file in Directory.GetFiles(FileHelper.TempPath, name) ) {

                if( file.EndsWith("__app.txt") )
                    continue;

                s.AppendLine(file);
                File.Delete(file);
            }

            context.Response.Write(s.ToString());

        }
    }
}
