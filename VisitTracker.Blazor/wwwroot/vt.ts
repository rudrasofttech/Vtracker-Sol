let vt_guid = '';
let cookieName = "visittrackercurrent";
let oldCookieName = "visittrackerold";

class VTStorage {
    name: string;
    permanentName: string;


    constructor(cname: string, pcname: string) {
        this.name = cname;
        this.permanentName = pcname;
    }

    localStorageSupported(): boolean {
        try {
            return "localStorage" in window && window["localStorage"] !== null;
        } catch (e) {
            return false;
        }
    }

    setCookie(cName: string, value: string, mins: number) {
        let expires = "";
        if (mins > 0) {
            var date = new Date();
            date.setTime(date.getTime() + (mins * 60 * 1000));
            expires = "; expires=" + date.toUTCString();
            document.cookie = cName + "=" + (value || "") + expires + "; path=/";
        } else {
            document.cookie = cName + "=" + (value || "") + "; path=/";
        }

    }

    setVolatile(value: string) {
        if (this.localStorageSupported()) {
            sessionStorage.setItem(this.name, value);
        } else {
            this.setCookie(this.name, value, 0);
        }
    }

    setPermanent(value: string) {
        if (this.localStorageSupported()) {
            var date = new Date();
            localStorage.setItem(this.permanentName, value);
            localStorage.setItem(this.permanentName + "time", date.getTime().toString());
        } else {

        }

    }

    getCookie(cName: string) {
        var nameEQ = cName + "=";
        var ca = document.cookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ') c = c.substring(1, c.length);
            if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
        }
        return null;
    }

    getVolatile() {
        if (this.localStorageSupported()) {
            return sessionStorage.getItem(this.name);
        } else {
            return this.getCookie(this.name);
        }
    }

    getPermanent() {
        if (this.localStorageSupported()) {
            if (localStorage[this.permanentName + "time"]) {
                var d = localStorage[this.permanentName + "time"];
                var n = Date.now();
                var elapsed = (n - d) / (1000 * 60);
                if (elapsed < 720) {
                    return localStorage.getItem(this.permanentName);
                } else {
                    this.erasePermanent();
                    return null;
                }
            }


        } else {
            return null;
        }
    }

    eraseCookie(cName: string) {
        document.cookie = cName + '=; Max-Age=-99999999;';
    }

    erasePermanent() {
        if (this.localStorageSupported()) {
            localStorage.removeItem(this.permanentName);
        } else {

        }

    }

    eraseVolatile() {
        if (this.localStorageSupported()) {
            sessionStorage.removeItem(this.name);
        } else {
            this.eraseCookie(this.name);
        }
    }
}

class VisitTracker {
    xhr: any;
    url: string;
    cookie: VTStorage;
    d: Date;
    tick: number;
    mousewheelevt: string;
    sbnoted: boolean;
    //windowFocused: boolean;
    focusedSeconds: number;

    constructor() {
        this.sbnoted = false;
        //this.windowFocused = true;
        this.focusedSeconds = 0;
        this.tick = 0;
        this.d = new Date();
        this.xhr = new Image();
        this.url = vt_root + "rv/";
        this.mousewheelevt = (/Firefox/i.test(navigator.userAgent)) ? "DOMMouseScroll" : "mousewheel";
        this.cookie = new VTStorage(cookieName, oldCookieName);
    }

    sendPulse() {
        var bwidth = -1;
        var bheight = -1;
        try {
            bwidth = window.innerWidth || document.body.clientWidth;
            bheight = window.innerHeight || document.body.clientHeight;
        } catch (err) { }

        var top = window.pageYOffset || document.documentElement.scrollTop,
            left = window.pageXOffset || document.documentElement.scrollLeft;


        var transimg = new Image();
        transimg.src = this.url + "index/" + this.tick + "?cc=" + this.getVisitID() + "&path=" + encodeURI(window.location.href) + "&wvri=" + vt_wvri + "&a=pulse&w=" + bwidth + "&h=" + bheight + "&st=" + top + "&sl=" + left;

    }

    documentWideClick(x, y, tag, tagid) {
        var clickimg = new Image();
        try {
            clickimg.src = this.url + "rc/" + this.tick + "?cc=" + this.getVisitID() + "&path=" + encodeURI(window.location.href) + "&x=" + x + "&y=" + y + "&tag=" + tag + "&tagid=" + encodeURI(tagid);
            this.tick++;
        } catch{ }
    }

    windowBlur() {
        //this.windowFocused = false;
        var wbimg = new Image();
        try {
            wbimg.src = this.url + "wf/" + this.tick + "?cc=" + this.getVisitID() + "&path=" + encodeURI(window.location.href) + "&s=" + this.focusedSeconds;
            this.focusedSeconds = 0;
            this.tick++;
        }
        catch{ }
    }

    windowFocus() {
        //this.windowFocused = true;
        var wbimg = new Image();
        try {
            wbimg.src = this.url + "wb/" + this.tick + "?cc=" + this.getVisitID() + "&path=" + encodeURI(window.location.href) + "&s=" + this.focusedSeconds;
            this.focusedSeconds = 0;
            this.tick++;
        }
        catch{ }
    }

    firstRequest() {
        //get existing visit id
        var oldcc = this.getPermanentVisitID();

        //set new visit id
        this.setVisit();

        var referer = encodeURIComponent(document.referrer);
        var path = window.location.href;
        var browser = "";
        try {
            browser = this.getBrowserDetail();
        } catch (err) {
            browser = "";
        }
        var screenwidth = -1;
        var screenheight = -1;
        try {
            screenwidth = screen.width;
            screenheight = screen.height;
        } catch (err) { }

        var top = window.pageYOffset || document.documentElement.scrollTop,
            left = window.pageXOffset || document.documentElement.scrollLeft;

        this.xhr.src = this.url + "fr?wid=" + vt_website_id + "&referer=" + encodeURI(referer) + "&path=" + encodeURI(path) + "&wvri=" + vt_wvri + "&cc=" + this.getVisitID() + "&r=" + this.tick + "&b=" + browser + "&sw=" + screenwidth + "&sh=" + screenheight + "&st=" + top + "&sl=" + left + "&occ=" + oldcc;
    }

    getVisitID() {
        var x = this.cookie.getVolatile();
        if (x) {
            return x;
        }

        return "";
    }

    setVisit() {
        var x = this.cookie.getVolatile();
        if (x) {

        } else {
            this.cookie.setVolatile(vt_guid);
        }
    }

    getPermanentVisitID() {
        var x = this.cookie.getPermanent();
        if (x) {
            return x;
        }

        return "";
    }

    setPermanentVisit(value: string) {
        var x = this.cookie.getPermanent();
        if (x) {

        } else {
            //set permanent cookie to expire in 12 hours
            this.cookie.setPermanent(value);
        }
    }

    scroller(evt) {
        //Guess the delta.
        var delta = 0;
        if (!evt) evt = window.event;
        if (evt.wheelDelta) {
            delta = evt.wheelDelta / 120;
        } else if (evt.detail) {
            delta = -evt.detail / 3;
        }

        var pageHeight = document.documentElement.offsetHeight,
            windowHeight = window.innerHeight,
            scrollPosition = window.scrollY || window.pageYOffset || document.body.scrollTop + (document.documentElement && document.documentElement.scrollTop || 0);

        if ((pageHeight - 150) <= (windowHeight + scrollPosition)  && !this.sbnoted) {
            var sbimg = new Image();
            try {
                sbimg.src = this.url + "sb/" + this.tick + "?cc=" + this.getVisitID() + "&path=" + encodeURI(window.location.href);
                this.tick++;
                this.sbnoted = true;
            }
            catch{ }
        } else if ((pageHeight - 150) > (windowHeight + scrollPosition)) {
            this.sbnoted = false;
        }

        
        //var body = document.body,
        //    html = document.documentElement;

        //var height = Math.max(body.scrollHeight, body.offsetHeight,
        //    html.clientHeight, html.scrollHeight, html.offsetHeight);
        //var scrollTop = window.pageYOffset || document.documentElement.scrollTop;
        //if (scrollTop >= (height - 100) && !this.sbnoted) {
        //    var sbimg = new Image();
        //    try {
        //        sbimg.src = this.url + "sb/" + this.tick + "?cc=" + this.getVisitID() + "&path=" + encodeURI(window.location.href);
        //        this.tick++;
        //        this.sbnoted = true;
        //    }
        //    catch{ }
        //} else if (scrollTop < (height - 150)) {
        //    this.sbnoted = false;
        //}
    }

    getBrowserDetail() {
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
        // In older Opera, the true version is after "Opera" or after "Version"
        else if ((verOffset = nAgt.indexOf("Opera")) != -1) {
            browserName = "Opera";
            fullVersion = nAgt.substring(verOffset + 6);
            if ((verOffset = nAgt.indexOf("Version")) != -1)
                fullVersion = nAgt.substring(verOffset + 8);
        }
        // In MSIE, the true version is after "MSIE" in userAgent
        else if ((verOffset = nAgt.indexOf("MSIE")) != -1) {
            browserName = "Microsoft Internet Explorer";
            fullVersion = nAgt.substring(verOffset + 5);
        }
        // In Edge, the true version is after "MSIE" in userAgent
        else if ((verOffset = nAgt.indexOf("Edge")) != -1) {
            browserName = "Edge";
            fullVersion = nAgt.substring(verOffset + 5);
        }
        // In Chrome, the true version is after "Chrome" 
        else if ((verOffset = nAgt.indexOf("Chrome")) != -1) {
            browserName = "Chrome";
            fullVersion = nAgt.substring(verOffset + 7);
        }
        // In Safari, the true version is after "Safari" or after "Version" 
        else if ((verOffset = nAgt.indexOf("Safari")) != -1) {
            browserName = "Safari";
            fullVersion = nAgt.substring(verOffset + 7);
            if ((verOffset = nAgt.indexOf("Version")) != -1)
                fullVersion = nAgt.substring(verOffset + 8);
        }
        // In Firefox, the true version is after "Firefox" 
        else if ((verOffset = nAgt.indexOf("Firefox")) != -1) {
            browserName = "Firefox";
            fullVersion = nAgt.substring(verOffset + 8);
        }
        // In most other browsers, "name/version" is at the end of userAgent 
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
    }
}

let vtobj = new VisitTracker();
vtobj.xhr.onload = function () {
    if (vtobj.getPermanentVisitID() == "") {
        vtobj.setPermanentVisit(vtobj.getVisitID());
    }
    //send pulse
    setInterval(function () {
        vtobj.tick++;
        
        vtobj.sendPulse();
    }, 5000);
    setInterval(function () { vtobj.focusedSeconds += 1; }, 1000);
    document.addEventListener("click", function (event) {
        var tag = "", tagid = "";
        if (event.target !== null) {
            tag = (event.target as HTMLElement).tagName;
            if ((event.target as HTMLElement).getAttribute("id")) {
                tagid = "#" + (event.target as HTMLElement).getAttribute("id");
            }
        }
        vtobj.documentWideClick(event.pageX, event.pageY, tag, tagid);
    });

    window.addEventListener('blur', function (e) {
        if (document.activeElement == document.querySelector('iframe')) {

        } else {
            vtobj.windowBlur();
        }
    });

    window.addEventListener('focus', function (e) {
        vtobj.windowFocus();
    });

    if (document.addEventListener) {
        document.addEventListener(vtobj.mousewheelevt, function (e) { vtobj.scroller(e) }, false);
    }
};

vtobj.firstRequest();

