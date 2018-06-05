///<reference path="typings/jquery/jquery.d.ts" />
///<reference path="typings/knockout/knockout.d.ts" />
var UserIdentity = /** @class */ (function () {
    function UserIdentity(_username, _id) {
        this.email = _username;
        this.id = _id;
        this.isValidated = false;
    }
    return UserIdentity;
}());
var MainApp = /** @class */ (function () {
    function MainApp() {
        this.user = null;
        this.url = "work";
    }
    MainApp.prototype.setUser = function (email) {
        $.getJSON(this.url, { email: email }, function (data, status, xhr) { });
    };
    return MainApp;
}());
//# sourceMappingURL=fp.js.map