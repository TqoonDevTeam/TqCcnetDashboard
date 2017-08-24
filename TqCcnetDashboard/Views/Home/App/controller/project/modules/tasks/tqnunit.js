define(['app'], function (app) {
    'use strict';
    app.controller('project.step4.tasks.tqnunit.customctrl', ['$scope', function ($scope) {
        $scope.custom.force_show = [];
        $scope.custom.forcer_equired = [];
        $scope.custom.template = {
            'version': '<div class="from-group">'
        };
        $scope.custom.init = function () {
            $scope.task.excludedCategories = $scope.task.excludedCategories || { string: [] };
            $scope.task.includedCategories = $scope.task.includedCategories || { string: [] };
            $scope.task.assemblies = $scope.task.assemblies || { string: [] };
        }

        $scope.excludeCategory = '';
        $scope.includeCategory = '';
        $scope.assembly = '';
        this.removeExclude = function (item) {
            $scope.task.excludedCategories.string.remove(item);
        }
        this.addExclude = function () {
            if ($scope.excludeCategory) {
                if (!_.contains($scope.task.excludedCategories.string, $scope.excludeCategory)) {
                    $scope.task.excludedCategories.string.push($scope.excludeCategory);
                }
                $scope.excludeCategory = '';
            }
        }
        this.removeInclude = function (item) {
            $scope.task.includedCategories.string.remove(item);
        }
        this.addInclude = function () {
            if ($scope.includeCategory) {
                if (!_.contains($scope.task.excludedCategories.string, $scope.includeCategory)) {
                    $scope.task.excludedCategories.string.push($scope.includeCategory);
                }
                $scope.includeCategory = '';
            }
        }
        this.removeAsm = function (item) {
            $scope.task.assemblies.string.remove(item);
        }
        this.addAsm = function () {
            if ($scope.assembly) {
                if (!_.contains($scope.task.assemblies.string, $scope.assembly)) {
                    $scope.task.assemblies.string.push($scope.assembly);
                }
                $scope.assembly = '';
            }
        }
    }]);
});