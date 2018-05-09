let vt_root : string = "//localhost:53501/";
let vt_website_id : string = "{websiteid}";
let vt_wvri : string = "";
class VTInit {
    initialize() {
        let seed = document.createElement("script");
        seed.setAttribute("src", vt_root + "rv/getjs/" + vt_website_id);
        if (document.getElementsByTagName("head").length > 0) {
            document.getElementsByTagName("head")[0].appendChild(seed);
        }
    }
}
let _vtInit = new VTInit();
_vtInit.initialize();