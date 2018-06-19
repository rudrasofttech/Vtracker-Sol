var app:MainApp = null;
var user: UserIdentity = null;

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
    }

    validateUser() {
        if (user != null) {
            var email: string = $("#" + this.emailTextBox).val();
            var otp: string = $("#" + this.otpTextBox).val();
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
                        
                    } else {
                        Message.Display("Incorrect OTP! Try again.", "error");
                    }
                })
                .fail(function () {
                    Message.Display("Something went wrong! Try again.", "error");
                }).always(function () {
                    Loader.Hide();
                });;
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
            }).fail(function (data) {
                if (data.status == 400) {
                    Message.Display("Something went wrong! Try again.", "error");
                } else if (data.status == 401) {
                    Message.Display("Unauthorized request! Login in again.", "error");
                    app.mainappdom.trigger("authexpired");
                }
        });
    }
}

class Message {
    static Display(msg: string, type: string) {
        var context = $("#message");
        context.removeClass("alert-error alert-success alert-info");
        if (type == "error") {
            context.addClass("alert-error");
        } else if (type == "success") {
            context.addClass("alert-success");
        } else if (type == "info") {
            context.addClass("alert-info");
        }
        context.html(msg).show().css("opacity", "1");
        setTimeout(function () { context.html("").hide(); }, 5000);
    }
}

class Loader {
    static Show() {
        var loader = $("#commonloading");
        loader.show();
    }

    static Hide() {
        var loader = $("#commonloading");
        loader.hide();
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
        this.mainappdom.on("authexpired", this.onAuthExpired);
    }

    onUserValidated(event, _mainappdom) {
        app.uploadFrm = new FileUploadForm(_mainappdom);
        app.uploadFrm.loadForm();
        app.viewshouldchange();
    }

    onAuthExpired(event) {
        console.log(event);
        user = null;
        app.viewshouldchange();
    }

    viewshouldchange() {
        if (user == null) {
            this.mainappdom.find(".view1").show();
            this.mainappdom.find(".view2").hide();
        } else {
            this.mainappdom.find(".view1").hide();
            this.mainappdom.find(".view2").show();
        }
    }
}

app = new MainApp();
user = null;
app.bind();

user = new UserIdentity('raj@gmail.com', '11310605-0FAA-467F-A5AC-211BB2BD0EA2');
user.isValidated = true;
user.token = '7D33061F-5BB3-480B-8E45-5E345143DF29';
app.mainappdom.trigger("uservalidated", app.mainappdom);