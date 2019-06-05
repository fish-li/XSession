using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;

namespace XSession.Modules.Debug
{
    public sealed class SessionDetectionModule : IHttpModule
    {
        private static readonly string ItemKey = "DebugInfo#4020dac915674b6bbb7550ab29fb3bdc";


        public void Init(HttpApplication app)
        {
            Initializer.Init();

            app.BeginRequest += App_BeginRequest;


            // 生产环境不工作，避免影响性能
            if( Initializer.IsProdEnvironment == false ) {
                app.PostRequestHandlerExecute += App_PostRequestHandlerExecute;
                app.UpdateRequestCache += App_UpdateRequestCache;
            }
        }

        private void App_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;

            IHttpHandler handler = null;

            if( app.Request.Path.Equals("/XSession/SessionList.aspx", StringComparison.OrdinalIgnoreCase) ) {
                handler = new SessionListHandler();
            }
            else if( app.Request.Path.Equals("/XSession/FileList.aspx", StringComparison.OrdinalIgnoreCase) ) {
                handler = new FileListHandler();
            }
            else if( app.Request.Path.Equals("/XSession/Delete.aspx", StringComparison.OrdinalIgnoreCase) ) {
                handler = new SessionDeleteHandler();
            }
            else if( app.Request.Path.Equals("/XSession/ShowDebug.aspx", StringComparison.OrdinalIgnoreCase) ) {
                handler = new ShowDebugHandler();
            }
            else if( app.Request.Path.Equals("/XSession/Detail.aspx", StringComparison.OrdinalIgnoreCase) ) {
                handler = new SessionDetailHandler();
            }

            if( handler != null ) {
                handler.ProcessRequest(app.Context);
                app.Response.End();
            }
        }


        private void App_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;

            if( app.Context.Session == null )
                return;

            var session = app.Context.Session;

            PropertyInfo property = session.GetType().GetProperty("Container", BindingFlags.Instance | BindingFlags.NonPublic);
            object stateContainer = property.GetValue(session, null);

            FieldInfo field = stateContainer.GetType().GetField("_sessionItems", BindingFlags.Instance | BindingFlags.NonPublic);
            object sessionItems = field.GetValue(stateContainer);

            PropertyInfo property2 = sessionItems.GetType().GetProperty("Dirty", BindingFlags.Instance | BindingFlags.NonPublic| BindingFlags.Public);
            bool dirty = (bool)property2.GetValue(sessionItems, null);


            DebugInfo debugInfo = DebugInfoHelper.CreateDebugInfo(app.Context);
            app.Context.Items[ItemKey] = debugInfo;


            if( dirty == false ) {
                // 还原状态，避免Session重新写入
                property2.SetValue(sessionItems, false, null);
            }
        }


       
        private void App_UpdateRequestCache(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;

            DebugInfo debugInfo = app.Context.Items[ItemKey] as DebugInfo;

            if( debugInfo == null )
                return;

            DebugInfoHelper.SetSize(debugInfo);
            
            DataQueue.Add(debugInfo);
        }


        public void Dispose()
        {
        }

        
    }
}
