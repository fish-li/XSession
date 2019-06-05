<%@ Page Language="C#" AutoEventWireup="true" EnableSessionState="True" CodeBehind="Default.aspx.cs" Inherits="WebApplication1.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div style="height: 200px; width:99%">
            <iframe src="WebForm1.aspx"></iframe>
        </div>
        <div style="height: 200px; width:99%">
            <iframe src="WebForm2.aspx"></iframe>
        </div>

        <p>
            <a href="/Default.aspx">Refresh</a> &nbsp; &nbsp;
            <a href="/WriteSession.aspx" target="_blank">Write Session</a> &nbsp; &nbsp;
            <a href="/DeleteSession.aspx" target="_blank">Delete Session</a>
        </p>
        <p>
            <a href="/XSession/SessionList.aspx" target="_blank">SessionList</a> &nbsp; &nbsp;
            <a href="/XSession/FileList.aspx" target="_blank">FileList</a> &nbsp; &nbsp;
            <a href="/XSession/ShowDebug.aspx" target="_blank">ShowDebug</a>
        </p>
    </form>        

</body>
</html>
