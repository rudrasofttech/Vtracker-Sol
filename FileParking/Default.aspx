<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <meta content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=0' name='viewport' />
    <meta name="viewport" content="width=device-width" />
    <link href="css/bootstrap.min.css" rel="stylesheet" />
    <link href="css/bootstrap-responsive.min.css" rel="stylesheet" />
    <link href='http://fonts.googleapis.com/css?family=Montserrat:400,300,700' rel='stylesheet' type='text/css' />
    <link href="http://maxcdn.bootstrapcdn.com/font-awesome/latest/css/font-awesome.min.css" rel="stylesheet" />
    <link href="css/custom.css" rel="stylesheet" />
    <link href="css/bootstrap-image-gallery.min.css" rel="stylesheet" type="text/css" />
    <link href="css/jquery.fileupload-ui.css" rel="stylesheet" type="text/css" />
</head>
<body id="mainapp">
    <div class="container-fluid">
        <div class="row-fluid view1" id="loginfrm">
            <div class="span4 offset4">
                <div class="alert fade">
                </div>
                <div>
                    <span class="help-block">Provide your email address, to generate an <a href="#" data-toggle="tooltip" title="One Time Password">OTP</a></span>
                    <input type="email" placeholder="you@mail.com" id="emailtxt" />
                </div>
                <div id="otppanel" class="step2">
                    <input type="password" id="otptxt" />
                </div>
                <div>
                    <button type="button" id="generateOtpbtn" class="btn btn-primary step1" onclick="app.loginFrm.setUser()">Generate OTP</button>
                    <button type="button" id="signinbtn" class="btn btn-primary step2" onclick="app.loginFrm.validateUser()">Sign In</button>
                </div>
            </div>
        </div>
        <div class="row-fluid view1" id="uploadfrm">
            
        </div>
    </div>

    <script src="js/jquery-1.7.2.min.js"></script>
    <script src="js/site/bootstrap.js"></script>
    <script src="Scripts/fp.js"></script>
</body>
</html>
