var vt_root = "//localhost:53501/";
var vt_website_id = "{websiteid}";
var vt_wvri = "";
var VTInit = /** @class */ (function () {
    function VTInit() {
    }
    VTInit.prototype.initialize = function () {
        var seed = document.createElement("script");
        seed.setAttribute("src", vt_root + "rv/getjs/" + vt_website_id);
        if (document.getElementsByTagName("head").length > 0) {
            document.getElementsByTagName("head")[0].appendChild(seed);
        }
    };
    return VTInit;
}());
var _vtInit = new VTInit();
_vtInit.initialize();
//# sourceMappingURL=vtinit.js.map