using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace XSession.Modules.Debug
{
    internal class CacheListHandler : IHttpHandler
    {
        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            StringBuilder s = new StringBuilder();

            int i = 0;
            foreach( DictionaryEntry x in HttpRuntime.Cache ) {
                i++;
                string value = x.Value == null ? "NULL" : x.Value.GetType().ToString();
                s.AppendLine($"#{i}, {x.Key}: {value}");
            }

            if( s.Length == 0 )
                s.AppendLine("None");

            context.Response.Write(s.ToString());
        }
    }
}
