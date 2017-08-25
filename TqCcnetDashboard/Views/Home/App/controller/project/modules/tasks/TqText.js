define(['app'], function (app) {
    'use strict';
    app.controller('project.step4.tasks.TqText.customctrl', ['$scope', function ($scope) {
        $scope.custom.attrs_force_show = ['saveEncoding', 'saveCondition'];
        $scope.custom.attrs_force_required = [];
        $scope.custom.template = {
            'saveCondition': [
                '<div class="col-sm-10">',
                    '<select class="form-control" name="saveType" ng-model="task.saveCondition" required>',
                        '<option value="IfChanged">IfChanged</option>',
                        '<option value="Force">Force</option>',
                    '</select>',
                '</div>'].join(''),
            'source': [
                '<div class="col-sm-10">',
                    '<textarea class="form-control" name="source" ng-model="task.source" style="min-height:150px"></textarea>',
                '</div>'].join(''),
        };
        $scope.custom.defaultValue = {
            saveEncoding: 'UTF-8', saveCondition: 'IfChanged'
        }
        $scope.custom.init = function () { }
    }]);
});