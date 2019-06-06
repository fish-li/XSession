<%@ Page Language="C#" AutoEventWireup="true" EnableSessionState="True" CodeBehind="Default.aspx.cs" Inherits="WebApplication1.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
    <style type="text/css">
        * {
            margin: 0;
            padding: 0;
            font-family: Consolas, 'Courier New', '微软雅黑', '宋体';
            font-size: 14px;
            color: #111111;
        }
        body{
            padding: 0;
            margin: 0;
        }
        a{
            text-decoration:none;
        }
        a:hover{
            color: blue;
        }
        form{
            padding: 20px;
        }

        p.navigation{
            font-weight: bold;
            padding: 10px;
            background-color: lightseagreen;
            margin-bottom: 20px;
        }
        p.navigation a {
            color: #fff;
        }
        p.navigation a:hover{
            background-color: forestgreen;
            color: white;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div style="height: 200px; width:99%">
            <iframe src="/WebForm1.aspx"></iframe>
        </div>
        <div style="height: 200px; width:99%">
            <iframe src="/WebForm2.aspx"></iframe>
        </div>

    </form>

       <p class="navigation">
            <a href="/Default.aspx">Refresh</a> &nbsp; &nbsp;
            <a href="/WebForm2.aspx" target="_blank">WebForm2.aspx</a> &nbsp; &nbsp;
            <a href="/WriteSession.aspx" target="_blank">Write Session</a> &nbsp; &nbsp;
            <a href="/DeleteSession.aspx" target="_blank">Delete Session</a> &nbsp; &nbsp;
        </p>
        <p class="navigation">
            <a href="/XSession/SessionList.aspx" target="_blank">SessionList</a> &nbsp; &nbsp;
            <a href="/XSession/FileList.aspx" target="_blank">FileList</a> &nbsp; &nbsp;
            <a href="/XSession/ShowDebug.aspx" target="_blank">ShowDebug</a>
        </p>
</body>
</html>
