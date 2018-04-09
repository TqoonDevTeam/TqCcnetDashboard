define(['app'], function (app) {
    'use strict';
    app.controller('project.plugins.TqText.customctrl', ['$scope', function ($scope) {
        $scope.custom.attrs_force_show = ['saveEncoding', 'saveCondition'];
        $scope.custom.attrs_force_required = [];
        $scope.custom.template = {};
        $scope.custom.defaultValue = {
            saveEncoding: 'UTF-8', saveCondition: 'IfChanged'
        }
        $scope.custom.init = function () { }
    }]);
});