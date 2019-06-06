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
            else if( app.Request.Path.Equals("/XSession/CacheList.aspx", StringComparison.OrdinalIgnoreCase) ) {
                handler = new CacheListHandler();
            }
            else if( app.Request.Path.Equals("/XSession/Delete.aspx", StringComparison.OrdinalIgnoreCase) ) {
                handler = new SessionDeleteHandler();
            }
            else if( app.Request.Path.Equals("/XSession/ShowDebug.aspx", StringComparison.OrdinalIgnoreCase) ) {
                handler = new DebugPageHandler();
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

            // 枚举Session数据时，可能会造成数据被标记为【已修改】，所以要先把标记拿到，枚举结束后恢复。
            PropertyInfo containerProperty = session.GetType().GetProperty("Container", BindingFlags.Instance | BindingFlags.NonPublic);
            object stateContainer = containerProperty.GetValue(session, null);

            FieldInfo sessionItemsField = stateContainer.GetType().GetField("_sessionItems", BindingFlags.Instance | BindingFlags.NonPublic);
            object sessionItems = sessionItemsField.GetValue(stateContainer);

            PropertyInfo dirtyProperty = sessionItems.GetType().GetProperty("Dirty", BindingFlags.Instance | BindingFlags.NonPublic| BindingFlags.Public);
            bool dirty = (bool)dirtyProperty.GetValue(sessionItems, null);


            DebugInfo debugInfo = DebugInfoHelper.CreateDebugInfo(app.Context);
            app.Context.Items[ItemKey] = debugInfo;


            if( dirty == false ) {
                // 还原状态，避免Session重新写入
                dirtyProperty.SetValue(sessionItems, false, null);
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
