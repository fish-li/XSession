using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace XSession.Modules.Debug
{
    internal sealed class ShowDebugHandler : IHttpHandler
    {
        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            StringBuilder s = new StringBuilder();
            s.AppendLine($"AppPath: {HttpRuntime.AppDomainAppPath}");

            s.AppendLine($"TempPath: {Initializer.TempPath}");
            s.AppendLine($"IsProd: {Initializer.IsProdEnvironment}");
            s.AppendLine($"Is64Bit: {Initializer.Is64Bit}");
            s.AppendLine($"CustomProvider: {Initializer.SessionConfig.CustomProvider}");
            s.AppendLine($"Timeout: {Initializer.SessionConfig.Timeout}");

            context.Response.Write(s.ToString());
        }
    }
}
