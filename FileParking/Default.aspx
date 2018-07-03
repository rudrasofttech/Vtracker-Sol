<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <meta content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=0' name='viewport' />
    <meta name="viewport" content="width=device-width" />

    <link href="css/bootstrap.min.css" rel="stylesheet" />
    <link href="css/bootstrap-responsive.min.css" rel="stylesheet" />
    <link href="css/custom.css" rel="stylesheet" />
    <link href='http://fonts.googleapis.com/css?family=Montserrat:400,300,700' rel='stylesheet' type='text/css' />
    <link href="http://maxcdn.bootstrapcdn.com/font-awesome/latest/css/font-awesome.min.css" rel="stylesheet" />

    <link href="css/bootstrap-image-gallery.min.css" rel="stylesheet" type="text/css" />
    <link href="css/jquery.fileupload-ui.css" rel="stylesheet" type="text/css" />
</head>
<body id="mainapp" class="bg1">
    <div class="container">
        <div class="row-fluid navbar-fixed-top">
            <div class="span4">
            </div>
            <div class="span4">
                <h1 class="sitename">File Transfer</h1>
            </div>
            <div class="span4" style="text-align: right; padding: 20px 10px;">
                <a href="#planModal" role="button" data-toggle="modal" class="btn btn-info" data-bind="visible: shouldshowplans">Plans</a>
            </div>
        </div>
        <div class="view1 card" id="loginfrm">
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
            <div class="span4 sendform">
                <div class="card">
                    <div>
                        <label class="control-label" for="toemailtxt">To</label>
                        <input type="text" id="toemailtxt" placeholder="Emails" />
                    </div>
                    <div>
                        <label class="control-label" for="messagetxt">Message</label>
                        <input type="text" id="messagetxt" />
                    </div>


                    <button type="button" class="btn"><i class="fa fa-paper-plane-o" aria-hidden="true"></i>Send</button>

                </div>

            </div>
            <div class="span8 drive">
                <div class="card ">
                    <div id="uploadfrm">
                    </div>
                    <div id="filelist">

                        <!-- ko if: activeplan() != null-->
                        <p>
                            You are currently using our <span data-bind="text: activeplan().name"></span>plan. You can park <span data-bind="text: activeplan().limit"></span>files (<span data-bind="text: activeplan().filesize"></span>) for 
                        up to <span data-bind="text: activeplan().term"></span>days.
                        </p>
                        <!-- /ko -->
                        <!-- ko if: files().length > 0-->
                        <table class="table table-hover table-condensed">
                            <tr>
                                <th>Name</th>
                                <th>Size</th>
                                <th>Create Date</th>
                                <th>Modified Date</th>
                                <th></th>
                            </tr>
                            <tbody data-bind="foreach: files">
                                <tr>
                                    <td data-bind="text: name"></td>
                                    <td data-bind="text: size"></td>
                                    <td data-bind="text: created"></td>
                                    <td data-bind="text: modified"></td>
                                    <td>
                                        <button class="btn btn-link" data-bind="click: $parent.removeFile"><i class="icon-remove"></i></button>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                        <!-- /ko -->
                        ​<!-- ko if: files().length == 0-->
                        <div class="empty">
                            <p>You don't have any files preserved.</p>
                        </div>
                        <!-- /ko -->
                    </div>
                </div>

            </div>
        </div>
    </div>
    <div class="fixed-bottom">
        <div class="alert fade" id="message">
        </div>
        <div class="progress progress-striped active" id="commonloading" style="">
            <div class="bar" style="width: 100%;"></div>
        </div>
    </div>
    <div id="planModal" class="modal hide fade" tabindex="-1" role="dialog" aria-labelledby="planModalLabel" aria-hidden="true">
        <div class="modal-header">
            <button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>
            <h3 id="planModalLabel">Our Plans</h3>
        </div>
        <div class="modal-body">
            <div class="container-fluid">
                <div class="row-fluid">
                    <div class="span6">
                        <h4>Free</h4>
                        <ul>
                            <li>10 Files</li>
                            <li>2 Gb per File</li>
                            <li>Parked for 2 days</li>
                            <li>Upto 20 GB</li>
                        </ul>
                        <a href="#" class="btn btn-default">I Choose Free</a>
                    </div>
                    <div class="span6">
                        <h4>Pro</h4>
                        <ul>
                            <li>30 Files</li>
                            <li>4 Gb per File</li>
                            <li>Parked for 365 days</li>
                            <li>Upto 120 GB Storage</li>
                        </ul>
                        <a href="#" class="btn btn-primary">I Prefer Pro</a>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <script src="js/jquery-1.7.2.min.js"></script>
    <script src="js/site/bootstrap.js"></script>
    <script src="Scripts/knockout-3.4.2.js"></script>
    <script src="Scripts/fp.js"></script>
</body>
</html>
