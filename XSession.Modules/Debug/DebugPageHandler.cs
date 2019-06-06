using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace XSession.Modules.Debug
{
    internal sealed class DebugPageHandler : IHttpHandler
    {
        internal static string TemplateHtml { get; private set; }

        static DebugPageHandler()
        {
            using( Stream stream = typeof(FileListHandler).Assembly.GetManifestResourceStream("XSession.Modules.Debug.DebugPageTemplate.html") ) {
                using( StreamReader reader = new StreamReader(stream, Encoding.UTF8) ) {
                    TemplateHtml = reader.ReadToEnd();
                }
            }
        }

        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/html";

            string dllPath = typeof(DebugPageHandler).Assembly.Location;
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(dllPath);


            StringBuilder s = new StringBuilder();
            s.AppendLine("<pre>");

            s.AppendLine($"Version: {versionInfo.FileVersion}");
            s.AppendLine("------------------------------------------------------");

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
            s.AppendLine("------------------------------------------------------");            
            s.AppendLine($"ErrorDataCount: {System.Threading.Interlocked.Read(ref SessionDataUtils.DeleteErrorDataCount)}");
            s.AppendLine("------------------------------------------------------");

            s.AppendLine("Session DataTypes:");
            s.AppendLine("  string");
            foreach(var key in SessionDataUtils.SessionDataTypes.Keys)
                s.AppendLine("  " + key.ToString());

            s.AppendLine("</pre>");

            string html = TemplateHtml.Replace("<!--{pagebody}-->", s.ToString());
            context.Response.Write(html);
        }
    }
}
