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
                $("#memberidhdn").val(data.id);
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
                    localStorage.setItem("token", user.token);
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
        this.shouldshowlogout = ko.observable(false);
        this.activeplan = ko.observable(null);
        this.remaininglimit = ko.observable(null);
        this.remainingLimitDisplay = ko.observable(null);
    }
    MainApp.prototype.removeCanceledFile = function (f) {
        var instance = this;
        Loader.Show();
        var jqxhr = $.getJSON("handlers/filehandler.ashx", { a: "remove", token: user.token, f: f }, function () {
        }).done(function (data) {
            if (data.success) {
                console.log("Cancelled file removed successfully.");
            }
        }).fail(function () {
        }).always(function () {
            Loader.Hide();
        });
    };
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
                    app.remainingLimitDisplay(data.remaining + " Remaining");
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
    MainApp.prototype.downloadFileLink = function (d) {
        var instance = this;
        Loader.Show();
        var jqxhr = $.getJSON("handlers/filehandler.ashx", { a: "link", token: user.token, f: d.name }, function () {
        }).done(function (data) {
            if (data.success) {
                $("#dlink").html(data.dl);
                $("#dlink").attr("href", data.dl);
                $('#downloadModal').modal('show');
            }
            else {
                Message.Display("Could not generate download link.", "error");
            }
        }).fail(function () {
            Message.Display("Something went wrong! Try again.", "error");
        }).always(function () {
            Loader.Hide();
        });
    };
    MainApp.prototype.logout = function () {
        if ($("body").data("isworking") == "true") {
            if (!confirm("File Upload is in progress, this action will stop it, do you wish to continue?")) {
                return;
            }
        }
        localStorage.clear();
        location.reload();
    };
    MainApp.prototype.shareFiles = function () {
        if ($(".filechk:checked").length == 0) {
            Message.Display("Please choose a file! Try again.", "error");
            return;
        }
        if ($('#toemailtxt').val().trim() == "") {
            Message.Display("Provide an email.", "error");
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
                instance.remainingLimitDisplay(data.remaining + " Remaining");
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
                instance.remainingLimitDisplay(data.remaining + " Remaining");
                if (data.remaining <= 0) {
                    Message.Display("File limit reached, you cannot upload any more files.", "error");
                }
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
        $("#loggedInLabel").html(user.email);
        app.uploadFrm = new FileUploadForm(_mainappdom);
        app.uploadFrm.loadForm();
        app.shouldshowlogout(true);
        app.viewshouldchange();
        app.loadActivePlan();
        app.loadRemainingLimit();
        app.mainappdom.trigger("loadfiles");
    };
    MainApp.prototype.onAuthExpired = function (event) {
        user = null;
        app.shouldshowlogout(false);
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
ko.applyBindings(app);
if (localStorage.getItem('token')) {
    $.getJSON('handlers/userhandler.ashx', { a: 'validatetoken', token: localStorage.getItem('token') }, function () {
        console.log("success");
    })
        .done(function (data) {
        if (data.success) {
            user = new UserIdentity(data.email, data.id);
            user.isValidated = data.isValidated;
            user.token = data.token;
            localStorage.setItem("token", user.token);
            app.mainappdom.trigger("uservalidated", app.mainappdom);
        }
        else {
            //ask for login
            app.mainappdom.trigger("authexpired");
        }
    })
        .fail(function () {
        //ask for login
        app.mainappdom.trigger("authexpired");
    }).always(function () {
        Loader.Hide();
    });
}
else {
    app.mainappdom.trigger("authexpired");
}
window.onbeforeunload = function (e) {
    if ($("body").data("isworking") == "true") {
        return 'File Upload is in progress, this action will stop it, do you wish to continue?';
    }
};
//# sourceMappingURL=fp.js.map