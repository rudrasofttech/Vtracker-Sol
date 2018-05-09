var vt_guid = '';
var cookieName = "visittrackercurrent";
var oldCookieName = "visittrackerold";
var VTStorage = /** @class */ (function () {
    function VTStorage(cname, pcname) {
        this.name = cname;
        this.permanentName = pcname;
    }
    VTStorage.prototype.localStorageSupported = function () {
        try {
            return "localStorage" in window && window["localStorage"] !== null;
        }
        catch (e) {
            return false;
        }
    };
    VTStorage.prototype.setCookie = function (cName, value, mins) {
        var expires = "";
        if (mins > 0) {
            var date = new Date();
            date.setTime(date.getTime() + (mins * 60 * 1000));
            expires = "; expires=" + date.toUTCString();
            document.cookie = cName + "=" + (value || "") + expires + "; path=/";
        }
        else {
            document.cookie = cName + "=" + (value || "") + "; path=/";
        }
    };
    VTStorage.prototype.setVolatile = function (value) {
        if (this.localStorageSupported()) {
            sessionStorage.setItem(this.name, value);
        }
        else {
            this.setCookie(this.name, value, 0);
        }
    };
    VTStorage.prototype.setPermanent = function (value) {
        if (this.localStorageSupported()) {
            var date = new Date();
            localStorage.setItem(this.permanentName, value);
            localStorage.setItem(this.permanentName + "time", date.getTime().toString());
        }
        else {
        }
    };
    VTStorage.prototype.getCookie = function (cName) {
        var nameEQ = cName + "=";
        var ca = document.cookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ')
                c = c.substring(1, c.length);
            if (c.indexOf(nameEQ) == 0)
                return c.substring(nameEQ.length, c.length);
        }
        return null;
    };
    VTStorage.prototype.getVolatile = function () {
        if (this.localStorageSupported()) {
            return sessionStorage.getItem(this.name);
        }
        else {
            return this.getCookie(this.name);
        }
    };
    VTStorage.prototype.getPermanent = function () {
        if (this.localStorageSupported()) {
            if (localStorage[this.permanentName + "time"]) {
                var d = localStorage[this.permanentName + "time"];
                var n = Date.now();
                var elapsed = (n - d) / (1000 * 60);
                if (elapsed < 720) {
                    return localStorage.getItem(this.permanentName);
                }
                else {
                    this.erasePermanent();
                    return null;
                }
            }
        }
        else {
            return null;
        }
    };
    VTStorage.prototype.eraseCookie = function (cName) {
        document.cookie = cName + '=; Max-Age=-99999999;';
    };
    VTStorage.prototype.erasePermanent = function () {
        if (this.localStorageSupported()) {
            localStorage.removeItem(this.permanentName);
        }
        else {
        }
    };
    VTStorage.prototype.eraseVolatile = function () {
        if (this.localStorageSupported()) {
            sessionStorage.removeItem(this.name);
        }
        else {
            this.eraseCookie(this.name);
        }
    };
    return VTStorage;
}());
var VisitTracker = /** @class */ (function () {
    function VisitTracker() {
        this.tick = 0;
        this.d = new Date();
        this.xhr = new Image();
        this.url = vt_root + "rv/";
        this.cookie = new VTStorage(cookieName, oldCookieName);
    }
    VisitTracker.prototype.sendPulse = function () {
        var bwidth = -1;
        var bheight = -1;
        try {
            bwidth = window.innerWidth || document.body.clientWidth;
            bheight = window.innerHeight || document.body.clientHeight;
        }
        catch (err) { }
        var top = window.pageYOffset || document.documentElement.scrollTop, left = window.pageXOffset || document.documentElement.scrollLeft;
        var transimg = new Image();
        transimg.src = this.url + "index/" + this.tick + "?cc=" + this.getVisitID() + "&path=" + encodeURI(window.location.href) + "&wvri=" + vt_wvri + "&a=pulse&w=" + bwidth + "&h=" + bheight + "&st=" + top + "&sl=" + left;
    };
    VisitTracker.prototype.documentWideClick = function (x, y, tag, tagid) {
        var clickimg = new Image();
        try {
            clickimg.src = this.url + "rc/" + this.tick + "?cc=" + this.getVisitID() + "&path=" + encodeURI(window.location.href) + "&x=" + x + "&y=" + y + "&tag=" + tag + "&tagid=" + encodeURI(tagid);
            this.tick++;
        }
        catch (_a) { }
    };
    VisitTracker.prototype.firstRequest = function () {
        //get existing visit id
        var oldcc = this.getPermanentVisitID();
        //set new visit id
        this.setVisit();
        var referer = encodeURIComponent(document.referrer);
        var path = window.location.href;
        var browser = "";
        try {
            browser = this.getBrowserDetail();
        }
        catch (err) {
            browser = "";
        }
        var screenwidth = -1;
        var screenheight = -1;
        try {
            screenwidth = screen.width;
            screenheight = screen.height;
        }
        catch (err) { }
        var top = window.pageYOffset || document.documentElement.scrollTop, left = window.pageXOffset || document.documentElement.scrollLeft;
        this.xhr.src = this.url + "fr?wid=" + vt_website_id + "&referer=" + encodeURI(referer) + "&path=" + encodeURI(path) + "&wvri=" + vt_wvri + "&cc=" + this.getVisitID() + "&r=" + this.tick + "&b=" + browser + "&sw=" + screenwidth + "&sh=" + screenheight + "&st=" + top + "&sl=" + left + "&occ=" + oldcc;
    };
    VisitTracker.prototype.getVisitID = function () {
        var x = this.cookie.getVolatile();
        if (x) {
            return x;
        }
        return "";
    };
    VisitTracker.prototype.setVisit = function () {
        var x = this.cookie.getVolatile();
        if (x) {
        }
        else {
            this.cookie.setVolatile(vt_guid);
        }
    };
    VisitTracker.prototype.getPermanentVisitID = function () {
        var x = this.cookie.getPermanent();
        if (x) {
            return x;
        }
        return "";
    };
    VisitTracker.prototype.setPermanentVisit = function (value) {
        var x = this.cookie.getPermanent();
        if (x) {
        }
        else {
            //set permanent cookie to expire in 12 hours
            this.cookie.setPermanent(value);
        }
    };
    VisitTracker.prototype.getBrowserDetail = function () {
        var nVer = navigator.appVersion;
        var nAgt = navigator.userAgent;
        var browserName = navigator.appName;
        var fullVersion = '' + parseFloat(navigator.appVersion);
        var majorVersion = parseInt(navigator.appVersion, 10);
        var nameOffset, verOffset, ix;
        // In Opera 15+, the true version is after "OPR/" 
        if ((verOffset = nAgt.indexOf("OPR/")) != -1) {
            browserName = "Opera";
            fullVersion = nAgt.substring(verOffset + 4);
        }
        else if ((verOffset = nAgt.indexOf("Opera")) != -1) {
            browserName = "Opera";
            fullVersion = nAgt.substring(verOffset + 6);
            if ((verOffset = nAgt.indexOf("Version")) != -1)
                fullVersion = nAgt.substring(verOffset + 8);
        }
        else if ((verOffset = nAgt.indexOf("MSIE")) != -1) {
            browserName = "Microsoft Internet Explorer";
            fullVersion = nAgt.substring(verOffset + 5);
        }
        else if ((verOffset = nAgt.indexOf("Edge")) != -1) {
            browserName = "Edge";
            fullVersion = nAgt.substring(verOffset + 5);
        }
        else if ((verOffset = nAgt.indexOf("Chrome")) != -1) {
            browserName = "Chrome";
            fullVersion = nAgt.substring(verOffset + 7);
        }
        else if ((verOffset = nAgt.indexOf("Safari")) != -1) {
            browserName = "Safari";
            fullVersion = nAgt.substring(verOffset + 7);
            if ((verOffset = nAgt.indexOf("Version")) != -1)
                fullVersion = nAgt.substring(verOffset + 8);
        }
        else if ((verOffset = nAgt.indexOf("Firefox")) != -1) {
            browserName = "Firefox";
            fullVersion = nAgt.substring(verOffset + 8);
        }
        else if ((nameOffset = nAgt.lastIndexOf(' ') + 1) <
            (verOffset = nAgt.lastIndexOf('/'))) {
            browserName = nAgt.substring(nameOffset, verOffset);
            fullVersion = nAgt.substring(verOffset + 1);
            if (browserName.toLowerCase() == browserName.toUpperCase()) {
                browserName = navigator.appName;
            }
        }
        // trim the fullVersion string at semicolon/space if present
        if ((ix = fullVersion.indexOf(";")) != -1)
            fullVersion = fullVersion.substring(0, ix);
        if ((ix = fullVersion.indexOf(" ")) != -1)
            fullVersion = fullVersion.substring(0, ix);
        majorVersion = parseInt('' + fullVersion, 10);
        if (isNaN(majorVersion)) {
            fullVersion = '' + parseFloat(navigator.appVersion);
            majorVersion = parseInt(navigator.appVersion, 10);
        }
        return browserName + " " + fullVersion;
    };
    return VisitTracker;
}());
var vtobj = new VisitTracker();
vtobj.xhr.onload = function () {
    if (vtobj.getPermanentVisitID() == "") {
        vtobj.setPermanentVisit(vtobj.getVisitID());
    }
    //send pulse
    setInterval(function () {
        vtobj.tick++;
        vtobj.sendPulse();
    }, 5000);
    document.addEventListener("click", function (event) {
        var tag = "", tagid = "";
        if (event.target !== null) {
            tag = event.target.tagName;
            if (event.target.getAttribute("id")) {
                tagid = "#" + event.target.getAttribute("id");
            }
        }
        vtobj.documentWideClick(event.pageX, event.pageY, tag, tagid);
    });
};
vtobj.firstRequest();
//# sourceMappingURL=vt.js.map