let vt_guid = '';
let cookieName = "visittrackercurrent";
let oldCookieName = "visittrackerold";
let visitorIdStorageKey = "visittrackervisitorid";

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
                var d = Number(localStorage[this.permanentName + "time"]);
                var n = Date.now();
                var elapsed = (n - d) / (1000 * 60);
                if (elapsed < 720) {
                    return localStorage.getItem(this.permanentName);
                } else {
                    this.erasePermanent();
                    return null;
                }
            }
        }
        return null;
    }

    setVisitorId(value: string) {
        if (this.localStorageSupported()) {
            localStorage.setItem(visitorIdStorageKey, value);
        } else {
            this.setCookie(visitorIdStorageKey, value, 60 * 24 * 365 * 10);
        }
    }

    getVisitorId() {
        if (this.localStorageSupported()) {
            return localStorage.getItem(visitorIdStorageKey);
        } else {
            return this.getCookie(visitorIdStorageKey);
        }
    }

    eraseCookie(cName: string) {
        document.cookie = cName + '=; Max-Age=-99999999; path=/';
    }

    erasePermanent() {
        if (this.localStorageSupported()) {
            localStorage.removeItem(this.permanentName);
            localStorage.removeItem(this.permanentName + "time");
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
    sbnoted: boolean;
    focusedSeconds: number;
    pulseTimer: any;
    focusTimer: any;
    scrollThrottleTimer: any;
    maxScrollPct: number;
    docHeight: number;

    constructor() {
        this.sbnoted = false;
        this.focusedSeconds = 0;
        this.tick = 0;
        this.d = new Date();
        this.xhr = new Image();
        this.url = vt_root + "rv/";
        this.cookie = new VTStorage(cookieName, oldCookieName);
        this.pulseTimer = null;
        this.focusTimer = null;
        this.scrollThrottleTimer = null;
        this.maxScrollPct = 0;
        this.docHeight = 0;
    }

    createVisitorId() {
        try {
            if (window.crypto && "randomUUID" in window.crypto) {
                return window.crypto.randomUUID();
            }
        } catch (err) { }
        return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0;
            var v = c == "x" ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    getOrCreateVisitorId() {
        var vid = this.cookie.getVisitorId();
        if (vid) {
            return vid;
        }
        vid = this.createVisitorId();
        this.cookie.setVisitorId(vid);
        return vid;
    }

    getDocumentHeight() {
        var body = document.body;
        var html = document.documentElement;
        return Math.max(
            body ? body.scrollHeight : 0,
            body ? body.offsetHeight : 0,
            html ? html.clientHeight : 0,
            html ? html.scrollHeight : 0,
            html ? html.offsetHeight : 0
        );
    }

    updateScrollMetrics() {
        var top = window.pageYOffset || document.documentElement.scrollTop || 0;
        var viewportHeight = window.innerHeight || document.documentElement.clientHeight || 0;
        var docHeight = this.getDocumentHeight();
        this.docHeight = docHeight;

        if (docHeight > 0) {
            var pct = Math.round(((top + viewportHeight) / docHeight) * 100);
            if (pct < 0) pct = 0;
            if (pct > 100) pct = 100;
            if (pct > this.maxScrollPct) {
                this.maxScrollPct = pct;
            }
        }

        if ((docHeight - 150) <= (viewportHeight + top) && !this.sbnoted) {
            var sbimg = new Image();
            try {
                sbimg.src = this.url + "sb/" + this.tick
                    + "?cc=" + encodeURIComponent(this.getVisitID())
                    + "&path=" + encodeURIComponent(window.location.href);
                this.tick++;
                this.sbnoted = true;
            } catch { }
        } else if ((docHeight - 150) > (viewportHeight + top)) {
            this.sbnoted = false;
        }
    }

    onScroll() {
        var _this = this;
        if (this.scrollThrottleTimer) {
            return;
        }
        this.scrollThrottleTimer = setTimeout(function () {
            _this.scrollThrottleTimer = null;
            _this.updateScrollMetrics();
        }, 250);
    }

    sendPulse() {
        var bwidth = -1;
        var bheight = -1;
        try {
            bwidth = window.innerWidth || document.body.clientWidth;
            bheight = window.innerHeight || document.body.clientHeight;
        } catch (err) { }

        var top = window.pageYOffset || document.documentElement.scrollTop || 0;
        var left = window.pageXOffset || document.documentElement.scrollLeft || 0;
        this.updateScrollMetrics();

        var transimg = new Image();
        transimg.src = this.url + "index/" + this.tick
            + "?cc=" + encodeURIComponent(this.getVisitID())
            + "&path=" + encodeURIComponent(window.location.href)
            + "&wvri=" + encodeURIComponent(vt_wvri)
            + "&a=" + encodeURIComponent("pulse")
            + "&w=" + bwidth
            + "&h=" + bheight
            + "&st=" + top
            + "&sl=" + left
            + "&ms=" + this.maxScrollPct
            + "&dh=" + this.docHeight;
    }

    startPulse() {
        var _this = this;
        if (this.pulseTimer != null) {
            return;
        }
        this.pulseTimer = setInterval(function () {
            _this.tick++;
            _this.sendPulse();
        }, 15000);
    }

    stopPulse() {
        if (this.pulseTimer != null) {
            clearInterval(this.pulseTimer);
            this.pulseTimer = null;
        }
    }

    documentWideClick(x, y, tag, tagid, href, txt) {
        var clickimg = new Image();
        try {
            clickimg.src = this.url + "rc/" + this.tick
                + "?cc=" + encodeURIComponent(this.getVisitID())
                + "&path=" + encodeURIComponent(window.location.href)
                + "&x=" + x
                + "&y=" + y
                + "&tag=" + encodeURIComponent(tag || "")
                + "&tagid=" + encodeURIComponent(tagid || "")
                + "&href=" + encodeURIComponent(href || "")
                + "&txt=" + encodeURIComponent(txt || "");
            this.tick++;
        } catch { }
    }

    windowBlur() {
        var wbimg = new Image();
        try {
            wbimg.src = this.url + "wb/" + this.tick
                + "?cc=" + encodeURIComponent(this.getVisitID())
                + "&path=" + encodeURIComponent(window.location.href)
                + "&s=" + this.focusedSeconds;
            this.focusedSeconds = 0;
            this.tick++;
        } catch { }
    }

    windowFocus() {
        var wfimg = new Image();
        try {
            wfimg.src = this.url + "wf/" + this.tick
                + "?cc=" + encodeURIComponent(this.getVisitID())
                + "&path=" + encodeURIComponent(window.location.href)
                + "&s=" + this.focusedSeconds;
            this.focusedSeconds = 0;
            this.tick++;
        } catch { }
    }

    firstRequest() {
        var oldcc = this.getPermanentVisitID();
        this.setVisit();

        var referer = document.referrer || "";
        var path = window.location.href;
        var browser = "";
        try {
            browser = this.getBrowserDetail();
        } catch (err) {
            browser = "";
        }

        var visitorId = this.getOrCreateVisitorId();

        var screenwidth = -1;
        var screenheight = -1;
        try {
            screenwidth = screen.width;
            screenheight = screen.height;
        } catch (err) { }

        var top = window.pageYOffset || document.documentElement.scrollTop || 0;
        var left = window.pageXOffset || document.documentElement.scrollLeft || 0;

        this.xhr.src = this.url + "fr?wid=" + encodeURIComponent(vt_website_id)
            + "&referer=" + encodeURIComponent(referer)
            + "&path=" + encodeURIComponent(path)
            + "&wvri=" + encodeURIComponent(vt_wvri)
            + "&cc=" + encodeURIComponent(this.getVisitID())
            + "&r=" + this.tick
            + "&b=" + encodeURIComponent(browser)
            + "&sw=" + screenwidth
            + "&sh=" + screenheight
            + "&st=" + top
            + "&sl=" + left
            + "&occ=" + encodeURIComponent(oldcc)
            + "&vid=" + encodeURIComponent(visitorId);
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
        if (!x) {
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
        if (!x) {
            this.cookie.setPermanent(value);
        }
    }

    getBrowserDetail() {
        var ua = navigator.userAgent || "";

        if (ua.indexOf("HeadlessChrome") >= 0) return "Headless Chrome";
        if (ua.indexOf("Edg/") >= 0) return "Edge";
        if (ua.indexOf("OPR/") >= 0) return "Opera";
        if (ua.indexOf("SamsungBrowser") >= 0) return "Samsung Internet";
        if (ua.indexOf("CriOS") >= 0) return "Chrome";
        if (ua.indexOf("FxiOS") >= 0) return "Firefox";
        if (ua.indexOf("Chrome") >= 0) return "Chrome";
        if (ua.indexOf("Safari") >= 0) return "Safari";
        if (ua.indexOf("Firefox") >= 0) return "Firefox";

        return "";
    }

    findClosestAnchor(target: HTMLElement | null): HTMLAnchorElement | null {
        var node = target;
        while (node) {
            if (node.tagName && node.tagName.toLowerCase() === "a") {
                return node as HTMLAnchorElement;
            }
            node = node.parentElement;
        }
        return null;
    }
}

let vtobj = new VisitTracker();
vtobj.xhr.onload = function () {
    if (vtobj.getPermanentVisitID() == "") {
        vtobj.setPermanentVisit(vtobj.getVisitID());
    }

    vtobj.updateScrollMetrics();
    if (document.visibilityState !== "hidden") {
        vtobj.startPulse();
    }

    if (vtobj.focusTimer == null) {
        vtobj.focusTimer = setInterval(function () { vtobj.focusedSeconds += 1; }, 1000);
    }

    document.addEventListener("click", function (event) {
        var tag = "";
        var tagid = "";
        var href = "";
        var txt = "";

        var target = event.target as HTMLElement | null;
        var anchor = vtobj.findClosestAnchor(target);

        if (anchor != null) {
            tag = anchor.tagName || "A";
            if (anchor.getAttribute("id")) {
                tagid = "#" + anchor.getAttribute("id");
            }

            href = (anchor.getAttribute("href") || anchor.href || "").trim();
            if (href.length > 500) {
                href = href.substring(0, 500);
            }

            txt = ((anchor.innerText || anchor.textContent || "") as string).trim();
            if (txt.length > 100) {
                txt = txt.substring(0, 100);
            }
        } else if (target !== null) {
            tag = target.tagName;
            if (target.getAttribute("id")) {
                tagid = "#" + target.getAttribute("id");
            }
        }

        vtobj.documentWideClick(event.pageX, event.pageY, tag, tagid, href, txt);
    });

    window.addEventListener("blur", function () {
        if (document.activeElement == document.querySelector("iframe")) {
            return;
        }
        vtobj.windowBlur();
    });

    window.addEventListener("focus", function () {
        vtobj.windowFocus();
    });

    document.addEventListener("visibilitychange", function () {
        if (document.visibilityState === "hidden") {
            vtobj.stopPulse();
        } else {
            vtobj.startPulse();
        }
    });

    window.addEventListener("pagehide", function () {
        vtobj.tick++;
        vtobj.sendPulse();
    });

    window.addEventListener("scroll", function () {
        vtobj.onScroll();
    }, { passive: true });
};

vtobj.firstRequest();

