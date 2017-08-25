define(['app'], function (app) {
    'use strict';
    app.controller('project.step4.tasks.TqIIS.customctrl', ['$scope', function ($scope) {
        $scope.custom.attrs_force_show = ['siteConfig', 'virtualDirectories'];
        $scope.custom.attrs_force_required = [];
        $scope.custom.template = {
            'siteConfig': [
                '<div class="col-sm-10">',
                    '<textarea class="form-control" name="siteConfig" ng-model="task.siteConfig" style="min-height:150px" ng-required="item.attr.Required"></textarea>',
                '</div>'].join(''),
            'poolConfig': [
                '<div class="col-sm-10">',
                    '<textarea class="form-control" name="poolConfig" ng-model="task.poolConfig" style="min-height:150px" ng-required="item.attr.Required"></textarea>',
                '</div>'].join(''),
            'bindings': [
                '<div class="col-sm-10">',
                    '<textarea class="form-control" name="bindings" ng-model="task.bindings" style="min-height:150px" ng-required="item.attr.Required"></textarea>',
                '</div>'].join(''),
            'virtualDirectories': [
                '<div class="col-sm-10">',
                    '<textarea class="form-control" name="virtualDirectories" ng-model="task.virtualDirectories" style="min-height:150px" ng-required="item.attr.Required"></textarea>',
                '</div>'].join(''),
        };
        $scope.custom.defaultValue = {
            poolConfig: 'Enable32BitAppOnWin64=false\nManagedRuntimeVersion=v4.0\nManagedPipelineMode=Integrated\nAutoStart=true\nFailure.OrphanWorkerProcess=false\nProcessModel.MaxProcesses=1\nProcessModel.LoadUserProfile=true\nProcessModel.IdentityType=ApplicationPoolIdentity'
        }
        $scope.custom.init = function () { }
    }]);
});