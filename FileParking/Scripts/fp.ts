

class UserIdentity {
    //User Email
    email: string;
    //User Public ID
    id: string;
    //Is User Password Validated
    isValidated: boolean;
    token: string;

    constructor(_username: string, _id: string) {
        this.email = _username;
        this.id = _id;
        this.isValidated = false;
        this.token = "";
    }
}

class LoginForm {
    emailTextBox: string;
    generateOtpbtn: string;
    otpDiv: string;
    url: string;
    otpTextBox: string;
    signInbtn: string;
    formDom: any;
    mainappdom: any;
    constructor(_mainappdom: any) {
        this.url = "handlers/userhandler.ashx";
        this.emailTextBox = "emailtxt";
        this.otpTextBox = "otptxt";
        this.signInbtn = "signinbtn";
        this.formDom = $("#loginfrm");
        this.mainappdom = _mainappdom;
    }

    setUser() {
        var email: string = $("#" + this.emailTextBox).val();
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
    }

    validateUser() {
        if (user != null) {
            var email: string = $("#" + this.emailTextBox).val();
            var otp: string = $("#" + this.otpTextBox).val();
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
                    } else {
                        Message.Display("Incorrect OTP! Try again.", "error");
                    }
                })
                .fail(function () {
                    Message.Display("Something went wrong! Try again.", "error");
                });
        } else {
            this.formDom.find(".step2").hide();
            this.formDom.find(".step1").show();
        }
    }
}

class FileUploadForm {
    mainappdom: any;
    uploadfrmdom: any;

    constructor(_mainappdom: any) {
        this.uploadfrmdom = $("#uploadfrm");
        this.mainappdom = _mainappdom;
    }

    loadForm() {
        var instance = this;
        var jqxhr = $.get("simpleupload", { token: user.token }, function () {
            console.log("success");
        }).done(function (data) {
            instance.uploadfrmdom.html(data);
        }).fail(function () {
            Message.Display("Something went wrong! Try again.", "error");
        });
    }
}

class Message {
    static Display(msg: string, type: string) {

        if ($("#loginfrm").is(':visible')) {
            var context = $("#loginfrm");
            context.find(".alert").removeClass("alert-error alert-success alert-info");
            if (type == "error") {
                context.find(".alert").addClass("alert-error");
            } else if (type == "success") {
                context.find(".alert").addClass("alert-success");
            } else if (type == "info") {
                context.find(".alert").addClass("alert-info");
            }
            context.find(".alert").html(msg).show().css("opacity", "1");
            setTimeout(function () { context.find(".alert").html("").hide(); }, 5000);
        }
    }
}

class MainApp {
    url: string;
    loginFrm: LoginForm;
    uploadFrm: FileUploadForm;
    mainappdom: any;

    constructor() {
        this.mainappdom = $("#mainapp");
        this.uploadFrm = null;
        this.loginFrm = new LoginForm(this.mainappdom);
    }

    bind() {
        this.mainappdom.on("uservalidated", this.onUserValidated);
    }

    onUserValidated(event, _mainappdom) {
        this.uploadFrm = new FileUploadForm(_mainappdom);
        this.uploadFrm.loadForm();
    }
}

var app: MainApp = new MainApp();
var user: UserIdentity = null;
app.bind();