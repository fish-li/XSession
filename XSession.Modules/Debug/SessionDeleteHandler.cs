using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace XSession.Modules.Debug
{
    internal sealed class SessionDeleteHandler : IHttpHandler
    {
        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            string name = context.Request.QueryString["sid"];

            if( string.IsNullOrEmpty(name) ) {
                context.Response.Write("EMPTY");
                return;
            }


            if( name == "ALL" )
                DeleteAll(context);

            else
                DeletOne(context, name);
        }


        private void DeleteAll(HttpContext context)
        {
            if( Initializer.IsProdEnvironment ) {
                context.Response.Write("ignore!");
                return;
            }

            StringBuilder s = new StringBuilder();
            s.AppendLine("Delete files:")
                .AppendLine(new string('-', 60));

            foreach( string file in Directory.GetFiles(Initializer.TempPath, "*.dat") ) {
                s.AppendLine(file);
                File.Delete(file);
            }

            context.Response.Write(s.ToString());
        }


        private void DeletOne(HttpContext context, string name)
        {
            string filePath = Path.Combine(Initializer.TempPath, name + ".dat");
            if( File.Exists(filePath) ) {

                File.Delete(filePath);
                context.Response.Write("delete file successed!");
            }
            else {
                context.Response.Write("file not found!");
            }
        }
    }
}
