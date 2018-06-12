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
    function LoginForm() {
        this.url = "handlers/userhandler.ashx";
        this.emailTextBox = "emailtxt";
        this.otpTextBox = "otptxt";
        this.signInbtn = "signinbtn";
        this.formDom = $("#loginfrm");
    }
    LoginForm.prototype.bind = function () {
    };
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
                console.log("Unable to create a user");
            });
        //.always(function () {
        //    console.log("complete");
        //});
    };
    LoginForm.prototype.validateUser = function () {
        if (user != null) {
            var email = $("#" + this.emailTextBox).val();
            var otp = $("#" + this.otpTextBox).val();
            var jqxhr = $.getJSON(this.url, { email: email, otp: otp, a: "validate" }, function () {
                console.log("success");
            })
                .done(function (data) {
                    if (data.success) {
                        user.isValidated = data.isValidated;
                        user.token = data.token;
                    }
                })
                .fail(function () {
                    console.log("Unable to validate the user");
                });
        }
        else {
            this.formDom.find(".step2").hide();
            this.formDom.find(".step1").show();
        }
    };
    return LoginForm;
}());
var Message = /** @class */ (function () {
    function Message() {
    }
    Message.Display = function (msg, type) {
        if ($("#loginfrm").is(':visible')) {
            $("#loginfrm .msg").removeClass("alert-error alert-success alert-info");
            if (type == "error") {
                $("#loginfrm .alert").addClass("alert-error");
            }
            else if (type == "success") {
                $("#loginfrm .alert").addClass("alert-success");
            }
            else if (type == "info") {
                $("#loginfrm .alert").addClass("alert-info");
            }
            $("#loginfrm .alert").html(msg).show();
            setTimeout(function () { $("#loginfrm .alert").html("").hide(); }, 5000);
        }
    };
    return Message;
}());
var MainApp = /** @class */ (function () {
    function MainApp() {
        this.loginFrm = new LoginForm();
    }
    MainApp.prototype.bind = function () {
        this.loginFrm.bind();
    };
    return MainApp;
}());
var app = new MainApp();
var user = null;
app.bind();
