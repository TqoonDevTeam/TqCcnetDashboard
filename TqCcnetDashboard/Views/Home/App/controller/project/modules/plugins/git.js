define(['app'], function (app) {
    'use strict';
    app.controller('project.plugins.git.customctrl', ['$scope', 'pathUtil', function ($scope, pathUtil) {
        $scope.custom.attrs_force_show = ['branch', 'cleanUntrackedFiles', 'workingDirectory'];
        $scope.custom.attrs_force_required = ['branch', 'cleanUntrackedFiles', 'workingDirectory'];
        $scope.custom.template = {};
        $scope.custom.defaultValue = { maxAmountOfModificationsToFetch: 1 };
        $scope.custom.init = function () {}
    }]);
});