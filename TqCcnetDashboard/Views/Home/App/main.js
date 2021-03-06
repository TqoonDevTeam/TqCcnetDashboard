﻿'use strict';
requirejs.config({
    paths: {
        'jquery': '/Scripts/jquery-3.1.1.min',
        'signalr.core': '/Scripts/jquery.signalR-2.2.2.min',
        'signalr.hubs': '/signalr/hubs?',
        'angular': '/Scripts/angular.min',
        'angular-animate': '/Scripts/angular-animate.min',
        'angular-aria': '/Scripts/angular-aria.min',
        'angular-route': '/Scripts/angular-route.min',
        'angular-cookies': '/Scripts/angular-cookies.min',
        'angular-touch': '/Scripts/angular-touch.min',
        'angular-message': '/Scripts/angular-messages.min',
        'angular-sanitize': '/Scripts/angular-sanitize.min',
        'ng-file-upload': '/Scripts/ng-file-upload-all.min',
        'ui-select': '/Scripts/select.min',
        'ui-grid': '/Scripts/ui-grid.min',
        'ui-bootstrap': '/Scripts/angular-ui/ui-bootstrap.min',
        'ui-bootstrap-tpls': '/Scripts/angular-ui/ui-bootstrap-tpls.min',
        'bootstrap': '/Scripts/bootstrap.min',
        'underscore': '/Scripts/underscore.min',
        'defaultExtends': '/Scripts/WebCore/defaultExtends',
        'angularJsExtends': '/Scripts/WebCore/angularJsExtends',
        'md5': '/Scripts/md5',
        'angularAMD': '/Scripts/angularAMD.min',
        'urijs': '/Scripts/urljs/src',
        // utils
        'utils': '/Views/Home/App/utils/utils',
        'utils-fileSelector': '/Views/Home/App/utils/fileSelector',
        // requirejs-plugins
        'text': '/Scripts/text',
        'async': '/Scripts/requirejs-plugins/async',
        'font': '/Scripts/requirejs-plugins/font',
        'goog': '/Scripts/requirejs-plugins/goog',
        'image': '/Scripts/requirejs-plugins/image',
        'json': '/Scripts/requirejs-plugins/json',
        'noext': '/Scripts/requirejs-plugins/noext',
        'mdown': '/Scripts/requirejs-plugins/mdown',
        'propertyParser': '/Scripts/requirejs-plugins/propertyParser'
    },
    shim: {
        'metisMenu': ['jquery'],
        "signalr.core": {
            deps: ["jquery"],
            exports: "$.connection"
        },
        "signalr.hubs": {
            deps: ["signalr.core"],
        },
        'angular': {
            deps: ['jquery'],
            exports: 'angular'
        },
        'angular-animate': ['angular'],
        'angular-aria': ['angular'],
        'angular-route': ['angular'],
        'angular-cookies': ['angular'],
        'angular-touch': ['angular'],
        'angular-message': ['angular'],
        'angular-sanitize': ['angular'],
        'bootstrap': ['jquery'],
        'ng-file-upload': ['angular'],
        'ui-select': ['angular'],
        'ui-grid': ['angular'],
        'ui-bootstrap': ['angular'],
        'ui-bootstrap-tpls': ['angular', 'ui-bootstrap'],
        'angularJsExtends': ['angular', 'underscore'],
        'utils': ['angular'],
        'angularAMD': ['underscore',
            'defaultExtends',
            'signalr.hubs',
            'angular',
            'angular-animate',
            'angular-aria',
            'angular-route',
            'angular-cookies',
            'angular-touch',
            'angular-message',
            'angular-sanitize',
            'bootstrap',
            'ui-select',
            'ui-bootstrap',
            'ui-bootstrap-tpls',
            'ng-file-upload',
            'ui-grid',
            'utils',
            'angularJsExtends']
    },
    deps: ['app']
});