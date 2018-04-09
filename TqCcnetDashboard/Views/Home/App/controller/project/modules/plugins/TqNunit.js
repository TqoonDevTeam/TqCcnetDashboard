define(['app'], function (app) {
    'use strict';
    app.controller('project.plugins.TqNunit.customctrl', ['$scope', function ($scope) {
        $scope.custom.attrs_force_show = ['excludedCategories', 'includedCategories'];
        $scope.custom.attrs_force_required = [];
        $scope.custom.template = {
            'version': [
                '<div class="col-sm-10 radio">',
                    '<label><input type="radio" ng-model="task.version" name="Configuration" value="2" required />2.x</label>',
                    ' <label><input type="radio" ng-model="task.version" name="Configuration" value="3" required />3.x</label>',
                    ' <label><input type="radio" ng-model="task.version" name="Configuration" value="CORE" required />core</label>',
                '</div>'].join(''),
            'assemblies': [
                '<div class="col-sm-10">',
                    '<ul class="list-group">',
                        '<li class="list-group-item" ng-repeat="asm in task.assemblies.string"> {{asm}}<button class="btn btn-danger btn-xs pull-right" type="button" ng-click="customCtrl.removeAsm(asm)"><i class="fa fa-times"></i></button>',
                        '<li class="list-group-item" style="padding:4px"><input class="form-control" type="text" name="assembly" ng-model="icustom.assembly" placeholder="assembly" on-enter="customCtrl.addAsm()" /></li>',
                    '</ul>',
                '</div>'].join(''),
            'excludedCategories': [
                '<div class="col-sm-10">',
                    '<ul class="list-group">',
                        '<li class="list-group-item" ng-repeat="asm in task.excludedCategories.string"> {{asm}}<button class="btn btn-danger btn-xs pull-right" type="button" ng-click="customCtrl.removeExclude(asm)"><i class="fa fa-times"></i></button>',
                        '<li class="list-group-item" style="padding:4px"><input class="form-control" type="text" name="excludeCategory" ng-model="icustom.excludeCategory" placeholder="Category" on-enter="customCtrl.addExclude()" /></li>',
                    '</ul>',
                '</div>'].join(''),
            'includedCategories': [
                '<div class="col-sm-10">',
                    '<ul class="list-group">',
                        '<li class="list-group-item" ng-repeat="asm in task.includedCategories.string"> {{asm}}<button class="btn btn-danger btn-xs pull-right" type="button" ng-click="customCtrl.removeInclude(asm)"><i class="fa fa-times"></i></button>',
                        '<li class="list-group-item" style="padding:4px"><input class="form-control" type="text" name="includeCategory" ng-model="icustom.includeCategory" placeholder="Category" on-enter="customCtrl.addInclude()" /></li>',
                    '</ul>',
                '</div>'].join('')
        };
        $scope.custom.init = function () {
            $scope.task.excludedCategories = $scope.task.excludedCategories || { string: [] };
            $scope.task.includedCategories = $scope.task.includedCategories || { string: [] };
            $scope.task.assemblies = $scope.task.assemblies || { string: [] };
        }
        $scope.icustom = {
            excludeCategory: '',
            includeCategory: '',
            assembly: ''
        };

        this.removeExclude = function (item) {
            $scope.task.excludedCategories.string.remove(item);
        }
        this.addExclude = function () {
            if ($scope.icustom.excludeCategory) {
                if (!_.contains($scope.task.excludedCategories.string, $scope.icustom.excludeCategory)) {
                    $scope.task.excludedCategories.string.push($scope.excludeCategory);
                }
                $scope.icustom.excludeCategory = '';
            }
        }
        this.removeInclude = function (item) {
            $scope.task.includedCategories.string.remove(item);
        }
        this.addInclude = function () {
            if ($scope.icustom.includeCategory) {
                if (!_.contains($scope.task.excludedCategories.string, $scope.icustom.includeCategory)) {
                    $scope.task.excludedCategories.string.push($scope.icustom.includeCategory);
                }
                $scope.icustom.includeCategory = '';
            }
        }
        this.removeAsm = function (item) {
            $scope.task.assemblies.string.remove(item);
        }
        this.addAsm = function () {
            if ($scope.icustom.assembly) {
                if (!_.contains($scope.task.assemblies.string, $scope.icustom.assembly)) {
                    $scope.task.assemblies.string.push($scope.icustom.assembly);
                }
                $scope.icustom.assembly = '';
            }
        }
    }]);
});