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
            Session["BUGUID"] = "521b8726-9240-4d10-882d-6c54800ef935";
            Session["BUName"] = "深圳公司";
            Session["UserCode"] = "Admin";
            Session["UserGUID"] = "4230bc6e-69e6-46a9-a39e-b929a06a84e8";
            Session["UserKind"] = "0";
            Session["UserName"] = "系统管理员";
            Session["ClientId"] = "2C89819A3B847046B44DD365B1D7D851";
            Session["CurrentBUGUID"] = "521b8726-9240-4d10-882d-6c54800ef935";
            Session["IsEndCompany"] = "1";
            Session["LogGUID"] = "f057a8e2-c00d-4d6d-ba4c-901f0007f317;02010304";
            Session["LoginData"] = "09701D11925BDCAAE1AF7B2ED6A68DCB";
            Session["LoginDate"] = DateTime.Now;
            Session["MySessionState"] = "f8745a94-d042-415a-ad63-62ae9917b957";
            Session["Password"] = "C4CA4238A0B923820DCC509A6F75849B";
            Session["ProviderServiceCode"] = "05.99,06.01.01,06.01.02,06.01.03,06.01.04,06.01.05,06.01.06,06.01.07,06.01.08,06.01.09,06.01.10,06.005.99,06.01.01,06.01.02,06.01.03,06.01.04,06.01.05,06.01.06,06.01.07,06.01.08,06.01.09,06.01.10,06.005.99,06.01.01,06.01.02,06.01.03,06.01.04,06.01.05,06.01.06,06.01.07,06.01.08,06.01.09,06.01.10,06.0";
            Session["Scope"] = "0201";            
            Session["UserPass"] = "C4CA4238A0B923820DCC509A6F75849B";
 
            Session["DateTime1"] = DateTime.Now;
            Session["Guid1"] = Guid.NewGuid();
            Session["long1"] = 252425234L;
            Session["int2"] = 235;
            Session["unit3"] = 423U;
            Session["bool1"] = true;

            Session["IntArray"] = new int[] { 1, 2, 3, 4, 5 };
            Session["StringArray"] = new string[] { "aaa", "bbb", "ccc" };


            // 模拟长字符串
            Session["long-string" + DateTime.Now.Millisecond] = new string('x', 4096);

            // 模拟长XML
            Session["xml-string" + DateTime.Now.Millisecond] = File.ReadAllText(Path.Combine(HttpRuntime.AppDomainAppPath, "web.config"), Encoding.UTF8);

            // 模拟 List<string>
            List<string> list = new List<string>();
            for( int i = 0; i < 102; i++ )
                list.Add(Guid.NewGuid().ToString() + new string('x', 20));
            Session["List" + DateTime.Now.Millisecond] = list;

            Session["Request.Headers" + DateTime.Now.Millisecond] = Request.Headers;

            // 模拟 NameValueCollection
            NameValueCollection headers = new NameValueCollection();
            foreach(string key in this.Request.Headers.AllKeys) {
                string value = this.Request.Headers[key];
                headers.Add(key, value);
            }
            Session["headers" + DateTime.Now.Millisecond] = headers;

            NameValueCollection serverVariables = new NameValueCollection();
            foreach( string key in this.Request.ServerVariables.AllKeys ) {
                string value = this.Request.ServerVariables[key];
                serverVariables.Add(key, value);
            }
            Session["serverVariables" + DateTime.Now.Millisecond] = serverVariables;


            CookieContainer cookieContainer = new CookieContainer();
            cookieContainer.Add(new Uri("http://www.abc.com/"), new Cookie("c1", "aaaaaaaaaaaaaa"));
            cookieContainer.Add(new Uri("http://www.abc.com/"), new Cookie("c2", "bbbbbbbbbbbbbb"));
            cookieContainer.Add(new Uri("http://www.abc.com/"), new Cookie("c3", "cccccccccccccc"));
            Session["CookieContainer" + DateTime.Now.Millisecond] = cookieContainer;


            // 模拟 Dictionary
            Dictionary<string, DateTime> dict = new Dictionary<string, DateTime>();
            for(int i=0;i<100;i++ ) {
                dict[Guid.NewGuid().ToString()] = new DateTime(2000 + i, 5, 3);
            }
            Session["Dictionary" + DateTime.Now.Millisecond] = dict;


            // 模拟 byte[]
            string binFile = Path.Combine(HttpRuntime.AppDomainAppPath, @"App_Data\Snipaste_2019-05-27_14-24-44.png");
            Session["ImageFile" + DateTime.Now.Millisecond] = File.ReadAllBytes(binFile);


            // 模拟 DataSet
            string datasetXml = Path.Combine(HttpRuntime.AppDomainAppPath, @"App_Data\dataset.xml");

            if( File.Exists(datasetXml) ) {
                DataSet ds = new DataSet();
                ds.ReadXml(datasetXml);
                Session["DataTable" + DateTime.Now.Millisecond] = ds.Tables[0];
                Session["DataSet" + DateTime.Now.Millisecond] = ds;
            }


        }
    }
}