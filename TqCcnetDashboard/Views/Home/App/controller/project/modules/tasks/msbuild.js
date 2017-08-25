define(['app'], function (app) {
    'use strict';
    app.controller('project.step4.tasks.msbuild.customctrl', ['$scope', function ($scope) {
        $scope.custom.attrs_force_show = ['projectFile', 'targets', 'buildArgs'];
        $scope.custom.attrs_force_required = ['projectFile', 'targets', 'buildArgs'];
        $scope.custom.defaultValue = {
            executable: 'msbuild.exe'
        }
        $scope.custom.init = function () {}
    }]);
});