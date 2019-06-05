using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace XSession.Modules.Debug
{
    internal static class DebugInfoHelper
    {
        public static DebugInfo CreateDebugInfo(HttpContext context)
        {
            var session = context.Session;

            DebugInfo debugInfo = new DebugInfo {
                Time = DateTime.Now,
                Url = context.Request.Path,
                SessionId = session.SessionID,
                Items = SessionDataUtils.GetDataLines(session, true)
            };

            return debugInfo;
        }
        
 

        public static void SetSize(DebugInfo debugInfo)
        {
             string filePath = FileStore.GetSessionFilePath(debugInfo.SessionId);

            if( File.Exists(filePath) ) {
                FileInfo file = new FileInfo(filePath);
                debugInfo.Size = file.Length;
            }
            else {
                debugInfo.Size = -1;
            }
        }


    }
}
