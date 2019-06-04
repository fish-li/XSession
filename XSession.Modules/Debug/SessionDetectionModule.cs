using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace XSession.Modules.Debug
{
    public sealed class SessionDetectionModule : IHttpModule
    {
        private static readonly string ItemKey = "DebugInfo#4020dac915674b6bbb7550ab29fb3bdc";

        private static readonly object s_lock = new object();
        internal static Hashtable SessionDataTypes = new Hashtable(256);


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


            CreateDebugInfo(app);


            if( dirty == false ) {
                // 还原状态，避免Session重新写入
                property2.SetValue(sessionItems, false, null);
            }
        }


        private void CreateDebugInfo(HttpApplication app)
        {
            var session = app.Context.Session;

            string[] names = session.Keys.Cast<string>().OrderBy(x => x).ToArray();
            List<string> items = new List<string>();

            foreach( string x in names ) {

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

                // 记录包含了哪些数据类型
                if( SessionDataTypes.ContainsKey(dataType) == false ) {
                    lock( s_lock ) {
                        SessionDataTypes[dataType] = "xx";
                    }
                }


                if( dataType == typeof(byte[]) ) {
                    items.Add($"{x} = {dataType.ToString()}, length: {((byte[])value).Length}");
                    continue;
                }

                if( value is ICollection ) {
                    ICollection collection = value as ICollection;
                    items.Add($"{x} = {dataType.ToString()}, count: {collection.Count}");
                    continue;
                }

                if( value is DataTable ) {
                    DataTable table = value as DataTable;
                    items.Add($"{x} = {dataType.ToString()}, rows: {table.Rows.Count}, cols: {table.Columns.Count}");
                    continue;
                }

                items.Add($"{x} = {dataType.ToString()}");
            }

            DebugInfo debugInfo = new DebugInfo {
                Time = DateTime.Now,
                Url = app.Request.Path,
                SessionId = session.SessionID,
                Items = items
            };

            app.Context.Items[ItemKey] = debugInfo;

        }

        private void App_UpdateRequestCache(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;

            DebugInfo debugInfo = app.Context.Items[ItemKey] as DebugInfo;
            if( debugInfo == null )
                return;

            
            string filePath = FileStore.GetSessionFilePath(debugInfo.SessionId);

            if( File.Exists(filePath) ) {
                FileInfo file = new FileInfo(filePath);
                debugInfo.Size = file.Length;
            }
            else {
                debugInfo.Size =  -1 ;
            }
            
            DataQueue.Add(debugInfo);
        }


        public void Dispose()
        {
        }

        
    }
}
