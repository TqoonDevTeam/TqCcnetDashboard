define(['app', 'guid', 'urijs/URI',
'controller/project/modules/svc'], function (app, guid, URI) {
    app.controller('project.step4.ctrl', ['$scope', '$uibModal', 'pathUtil', 'projectDesc', function ($scope, $uibModal, pathUtil, projectDesc) {
        this.getDesc = function (item) {
            return (projectDesc[item['@type']] || projectDesc['default'])(item);
        }
        this.onTasksAddClick = function () {
            var instance = $uibModal.open({
                templateUrl: pathUtil.GetTemplate('project/plugins.tmpl.html'),
                controller: 'project.plugins.ctrl',
                controllerAs: 'ctrl',
                scope: $scope,
                size: 'lg', backdrop: 'static',
                resolve: {
                    items: function () {
                        return {
                            mode: 'new',
                            pluginType: 'task',
                        };
                    }
                }
            });
            instance.result.then(function (item) {
                $scope.p.tasks.push(item);
            }, function () { });
        }
        this.onTasksDelClick = function (item) {
            $scope.p.tasks.remove(item);
        }
        this.onTasksModClick = function (item) {
            var instance = $uibModal.open({
                templateUrl: pathUtil.GetTemplate('project/plugins.tmpl.html'),
                controller: 'project.plugins.ctrl',
                controllerAs: 'ctrl',
                scope: $scope,
                size: 'lg', backdrop: 'static',
                resolve: {
                    items: function () {
                        return {
                            mode: 'mod',
                            pluginType: 'task',
                            item: angular.copy(item)
                        };
                    }
                }
            });
            instance.result.then(function (modItem) {
                var idx = _.indexOf($scope.p.tasks, item);
                if (idx > -1) {
                    $scope.p.tasks[idx] = modItem;
                }
            }, function () { });
        }
        this.onOrderUpClick = function (item) {
            var idx = _.indexOf($scope.p.tasks, item) - 1;
            if (idx > -1) {
                $scope.p.tasks.remove(item);
                $scope.p.tasks.insert(idx, item);
            }
        }
        this.onOrderDownClick = function (item) {
            var idx = _.indexOf($scope.p.tasks, item) + 1;
            if (idx < $scope.p.tasks.length) {
                $scope.p.tasks.remove(item);
                $scope.p.tasks.insert(idx, item);
            }
        }
        this.init = function () {
            $scope.p.tasks = $scope.p.tasks || [];
        }();
    }]);
});