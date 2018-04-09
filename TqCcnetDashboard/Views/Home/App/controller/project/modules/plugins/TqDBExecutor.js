define(['app'], function (app) {
    'use strict';
    app.controller('project.plugins.TqDBExecutor.customctrl', ['$scope', function ($scope) {
        $scope.custom.attrs_force_show = ['siteConfig', 'virtualDirectories'];
        $scope.custom.attrs_force_required = [];
        $scope.custom.template = {};
        $scope.custom.defaultValue = {}
        $scope.custom.init = function () { }
    }]);
});