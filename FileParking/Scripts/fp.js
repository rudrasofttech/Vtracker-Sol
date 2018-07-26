///<reference path="typings/jquery/jquery.d.ts" />
///<reference path="typings/knockout/knockout.d.ts" />
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
var Plan = /** @class */ (function () {
    function Plan(_id, _name, _term, _price, _limit) {
        this.id = ko.observable(_id);
        this.name = ko.observable(_name);
        this.term = ko.observable(_term);
        this.price = ko.observable(_price);
        this.limit = ko.observable(_limit);
    }
    return Plan;
}());
var MemberPlan = /** @class */ (function () {
    function MemberPlan(_name, _term, _limit, _created, _expiry, _filesize) {
        this.name = ko.observable(_name);
        this.limit = ko.observable(_limit);
        this.created = ko.observable(_created);
        this.expiry = ko.observable(_expiry);
        this.term = ko.observable(_term);
        this.filesize = ko.observable(_filesize);
    }
    return MemberPlan;
}());
var ParkedFile = /** @class */ (function () {
    function ParkedFile(_name, _size, _created, _expiry) {
        this.name = ko.observable(_name);
        this.size = ko.observable(_size);
        this.created = ko.observable(_created);
        this.expiry = ko.observable(_expiry);
    }
    return ParkedFile;
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
        this.files = ko.observableArray([]);
        this.shouldshowplans = ko.observable(true);
        this.activeplan = ko.observable(null);
        this.remaininglimit = ko.observable(null);
    }
    MainApp.prototype.removeFile = function (d) {
        if (confirm("Are you sure you want to delete this file?")) {
            var instance = this;
            Loader.Show();
            var jqxhr = $.getJSON("handlers/filehandler.ashx", { a: "remove", token: user.token, f: d.name }, function () {
            }).done(function (data) {
                if (data.success) {
                    app.files.remove(function (f) {
                        return f.name() == data.name;
                    });
                    app.remaininglimit(data.remaining);
                }
                else {
                    Message.Display("Could fetch files", "error");
                }
            }).fail(function () {
                Message.Display("Something went wrong! Try again.", "error");
            }).always(function () {
                Loader.Hide();
            });
        }
    };
    MainApp.prototype.shareFiles = function () {
        if ($(".filechk:checked").length == 0) {
            alert("Please choose a file");
            return;
        }
        if ($('#toemailtxt').val() == "") {
            alert("Provide an email");
            $('.multiple_emails-input').focus();
            return;
        }
        var filenames = "";
        $(".filechk:checked").each(function () {
            filenames += "," + $(this).val();
        });
        var instance = this;
        Loader.Show();
        var jqxhr = $.getJSON("handlers/filehandler.ashx", { a: "share", token: user.token, emails: $('#toemailtxt').val(), f: filenames }, function () {
        }).done(function (data) {
            if (data.success) {
                Message.Display(data.message, "success");
            }
            else {
                Message.Display("Could not fetch files", "error");
            }
        }).fail(function () {
            Message.Display("Something went wrong! Try again.", "error");
        }).always(function () {
            Loader.Hide();
        });
    };
    MainApp.prototype.bind = function () {
        this.mainappdom.on("uservalidated", this.onUserValidated);
        this.mainappdom.on("authexpired", this.onAuthExpired);
        this.mainappdom.on("loadfiles", this.onLoadFiles);
    };
    MainApp.prototype.loadPlans = function () {
        var instance = this;
        Loader.Show();
        var jqxhr = $.getJSON("handlers/mplanhandler.ashx", { a: "plans" }, function () {
        }).done(function (data) {
            if (data.success) {
                for (var k in data.plans) {
                    var p = data.plans[k];
                    instance.plans.push(new Plan(p.Id, p.Name, p.Term, p.Number, p.Limit));
                }
            }
            else {
                Message.Display("Could not fetch Plans", "error");
            }
        }).fail(function () {
            Message.Display("Something went wrong! Try again.", "error");
        }).always(function () {
            Loader.Hide();
        });
    };
    MainApp.prototype.loadList = function () {
        var instance = this;
        Loader.Show();
        var jqxhr = $.getJSON("handlers/filehandler.ashx", { a: "list", token: user.token }, function () {
        }).done(function (data) {
            if (data.success) {
                instance.remaininglimit(data.remaining);
                instance.files.removeAll();
                for (var k in data.files) {
                    var f = data.files[k];
                    instance.files.push(new ParkedFile(f.Name, f.Size, f.CreateDateDisplay, f.ExpiryDateDisplay));
                }
                $(".filechk").click(function (event) {
                    event.stopPropagation();
                });
            }
            else {
                Message.Display("Could not fetch files", "error");
            }
        }).fail(function () {
            Message.Display("Something went wrong! Try again.", "error");
        }).always(function () {
            Loader.Hide();
        });
    };
    MainApp.prototype.loadActivePlan = function () {
        var instance = this;
        Loader.Show();
        var jqxhr = $.getJSON("handlers/mplanhandler.ashx", { a: "active", token: user.token }, function () {
        }).done(function (data) {
            if (data.success) {
                if (data.plan) {
                    instance.activeplan(new MemberPlan(data.plan.Name, data.plan.Term, data.plan.Limit, data.plan.DateCreated, data.plan.ExpiryDate, data.plan.FileSize));
                }
            }
            else {
                Message.Display("Could not fetch active plan", "error");
            }
        }).fail(function () {
            Message.Display("Something went wrong! Try again.", "error");
        }).always(function () {
            Loader.Hide();
        });
    };
    MainApp.prototype.loadRemainingLimit = function () {
        var instance = this;
        Loader.Show();
        var jqxhr = $.getJSON("handlers/filehandler.ashx", { a: "checklimit", token: user.token }, function () {
        }).done(function (data) {
            if (data.success) {
                instance.remaininglimit(data.remaining);
            }
            else {
                Message.Display("Could not fetch remaining limit", "error");
            }
        }).fail(function () {
            Message.Display("Something went wrong! Try again.", "error");
        }).always(function () {
            Loader.Hide();
        });
    };
    MainApp.prototype.onLoadFiles = function (event) {
        app.loadList();
        app.loadRemainingLimit();
    };
    MainApp.prototype.onUserValidated = function (event, _mainappdom) {
        app.uploadFrm = new FileUploadForm(_mainappdom);
        app.uploadFrm.loadForm();
        app.viewshouldchange();
        app.loadActivePlan();
        app.loadRemainingLimit();
        app.mainappdom.trigger("loadfiles");
    };
    MainApp.prototype.onAuthExpired = function (event) {
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
user = new UserIdentity('raj@gmail.com', '11310605-0FAA-467F-A5AC-211BB2BD0EA2');
user.isValidated = true;
user.token = 'AFE77E11-61D5-4566-9CAC-4B821BF30D6F';
app.bind();
ko.applyBindings(app);
app.mainappdom.trigger("uservalidated", app.mainappdom);
//app.mainappdom.trigger("loadfiles"); 
//# sourceMappingURL=fp.js.map