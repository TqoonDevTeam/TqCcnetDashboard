define(['app'], function (app) {
    'use strict';
    app.controller('project.step4.tasks.TqDBExecutor.customctrl', ['$scope', function ($scope) {
        $scope.custom.attrs_force_show = ['siteConfig', 'virtualDirectories'];
        $scope.custom.attrs_force_required = [];
        $scope.custom.template = {
            'query': [
                '<div class="col-sm-10">',
                    '<textarea class="form-control" name="query" ng-model="task.query" ng-required="item.attr.Required" style="min-height:150px"></textarea>',
                '</div>'].join('')
        };
        $scope.custom.defaultValue = {}
        $scope.custom.init = function () { }
    }]);
});