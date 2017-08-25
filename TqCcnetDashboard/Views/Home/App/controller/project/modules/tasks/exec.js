define(['app'], function (app) {
    'use strict';
    app.controller('project.step4.tasks.exec.customctrl', ['$scope', function ($scope) {
        $scope.custom.attrs_force_show = ['buildArgs', 'buildTimeoutSeconds', 'successExitCodes', 'baseDirectory'];
        $scope.custom.attrs_force_required = ['buildArgs', 'buildTimeoutSeconds', 'successExitCodes', 'baseDirectory'];
        $scope.custom.defaultValue = {
            buildTimeoutSeconds: 120
        }
        $scope.custom.init = function () {}
    }]);
});