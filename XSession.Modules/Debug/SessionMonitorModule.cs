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
    public sealed class SessionMonitorModule : IHttpModule
    {
        private static readonly string ItemKey = "DebugInfo#4020dac915674b6bbb7550ab29fb3bdc";
        internal static readonly bool EnableSessionMonitor;

        static SessionMonitorModule()
        {
            // 增加一个参数，用于紧急情况下，线上查看生产环境数据
            bool flag = System.Configuration.ConfigurationManager.AppSettings["XSession.SessionMonitorModule.Enabled"] == "1";

            // 默认启用条件：【非生产环境】，或者特殊情况下临时开启。
            EnableSessionMonitor = (Initializer.IsProdEnvironment == false) || flag;

            // 说明
            // 理论上这样的判断有些多余，如果希望生产环境不启用当前Module，可以不配置它，
            // 然而实际情况下，我们的同事在实际部署时，几乎都是会从测试环境中复制配置文件，不能指望他们去调整这些配置，
            // 所以，为了避免影响产生环境的性能，Module内部根据软件狗的类型来做环境判断，生产环境就默认不工作。
        }

        public void Init(HttpApplication app)
        {
            Initializer.Init();
   
            if( EnableSessionMonitor ) {
                app.BeginRequest += App_BeginRequest;

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
