<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <meta content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=0' name='viewport' />
    <meta name="viewport" content="width=device-width" />
    <link href="css/custom.css" rel="stylesheet" />
    <link href="css/bootstrap.min.css" rel="stylesheet" />
    <link href="css/bootstrap-responsive.min.css" rel="stylesheet" />
    <link href='http://fonts.googleapis.com/css?family=Montserrat:400,300,700' rel='stylesheet' type='text/css' />
    <link href="http://maxcdn.bootstrapcdn.com/font-awesome/latest/css/font-awesome.min.css" rel="stylesheet" />

    <link href="css/bootstrap-image-gallery.min.css" rel="stylesheet" type="text/css" />
    <link href="css/jquery.fileupload-ui.css" rel="stylesheet" type="text/css" />
</head>
<body id="mainapp">
    <div class="container-fluid">
        <div class="view1" id="loginfrm">

            <div>
                <label for="emailtxt">Email</label>
                <input type="email" placeholder="your@mail.com" class="input" id="emailtxt" />
                <span class="help-block">You would need an <a href="#" data-toggle="tooltip" title="One Time Password">OTP</a></span>
            </div>
            <div id="otppanel" class="step2">
                <input type="password" class="input" id="otptxt" />
            </div>
            <div>
                <button type="button" id="generateOtpbtn" class="btn btn-primary step1" onclick="app.loginFrm.setUser()">Generate OTP</button>
                <button type="button" id="signinbtn" class="btn btn-primary step2" onclick="app.loginFrm.validateUser()">Sign In</button>
            </div>
        </div>
        <div class="row-fluid view2">
            <div class="span5" id="uploadfrm"></div>
            <div class="span7" id="filelist"></div>
        </div>
    </div>
    <div class="fixed-bottom">
        <div class="alert fade" id="message">
        </div>
        <div class="progress progress-striped active" id="commonloading" style="">
            <div class="bar" style="width: 100%;"></div>
        </div>
    </div>

    <script src="js/jquery-1.7.2.min.js"></script>
    <script src="js/site/bootstrap.js"></script>
    <script src="Scripts/fp.js"></script>
</body>
</html>
