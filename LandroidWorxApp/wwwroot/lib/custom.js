window.interop = {
    getElementByName: function (name) {
        var elements = document.getElementsByName(name);
        if (elements.length) {
            return elements[0].value;
        } else {
            return "";
        }
    },
    submitForm: function (path, fields) {
        const form = document.createElement('form');
        form.method = 'post';
        form.action = path;

        for (const key in fields) {
            if (fields.hasOwnProperty(key)) {
                const hiddenField = document.createElement('input');
                hiddenField.type = 'hidden';
                hiddenField.name = key;
                hiddenField.value = fields[key];
                form.appendChild(hiddenField);
            }
        }

        document.body.appendChild(form);
        form.submit();
    },
    renderPickers: function (className) {
        
        document.activeElement.blur();
        $("input").blur();
        var sUserAgent = navigator.userAgent,
            isMobile =
            {
                Android: sUserAgent.match(/Android|Silk/i),
                iOS: sUserAgent.match(/iPhone|iPad|iPod/i),
                Windows: sUserAgent.match(/IEMobile/i)
            };
        isMobile.other = !(isMobile.Android || isMobile.iOS || isMobile.Windows);

        if ($('.' + className).length > 0) {
            if (isMobile.other || isMobile.Windows)
               $('.' + className).AnyPicker({
                    onInit: function () {
                        apo = this;
                    },
                    onSetOutput: function () {
                        var event = new Event('change');
                        this.elem.dispatchEvent(event);
                    },
                    mode: 'datetime',
                    dateTimeFormat: 'HH:mm',
                    lang: 'it',
               });
            else if (isMobile.iOS)
                $('.' + className).AnyPicker({
                    onInit: function () {
                        apo = this;
                    },
                    onSetOutput: function () {
                        var event = new Event('change');
                        this.elem.dispatchEvent(event);
                    },
                    mode: 'datetime',
                    dateTimeFormat: 'HH:mm',
                    lang: 'it',
                    theme: 'iOS',
                    i18n:
                    {
                        headerTitle: "",
                        setButton: "Save"
                    }
                });
            else if (isMobile.Android)
                $('.' + className).AnyPicker({
                    onInit: function () {
                        apo = this;
                    },
                    onSetOutput: function () {
                        var event = new Event('change');
                        this.elem.dispatchEvent(event);
                    },
                    mode: 'datetime',
                    dateTimeFormat: 'HH:mm',
                    lang: 'it',
                    theme: 'Android'
                });
        }       
    },
    showSwallAlert: function (type, title, message, confirmBtnText, cancelBtnText, callback) {
        debugger
        swal({
            title: title,
            text: message,
            icon: type,
            buttons: {
                cancel: cancelBtnText != null ? cancelBtnText : false,
                confirm: confirmBtnText != null ? confirmBtnText : false,
            },
        }).then(callback);
    },
    showDeleteUserAlert: function (type, title, message, confirmBtnText, cancelBtnText) {
        
        swal({
            title: title,
            text: message,
            icon: type,
            buttons: {
                cancel: cancelBtnText != null ? cancelBtnText : false,
                confirm: confirmBtnText != null ? confirmBtnText : false,
            },
        }).then(function (data) {
            
            if (data) {
                location.href = "/DeleteUserData";
            }
        });
    },
    hideOverlay: function (callback) {
        if ($('#overlay')) {
            $('#overlay').hide(callback);
        };
    },
    showOverlay: function (callback) {
        if ($('#overlay')) {
            $('#overlay').show(callback);
        };
    },
    writeCookie: function (name, value, days) {
        debugger
        var expires;
        if (days) {
            var date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            expires = "; expires=" + date.toGMTString();
        }
        else {
            expires = "";
        }
        document.cookie = name + "=" + value + expires + "; path=/";
    },
    readCookie: function (cname) {
        var name = cname + "=";
        var decodedCookie = decodeURIComponent(document.cookie);
        var ca = decodedCookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ') {
                c = c.substring(1);
            }
            if (c.indexOf(name) == 0) {
                return c.substring(name.length, c.length);
            }
        }
        return null;
    },

};



