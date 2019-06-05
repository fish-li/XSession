using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web.SessionState;

namespace XSession.Modules
{
    internal static class SessionDataUtils
    {
        private static readonly object s_lock = new object();
        internal static Hashtable SessionDataTypes = new Hashtable(256);

        public static long DeleteErrorDataCount = 0;


        public static List<string> GetDataLines(HttpSessionState session, bool recordType)
        {
            List<string> items = new List<string>();
            string[] names = null;

            try {
                // 直接访问 session.Keys 会导致Session的所有数据全部还原
                // 此时有一种特殊的数据类型：Microsoft.Reporting.WebForms.ReportHierarchy
                // 它在还原时，【可能】会出现异常，可参考异常详情文件： /doc/RSExecutionConnection-MissingEndpointException.txt

                names = session.Keys.Cast<string>().ToArray();
            }
            catch {
                // Session还原数据时发生了异常，这里忽略
            }

            if( names != null ) {

                foreach( string x in names ) {
                    object value = session[x];
                    string line = GetLine(x, value, true);
                    items.Add(line);
                }
            }
            else {
                Dictionary<string, object> dict = GetKeyValues(session, recordType);

                foreach( var kv in dict ) {
                    string line = GetLine(kv.Key, kv.Value, recordType);
                    items.Add(line);
                }
            }

            return items;
        }


        public static Dictionary<string, object> GetKeyValues(HttpSessionState session, bool deleteErrorData)
        {
            var container = session.GetType().InvokeMember(
                            "_container",
                            BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic,
                            null, session, null);

            var items = (SessionStateItemCollection)container.GetType().InvokeMember(
                            "_sessionItems",
                            BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic,
                            null, container, null);

            var sitems = items.GetType().InvokeMember(
                            "_serializedItems",
                            BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic,
                            null, items, null);

            MethodInfo method = sitems.GetType().GetMethod("GetKey", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(int) }, null);

            Dictionary<string, object> dict = new Dictionary<string, object>();

            for( int i = 0; i < items.Count; i++ ) {
                string key = (string)method.Invoke(sitems, new object[] { i });

                object value = null;
                try {
                    value = items[key];
                }
                catch( Exception ex ) {
                    value = ex;

                    if( deleteErrorData ) {
                        session.Remove(key);
                        System.Threading.Interlocked.Increment(ref DeleteErrorDataCount);
                    }
                }

                dict[key] = value;
            }

            return dict;
        }



        public static string GetLine(string name, object value, bool recordType)
        {
            if( value == null )
                return $"{name} = NULL";

            if( value is Exception ) {
                Exception ex = value as Exception;
                Exception ex2 = ex.InnerException == null ? ex : ex.InnerException;  // 这里不使用 ex.GetBaseException()，因为RTS的异常描述太长了
                return $"{name} = [ERROR] {ex2.GetType().ToString()}: {ex2.Message}";
            }


            Type dataType = value.GetType();

            if( dataType == typeof(string) ) {
                string text = (string)value;
                string display = (text.Length <= 100 ? text : text.Substring(0, 100) + "...").Replace("\r", "\\r").Replace("\n", "\\n");
                return $"{name} = {display}, ({text.Length})";
            }

            // 记录包含了哪些数据类型
            if( recordType && SessionDataTypes.ContainsKey(dataType) == false ) {
                lock( s_lock ) {
                    SessionDataTypes[dataType] = "xx";
                }
            }

            if( dataType.IsPrimitive || dataType == typeof(DateTime) || dataType == typeof(Guid) ) {
                return $"{name} = {value.ToString()},  ({dataType.ToString()})";
            }

            if( value is CookieContainer ) {
                CookieContainer collection = value as CookieContainer;
                return $"{name} = {dataType.ToString()}, count: {collection.Count}";
            }

            if( dataType == typeof(byte[]) ) {
                return $"{name} = {dataType.ToString()}, length: {((byte[])value).Length}";
            }

            if( value is ICollection ) {
                ICollection collection = value as ICollection;
                return $"{name} = {dataType.ToString()}, count: {collection.Count}";
            }

            if( value is DataTable ) {
                DataTable table = value as DataTable;
                return $"{name} = {dataType.ToString()}, rows: {table.Rows.Count}, cols: {table.Columns.Count}";
            }

            return $"{name} = {dataType.ToString()}";
        }






    }
}
