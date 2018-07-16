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
    <link href="css/multiple-emails.css" rel="stylesheet" />
  
</head>
<body id="mainapp" class="curls1">
    <div class="container">
        <div class="row-fluid navbar-fixed-top">
            <div class="span4">
            </div>
            <div class="span4">
                <h1 class="sitename">File Transfer</h1>
            </div>
            <div class="span4" style="text-align: right; padding: 20px 10px;">
                <!--<a href="#planModal" role="button" data-toggle="modal" class="btn btn-info" data-bind="visible: shouldshowplans">Plans</a>-->
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
                <div data-spy="affix" data-offset-top="0" style="display: inline-block">
                    <div class="card">
                        <div>
                            <label class="control-label" for="toemailtxt">To</label>
                            <input type="text" id="toemailtxt" placeholder="Emails" />
                        </div>
                        <div>
                            <label class="control-label" for="messagetxt">Message</label>
                            <input type="text" id="messagetxt" />
                        </div>
                        <button type="button" class="btn" data-bind="click: shareFiles"><i class="fa fa-paper-plane-o" aria-hidden="true"></i> Send</button>
                    </div>
                    <!-- ko if: activeplan() != null-->
                    <div class="card" style="margin-top: 30px;">
                        <h4 class="bold"><span data-bind="text: activeplan().name"></span> User</h4>
                        <h5><span class="bold" data-bind="text: activeplan().limit"></span> Files Limit</h5>
                        <h6><span class="bold" data-bind="text: activeplan().filesize"></span> per File Limit</h6>
                        <div data-bind="visible: shouldshowplans">
                            Need more storage <a href="#planModal" role="button" data-toggle="modal" class="btn btn-info">Go Pro</a>
                        </div>
                    </div>
                    <!-- /ko -->

                </div>

            </div>
            <div class="span8 drive">
                <div class="card ">
                    <div id="uploadfrm">
                    </div>
                    <div id="filelist">
                        <!-- ko if: files().length > 0-->
                        <table class="table table-hover table-condensed">
                            <tr>
                                <th><input type="checkbox" class="filechkheader" onchange="$('.filechk').prop('checked', $(this).is(':checked'))" /></th>
                                <th>Name</th>
                                <th>Size</th>
                                <th>Create Date</th>
                                <th>Expiry Date</th>
                                <th></th>
                            </tr>
                            <tbody data-bind="foreach: files">
                                <tr onclick="$(this).find('.filechk').prop('checked', !($(this).find('.filechk').is(':checked')))">
                                    <td>
                                        <input type="checkbox" class="filechk" data-bind="attr: {value: name}" onclick="fileCHKChecked(event)" />
                                    </td>
                                    <td data-bind="text: name"></td>
                                    <td data-bind="text: size"></td>
                                    <td data-bind="text: created"></td>
                                    <td data-bind="text: expiry"></td>
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
                    <div class="span12">
                        <h4>Pro</h4>
                        <ul>
                            <li>30 Files</li>
                            <li>4 Gb per File</li>
                            <li>Parked for 365 days</li>
                            <li>Upto 120 GB Storage</li>
                        </ul>

                        <script src="https://www.paypalobjects.com/api/checkout.js"></script>
                        <div id="paypal-button">
                            <script>
                                (function () {
                                    paypal.Button.render({
                                        env: 'sandbox', // Or 'production',
                                        client: {
                                            sandbox: 'rajkiran.singh-facilitator@rudrasofttech.com',
                                            production: 'rajkiran.singh@rudrasofttech.com'
                                        }, style: {
                                            size: 'small',
                                            color: 'gold',
                                            shape: 'pill',
                                        },
                                        payment: function (data, actions) {
                                            return actions.payment.create({
                                                transactions: [{
                                                    amount: {
                                                        total: '<%: ProPlan.Price %>',
                                                        currency: 'USD'
                                                    }
                                                }]
                                            });
                                        },
                                        onAuthorize: function (data, actions) {
                                            return actions.redirect();
                                        },
                                        onCancel: function (data, actions) {
                                            return actions.redirect();
                                        },
                                        onError: function (error) {
                                            // You will want to handle this differently
                                            return alert(error);
                                        }
                                    }, '#paypal-button');
                                })();
                            </script>
                        </div>

                    </div>
                </div>
            </div>
        </div>
    </div>
    <script src="js/jquery-1.7.2.min.js"></script>
    <script src="js/multiple-emails.js"></script>
    <script src="js/site/bootstrap.js"></script>
    <script src="Scripts/knockout-3.4.2.js"></script>
    <script src="Scripts/fp.js"></script>
    <script>
        $(document).ready(function () {
            $('#toemailtxt').multiple_emails();
        });

        function fileCHKChecked(event) {
            event.stopPropagation();
        }
    </script>
</body>
</html>
