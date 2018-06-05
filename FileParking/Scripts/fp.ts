///<reference path="typings/jquery/jquery.d.ts" />
///<reference path="typings/knockout/knockout.d.ts" />

class UserIdentity {
    //User Email
    email: string;
    //User Public ID
    id: string;
    //Is User Password Validated
    isValidated: boolean;

    constructor(_username: string, _id:string) {
        this.email = _username;
        this.id = _id;
        this.isValidated = false;
    }
}

class MainApp {
    user: UserIdentity;
    url: string;
    constructor() {
        this.user = null;
        this.url = "work";
    }

    setUser(email: string) {
        $.getJSON(this.url, { email: email }, function (data, status, xhr) { });
    }
}