using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplication1
{
    public partial class WriteSession : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // 模拟长字符串
            Session["long-string" + DateTime.Now.Millisecond] = new string('x', 4096);

            // 模拟长XML
            Session["xml-string"] = File.ReadAllText(Path.Combine(HttpRuntime.AppDomainAppPath, "web.config"), Encoding.UTF8);


            Session["DateTime1"] = DateTime.Now;
            Session["Guid1"] = Guid.NewGuid();
            Session["long1"] = 252425234L;
            Session["int2"] = 235;
            Session["unit3"] = 423U;
            Session["bool1"] = true;

            Session["IntArray"] = new int[] { 1, 2, 3, 4, 5 };
            Session["StringArray"] = new string[] { "aaa", "bbb", "ccc" };

            // 模拟 List<string>
            List<string> list = new List<string>();
            for( int i = 0; i < 102; i++ )
                list.Add(Guid.NewGuid().ToString() + new string('x', 20));
            Session["List"] = list;


            // 模拟 NameValueCollection
            NameValueCollection collection = new NameValueCollection();
            foreach(string key in this.Request.Headers) {
                string value = this.Request.Headers[key];
                collection.Add(key, value);
            }
            foreach( string key in this.Request.ServerVariables ) {
                string value = this.Request.ServerVariables[key];
                collection.Add(key, value);
            }
            Session["NameValueCollection"] = collection;


            CookieContainer cookieContainer = new CookieContainer();
            cookieContainer.Add(new Uri("http://www.abc.com/"), new Cookie("c1", "aaaaaaaaaaaaaa"));
            cookieContainer.Add(new Uri("http://www.abc.com/"), new Cookie("c2", "bbbbbbbbbbbbbb"));
            cookieContainer.Add(new Uri("http://www.abc.com/"), new Cookie("c3", "cccccccccccccc"));
            Session["CookieContainer"] = cookieContainer;


            // 模拟 Dictionary
            Dictionary<string, DateTime> dict = new Dictionary<string, DateTime>();
            for(int i=0;i<100;i++ ) {
                dict[Guid.NewGuid().ToString()] = new DateTime(2000 + i, 5, 3);
            }
            Session["Dictionary"] = dict;


            // 模拟 byte[]
            string binFile = Path.Combine(HttpRuntime.AppDomainAppPath, @"App_Data\Snipaste_2019-05-27_14-24-44.png");
            Session["ImageFile"] = File.ReadAllBytes(binFile);


            // 模拟 DataSet
            string datasetXml = Path.Combine(HttpRuntime.AppDomainAppPath, @"App_Data\dataset.xml");

            if( File.Exists(datasetXml) ) {
                DataSet ds = new DataSet();
                ds.ReadXml(datasetXml);
                Session["DataTable"] = ds.Tables[0];
                Session["DataSet"] = ds;
            }


        }
    }
}