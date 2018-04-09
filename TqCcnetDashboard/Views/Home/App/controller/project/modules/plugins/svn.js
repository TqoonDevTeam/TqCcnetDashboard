define(['app'], function (app) {
    'use strict';
    app.controller('project.plugins.svn.customctrl', ['$scope', 'pathUtil', function ($scope, pathUtil) {
        $scope.custom.attrs_force_show = ['trunkUrl', 'username', 'password', 'forceUpdate', 'cleanCopy', 'cleanUp', 'workingDirectory'];
        $scope.custom.attrs_force_required = ['trunkUrl', 'username', 'password', 'forceUpdate', 'cleanCopy', 'cleanUp', 'workingDirectory'];
        $scope.custom.template = {};
        $scope.custom.defaultValue = {};
        $scope.custom.beforeOK = function () {
            var password = $scope.sc.password;
            delete $scope.sc.password;
            $scope.sc.dynamicValues = {
                directValue: {
                    'default': password,
                    parameter: 'Password',
                    property: 'password'
                }
            };
        }
        $scope.custom.init = function () {
            if ($scope.mode === 'mod') {
                $scope.sc.password = $scope.sc.dynamicValues.directValue['default'];
            }
        }

        var sourcecontrolDesc = {
            'svn': function (item) {
                return item.trunkUrl;
            },
            'git': function (item) {
                return item.repository + ' ' + item.branch;
            },
            'multi': function (item) {
                var kv = {};
                var k;
                _.each(item.sourceControls, function (v) {
                    k = v['@type'];
                    kv[k] = (kv[k] || 0) + 1;
                });
                return _.map(kv, function (v, p) {
                    return p + '(' + v + ')';
                }).join(', ');
            }
        }

        this.getDesc = function (item) {
            return sourcecontrolDesc[item['@type']](item);
        }
        this.onScAddClick = function () {
            var instance = $uibModal.open({
                templateUrl: pathUtil.GetTemplate('/project/step2/sc.add.tmpl.html'),
                controller: 'project.step2.sc.add.ctrl',
                controllerAs: 'ctrl',
                scope: $scope,
                size: 'lg', backdrop: 'static',
                resolve: {
                    items: function () {
                        return {
                            mode: 'new'
                        };
                    }
                }
            });
            instance.result.then(function (item) {
                $scope.sc.sourceControls.push(item);
            }, function () { });
        }
        this.onScDelClick = function (item) {
            $scope.sc.sourceControls.remove(item);
        }
        this.onScModClick = function (item) {
            var instance = $uibModal.open({
                templateUrl: pathUtil.GetTemplate('/project/step2/sc.add.tmpl.html'),
                controller: 'project.step2.sc.add.ctrl',
                controllerAs: 'ctrl',
                scope: $scope,
                size: 'lg', backdrop: 'static',
                resolve: {
                    items: function () {
                        return {
                            mode: 'mod',
                            item: angular.copy(item)
                        };
                    }
                }
            });
            instance.result.then(function (sc) {
                var idx = _.indexOf($scope.sc.sourceControls, item);
                if (idx > -1) {
                    $scope.sc.sourceControls[idx] = sc;
                }
            }, function () { });
        }
    }]);
});