define(['app'], function (app) {
    'use strict';
    app.controller('project.plugins.TqRsync.customctrl', ['$scope', function ($scope) {
        $scope.custom.attrs_force_show = ['workingDirectory', 'timeout'];
        $scope.custom.attrs_force_required = [];
        $scope.custom.template = {};
        $scope.custom.defaultValue = {
            options: '-avrzP --chmod=ugo=rwX'
        }
        $scope.custom.init = function () { }
    }]);
});