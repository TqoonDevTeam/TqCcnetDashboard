define(['app'], function (app) {
    'use strict';
    app.controller('project.plugins.TqIIS.customctrl', ['$scope', function ($scope) {
        $scope.custom.attrs_force_show = ['siteConfig', 'virtualDirectories'];
        $scope.custom.attrs_force_required = [];
        $scope.custom.template = {};
        $scope.custom.defaultValue = {
            poolConfig: 'Enable32BitAppOnWin64=false\nManagedRuntimeVersion=v4.0\nManagedPipelineMode=Integrated\nAutoStart=true\nFailure.OrphanWorkerProcess=false\nProcessModel.MaxProcesses=1\nProcessModel.LoadUserProfile=true\nProcessModel.IdentityType=ApplicationPoolIdentity'
        }
        $scope.custom.init = function () { }
    }]);
});