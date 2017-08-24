define({
    load: function (name, req, onload) {
        'use strict';
        req(['angularAMD', name], function (angularAMD, value) {
            angularAMD.processQueue();
            onload(value);
        });
    }
});