using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace XSession.Modules.Test
{
    public sealed class SessionDetectionModule : IHttpModule
    {
        private static readonly string ItemKey = "4020dac915674b6bbb7550ab29fb3bdc";

        public void Init(HttpApplication app)
        {
            FileHelper.Init();

            app.BeginRequest += App_BeginRequest;


            // 生产环境不工作，避免影响性能
            if( FileHelper.IsProdEnvironment == false ) {                
                app.PostRequestHandlerExecute += App_PostRequestHandlerExecute;
                app.UpdateRequestCache += App_UpdateRequestCache;
            }
        }

        private void App_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;

            IHttpHandler handler = null;

            if( app.Request.Path.Equals("/debug/SessionList.aspx", StringComparison.OrdinalIgnoreCase) ) {
                handler = new SessionListHandler();
            }
            else if( app.Request.Path.Equals("/debug/FileList.aspx", StringComparison.OrdinalIgnoreCase) ) {
                handler = new FileListHandler();
            }
            else if( app.Request.Path.Equals("/debug/SessionDelete.aspx", StringComparison.OrdinalIgnoreCase) ) {
                handler = new SessionDeleteHandler();
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


            string[] names = session.Keys.Cast<string>().OrderBy(x => x).ToArray();
            List<string> items = new List<string>();

            foreach(string x in names ) {

                object value = session[x];
                if( value == null ) {
                    items.Add($"{x} = NULL");
                    continue;
                }

                Type dataType = value.GetType();

                if( dataType == typeof(string) ) {
                    string text = (string)value;
                    string display = (text.Length <= 100 ? text : text.Substring(0, 100) + "...").Replace("\r", "\\r").Replace("\n", "\\n");
                    items.Add($"{x} = {display} , ({text.Length})");
                    continue;
                }

                if( dataType == typeof(byte[]) ) {
                    items.Add($"{x} = {dataType.FullName}, length: {((byte[])value).Length}");
                    continue;
                }

                if( value is ICollection ) {
                    ICollection collection = value as ICollection;
                    items.Add($"{x} = {dataType.FullName}, count: {collection.Count}");
                    continue;
                }

                if( value is DataTable ) {
                    DataTable table = value as DataTable;
                    items.Add($"{x} = {dataType.FullName}, rows: {table.Rows.Count}, cols: {table.Columns.Count}");
                    continue;
                }

                items.Add($"{x} = {dataType.FullName}");
            }

            DebugInfo debugInfo = new DebugInfo {
                Time = DateTime.Now,
                Url = app.Request.Path,
                SessionId = session.SessionID,
                Items = items
            };

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

            
            string filePath = FileHelper.GetSessionFilePath(app.Context, debugInfo.SessionId);

            FileInfo file = File.Exists(filePath) ? new FileInfo(filePath) : null;
            file.Refresh();

            debugInfo.Size = (file == null ? -1 : file.Length);
            
            DataQueue.Add(debugInfo);
        }


        public void Dispose()
        {
        }

        
    }
}
