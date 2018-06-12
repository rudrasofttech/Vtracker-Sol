var UserIdentity = /** @class */ (function () {
    function UserIdentity(_username, _id) {
        this.email = _username;
        this.id = _id;
        this.isValidated = false;
        this.token = "";
    }
    return UserIdentity;
}());
var LoginForm = /** @class */ (function () {
    function LoginForm(_mainappdom) {
        this.url = "handlers/userhandler.ashx";
        this.emailTextBox = "emailtxt";
        this.otpTextBox = "otptxt";
        this.signInbtn = "signinbtn";
        this.formDom = $("#loginfrm");
        this.mainappdom = _mainappdom;
    }
    LoginForm.prototype.setUser = function () {
        var email = $("#" + this.emailTextBox).val();
        var instance = this;
        var jqxhr = $.getJSON(this.url, { email: email, a: "email" }, function (data) {
            console.log(data);
            if (data.success) {
                user = new UserIdentity(data.email, data.id);
                instance.formDom.find(".step2").show();
                instance.formDom.find(".step1").hide();
                Message.Display("OTP sent to your mail address.", "info");
            }
        })
            .fail(function () {
                Message.Display("Something went wrong! Try again.", "error");
            });
    };
    LoginForm.prototype.validateUser = function () {
        if (user != null) {
            var email = $("#" + this.emailTextBox).val();
            var otp = $("#" + this.otpTextBox).val();
            var instance = this;
            var jqxhr = $.getJSON(this.url, { email: email, otp: otp, a: "validate" }, function () {
                console.log("success");
            })
                .done(function (data) {
                    if (data.success) {
                        user.isValidated = data.isValidated;
                        user.token = data.token;
                        instance.mainappdom.trigger("uservalidated", this.mainappdom);
                        instance.formDom.hide();
                    }
                    else {
                        Message.Display("Incorrect OTP! Try again.", "error");
                    }
                })
                .fail(function () {
                    Message.Display("Something went wrong! Try again.", "error");
                });
        }
        else {
            this.formDom.find(".step2").hide();
            this.formDom.find(".step1").show();
        }
    };
    return LoginForm;
}());
var FileUploadForm = /** @class */ (function () {
    function FileUploadForm(_mainappdom) {
        this.uploadfrmdom = $("#uploadfrm");
        this.mainappdom = _mainappdom;
    }
    FileUploadForm.prototype.loadForm = function () {
        var instance = this;
        var jqxhr = $.get("simpleupload", { token: user.token }, function () {
            console.log("success");
        }).done(function (data) {
            instance.uploadfrmdom.html(data);
        }).fail(function () {
            Message.Display("Something went wrong! Try again.", "error");
        });
    };
    return FileUploadForm;
}());
var Message = /** @class */ (function () {
    function Message() {
    }
    Message.Display = function (msg, type) {
        if ($("#loginfrm").is(':visible')) {
            var context = $("#loginfrm");
            context.find(".alert").removeClass("alert-error alert-success alert-info");
            if (type == "error") {
                context.find(".alert").addClass("alert-error");
            }
            else if (type == "success") {
                context.find(".alert").addClass("alert-success");
            }
            else if (type == "info") {
                context.find(".alert").addClass("alert-info");
            }
            context.find(".alert").html(msg).show().css("opacity", "1");
            setTimeout(function () { context.find(".alert").html("").hide(); }, 5000);
        }
    };
    return Message;
}());
var MainApp = /** @class */ (function () {
    function MainApp() {
        this.mainappdom = $("#mainapp");
        this.uploadFrm = null;
        this.loginFrm = new LoginForm(this.mainappdom);
    }
    MainApp.prototype.bind = function () {
        this.mainappdom.on("uservalidated", this.onUserValidated);
    };
    MainApp.prototype.onUserValidated = function (event, _mainappdom) {
        this.uploadFrm = new FileUploadForm(_mainappdom);
        this.uploadFrm.loadForm();
    };
    return MainApp;
}());
var app = new MainApp();
var user = null;
app.bind();
