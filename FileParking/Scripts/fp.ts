

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
    formDom : any;
    constructor() {
        this.url = "handlers/userhandler.ashx";
        this.emailTextBox = "emailtxt";
        this.otpTextBox = "otptxt";
        this.signInbtn = "signinbtn";
        this.formDom = $("#loginfrm");
    }

    bind() {

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
                Message.Display("OTP sent to your mail address.", "Info");
            }
        })
            .fail(function () {
                Message.Display("Something went wrong! Try again.", "error");
                console.log("Unable to create a user");
            });
        //.always(function () {
        //    console.log("complete");
        //});

    }

    validateUser() {
        if (user != null) {
            var email: string = $("#" + this.emailTextBox).val();
            var otp: string = $("#" + this.otpTextBox).val();
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
        } else {
            this.formDom.find(".step2").hide();
            this.formDom.find(".step1").show();
        }
    }
}

class Message {
    static Display(msg: string, type: string) {

        if ($("#loginfrm").is(':visible')) {
            $("#loginfrm .msg").removeClass("alert-error alert-success alert-info");
            if (type == "error") {
                $("#loginfrm .alert").addClass("alert-error");
            } else if (type == "success") {
                $("#loginfrm .alert").addClass("alert-success");
            } else if (type == "info") {
                $("#loginfrm .alert").addClass("alert-info");
            }
            $("#loginfrm .alert").html(msg).show().css("opacity","1"); 
            setTimeout(function () { $("#loginfrm .alert").html("").hide(); }, 5000);
        }

    }
}

class MainApp {
    url: string;
    loginFrm: LoginForm;

    constructor() {
        this.loginFrm = new LoginForm();
    }

    bind() {
        this.loginFrm.bind();
    }
}

var app: MainApp = new MainApp();
var user: UserIdentity = null;
app.bind();