'use strict';
define(['angularAMD'], function (angularAMD) {
    var app = angular.module('app', ['ngAnimate', 'ngFileUpload', 'ngRoute', 'angularJsExtends', 'ngMessages'])
    .controller('settingCtrl', ['$scope', function ($scope) {
        this.onStart = function () {
            location.href = '/login';
        }
    }])
    ;
    return angularAMD.bootstrap(app);
});