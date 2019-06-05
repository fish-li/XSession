using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.SessionState;

namespace XSession.Modules.Debug
{
    internal sealed class SessionDetailHandler : IHttpHandler
    {
        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            string sid = context.Request.QueryString["sid"];
            if( string.IsNullOrEmpty(sid)) {
                context.Response.Write("sid not found!");
                return;
            }

            try {
                FileSessionStateStore store = new FileSessionStateStore();
                SessionStateStoreData data = store.DoGet(context, sid, false);

                var container = new HttpSessionStateContainer(sid, data.Items, null, 10, false, HttpCookieMode.UseCookies, SessionStateMode.Custom, true);

                var ctor = typeof(HttpSessionState).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(IHttpSessionState) }, null);

                HttpSessionState session = (HttpSessionState)ctor.Invoke(new object[] { container });

                List<string> items = SessionDataUtils.GetDataLines(session, false);

                //List<string> items = new List<string>();
                //Dictionary<string, object> dict = SessionUtils.GetKeyValues(session);

                //foreach(var kv in dict ) {
                //    string line = DebugInfoHelper.GetDataLine(kv.Key, kv.Value, false);
                //    items.Add(line);
                //}

                StringBuilder s = new StringBuilder();
                s.AppendLine($"Session Id: {sid}");

                int j = 1;
                foreach( var n in items )
                    s.AppendLine($"  #{j++}, {n}");

                if( s.Length == 0 )
                    s.AppendLine("None");

                context.Response.Write(s.ToString());
            }
            catch(Exception ex ) {
                context.Response.Write(ex.ToString());
            }

        }
    }
}
