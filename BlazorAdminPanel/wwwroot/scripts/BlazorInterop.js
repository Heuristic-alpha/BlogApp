window.BlazorExtensions = {
    SetCookie: function (name, value, hours) {
        const d = new Date();
        d.setTime(d.getTime() + hours * 60 * 60 * 1000);
        const expires = "expires=" + d.toUTCString();
        document.cookie = name + "=" + encodeURIComponent(value) + "; " + expires + "; path=/";
    },

    GetCookie: function (name) {
        const decodedCookie = decodeURIComponent(document.cookie);
        const ca = decodedCookie.split(';');
        for (let i = 0; i < ca.length; i++) {
            let c = ca[i].trim();
            if (c.indexOf(name + "=") == 0) {
                return c.substring(name.length + 1, c.length);
            }
        }
        return "";
    }
};