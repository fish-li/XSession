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
            s.AppendLine("------------------------------------------------------");
            s.AppendLine($"IsProd: {Initializer.IsProdEnvironment}");
            s.AppendLine($"Is64Bit: {Initializer.Is64Bit}");
            s.AppendLine("------------------------------------------------------");
            s.AppendLine($"CustomProvider: {Initializer.SessionConfig.CustomProvider}");
            s.AppendLine($"Timeout: {Initializer.SessionConfig.Timeout}");
            s.AppendLine("------------------------------------------------------");
            s.AppendLine($"Cache.Count: {HttpRuntime.Cache.Count}");
            s.AppendLine($"EffectiveMemory(MB): {(long)(HttpRuntime.Cache.EffectivePrivateBytesLimit /1024.0/1024.0)}");

            context.Response.Write(s.ToString());
        }
    }
}
