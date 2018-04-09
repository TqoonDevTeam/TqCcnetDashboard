define(['app'], function (app) {
    'use strict';
    app.controller('project.plugins.TqForeachFromDB.customctrl', ['$scope', '$uibModal', 'pathUtil', function ($scope, $uibModal, pathUtil) {
        $scope.custom.attrs_force_show = ['tasks'];
        $scope.custom.attrs_force_required = [];
        $scope.custom.template = {
            'query': [
                '<div class="col-sm-10">',
                    '<textarea class="form-control" name="query" ng-model="task.query" ng-required="item.attr.Required" style="min-height:150px"></textarea>',
                '</div>'].join(''),
            'tasks': [
                '<div class="col-sm-10">',
                    '<div class="row">',
                        '<div class="col-sm-12">',
                            '<button type="button" class="btn btn-success btn-sm" uib-tooltip="Task 추가" ng-click="customCtrl.onTasksAddClick()"><i class="fa fa-plus"></i></button>',
                        '</div>',
                        '<div class="col-sm-12" style="margin-top:10px">',
                            '<div class="list-group">',
                                '<div class="list-group-item" ng-repeat="item in task.tasks">',
                                    '<span class="label label-primary">{{item["@type"]}}</span>',
                                    '<span>{{customCtrl.getDesc(item)}}</span>',
                                    '<div class="pull-right">',
                                        '<button type="button" ng-click="customCtrl.onTasksModClick(item)" class="btn btn-xs btn-warning"><i class="fa fa-pencil"></i></button>',
                                        ' <button type="button" ng-click="customCtrl.onTasksDelClick(item)" class="btn btn-xs btn-danger"><i class="fa fa-times"></i></button>',
                                        ' <button type="button" uib-tooltip="순서변경" ng-disabled="$index === 0" class="btn btn-default btn-xs" ng-click="customCtrl.onOrderUpClick(item)"><i class="fa fa-arrow-circle-up"></i></button>',
                                        ' <button type="button" uib-tooltip="순서변경" ng-disabled="($index + 1) === p.tasks.length" class="btn btn-default btn-xs" ng-click="customCtrl.onOrderDownClick(item)"><i class="fa fa-arrow-circle-down"></i></button>',
                                    '</div>',
                                '</div>',
                            '</div>',
                        '</div>',
                    '</div>',
                '</div>'].join('')
        };
        $scope.custom.defaultValue = {}
        $scope.custom.init = function () { }

        var taskDesc = {
            'TqForeachFromDB': function (item) {
                var kv = {};
                var k;
                _.each(item.tasks, function (v) {
                    k = v['@type'];
                    kv[k] = (kv[k] || 0) + 1;
                });
                return item.description + ' ' + _.map(kv, function (v, p) {
                    return p + '(' + v + ')';
                }).join(', ');
            },
            'default': function (item) {
                return item.description || '';
            }
        }

        this.getDesc = function (item) {
            return (taskDesc[item['@type']] || taskDesc['default'])(item);
        }
        this.onTasksAddClick = function () {
            var instance = $uibModal.open({
                templateUrl: pathUtil.GetTemplate('/project/step4/tasks.add.tmpl.html'),
                controller: 'project.step4.tasks.add.ctrl',
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
                $scope.task.tasks.push(item);
            }, function () { });
        }
        this.onTasksDelClick = function (item) {
            $scope.task.tasks.remove(item);
        }
        this.onTasksModClick = function (item) {
            var instance = $uibModal.open({
                templateUrl: pathUtil.GetTemplate('/project/step4/tasks.add.tmpl.html'),
                controller: 'project.step4.tasks.add.ctrl',
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
            instance.result.then(function (modItem) {
                var idx = _.indexOf($scope.task.tasks, item);
                if (idx > -1) {
                    $scope.task.tasks[idx] = modItem;
                }
            }, function () { });
        }
        this.onOrderUpClick = function (item) {
            var idx = _.indexOf($scope.task.tasks, item) - 1;
            if (idx > -1) {
                $scope.task.tasks.remove(item);
                $scope.task.tasks.insert(idx, item);
            }
        }
        this.onOrderDownClick = function (item) {
            var idx = _.indexOf($scope.task.tasks, item) + 1;
            if (idx < $scope.task.tasks.length) {
                $scope.task.tasks.remove(item);
                $scope.task.tasks.insert(idx, item);
            }
        }
    }]);
});