<%@ Page Language="C#" AutoEventWireup="true" CodeFile="simpleupload.aspx.cs" Inherits="simpleupload" %>
<%@ Register TagPrefix="jQuery" TagName="MultipleFileUpload" Src="control/MultipleFileUpload.ascx" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <meta content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=0' name='viewport' />
    <meta name="viewport" content="width=device-width" />
    <link href="css/bootstrap.min.css" rel="stylesheet" type="text/css" />
    <!-- Le HTML5 shim, for IE6-8 support of HTML5 elements -->
    <!--[if lt IE 9]>
      <script src="http://html5shim.googlecode.com/svn/trunk/html5.js"></script>
    <![endif]-->
    <link href="css/bootstrap-responsive.min.css" rel="stylesheet" type="text/css" />
    <script src="http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.7.2.min.js" type="text/javascript"></script>
    <script src="js/site/bootstrap.min.js" type="text/javascript"></script>
    <link href="css/custom.css" rel="stylesheet" type="text/css" />
    <link href="css/bootstrap-image-gallery.min.css"
        rel="stylesheet" type="text/css" />
    <link href="css/jquery.fileupload-ui.css"
        rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
    <jQuery:MultipleFileUpload ID="MultipleFileUpload" runat="server" AcceptFileTypes=".*"
        SequentialUploads="false" EnableChunkedUploads="true" MaxChunkSize="15000000"
        Resume="true" AutoRetry="true" RetryTimeout="500" MaxRetries="100" OnFileUploadDone="fileuploaddone"
        LimitConcurrentUploads="2" ForceIframeTransport="false" AutoUpload="false" PreviewAsCanvas="true" />
    <script type="text/javascript">
        function fileuploaddone(e, data) {
            return false;
        }
    </script>
    </form>
</body>
</html>
