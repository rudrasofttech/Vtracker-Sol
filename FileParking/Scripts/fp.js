var app = null;
var user = null;
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
        Loader.Show();
        var jqxhr = $.getJSON(this.url, { email: email, a: "email" }, function (data) {
            console.log(data);
            if (data.success) {
                user = new UserIdentity(data.email, data.id);
                instance.formDom.find(".step2").show();
                instance.formDom.find(".step1").hide();
                Message.Display("OTP sent to your mail address.", "info");
            }
        }).fail(function () {
            Message.Display("Something went wrong! Try again.", "error");
        }).always(function () {
            Loader.Hide();
        });
    };
    LoginForm.prototype.validateUser = function () {
        if (user != null) {
            var email = $("#" + this.emailTextBox).val();
            var otp = $("#" + this.otpTextBox).val();
            var instance = this;
            Loader.Show();
            var jqxhr = $.getJSON(this.url, { email: email, otp: otp, a: "validate" }, function () {
                console.log("success");
            })
                .done(function (data) {
                if (data.success) {
                    user.isValidated = data.isValidated;
                    user.token = data.token;
                    instance.mainappdom.trigger("uservalidated", this.mainappdom);
                }
                else {
                    Message.Display("Incorrect OTP! Try again.", "error");
                }
            })
                .fail(function () {
                Message.Display("Something went wrong! Try again.", "error");
            }).always(function () {
                Loader.Hide();
            });
            ;
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
        }).fail(function (data) {
            if (data.status == 400) {
                Message.Display("Something went wrong! Try again.", "error");
            }
            else if (data.status == 401) {
                Message.Display("Unauthorized request! Login in again.", "error");
                app.mainappdom.trigger("authexpired");
            }
        });
    };
    return FileUploadForm;
}());
var Message = /** @class */ (function () {
    function Message() {
    }
    Message.Display = function (msg, type) {
        var context = $("#message");
        context.removeClass("alert-error alert-success alert-info");
        if (type == "error") {
            context.addClass("alert-error");
        }
        else if (type == "success") {
            context.addClass("alert-success");
        }
        else if (type == "info") {
            context.addClass("alert-info");
        }
        context.html(msg).show().css("opacity", "1");
        setTimeout(function () { context.html("").hide(); }, 5000);
    };
    return Message;
}());
var Loader = /** @class */ (function () {
    function Loader() {
    }
    Loader.Show = function () {
        var loader = $("#commonloading");
        loader.show();
    };
    Loader.Hide = function () {
        var loader = $("#commonloading");
        loader.hide();
    };
    return Loader;
}());
var MainApp = /** @class */ (function () {
    function MainApp() {
        this.mainappdom = $("#mainapp");
        this.uploadFrm = null;
        this.loginFrm = new LoginForm(this.mainappdom);
    }
    MainApp.prototype.bind = function () {
        this.mainappdom.on("uservalidated", this.onUserValidated);
        this.mainappdom.on("authexpired", this.onAuthExpired);
    };
    MainApp.prototype.onUserValidated = function (event, _mainappdom) {
        app.uploadFrm = new FileUploadForm(_mainappdom);
        app.uploadFrm.loadForm();
        app.viewshouldchange();
    };
    MainApp.prototype.onAuthExpired = function (event) {
        console.log(event);
        user = null;
        app.viewshouldchange();
    };
    MainApp.prototype.viewshouldchange = function () {
        if (user == null) {
            this.mainappdom.find(".view1").show();
            this.mainappdom.find(".view2").hide();
        }
        else {
            this.mainappdom.find(".view1").hide();
            this.mainappdom.find(".view2").show();
        }
    };
    return MainApp;
}());
app = new MainApp();
user = null;
app.bind();
user = new UserIdentity('raj@gmail.com', '11310605-0FAA-467F-A5AC-211BB2BD0EA2');
user.isValidated = true;
user.token = '7D33061F-5BB3-480B-8E45-5E345143DF29';
app.mainappdom.trigger("uservalidated", app.mainappdom);
//# sourceMappingURL=fp.js.map