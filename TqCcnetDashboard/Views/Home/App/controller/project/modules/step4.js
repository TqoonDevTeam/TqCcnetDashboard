define(['app', 'guid', 'urijs/URI',
'controller/project/modules/svc'], function (app, guid, URI) {
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
    var taskPlugins = [];
    app.controller('project.step4.ctrl', ['$scope', '$uibModal', 'pathUtil', function ($scope, $uibModal, pathUtil) {
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
                $scope.p.tasks.push(item);
            }, function () { });
        }
        this.onTasksDelClick = function (item) {
            $scope.p.tasks.remove(item);
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
    }])
    .controller('project.step4.tasks.add.ctrl', ['$scope', '$uibModalInstance', 'items', 'project.svc', 'pathUtil', function ($scope, $uibModalInstance, items, svc, pathUtil) {
        var forceTemplateLoad = false;
        $scope.mode = items.mode;
        $scope.taskPlugins = taskPlugins;
        $scope.task = {};
        $scope.templateUrl = undefined;
        $scope.$watch('task["@type"]', function (newVal, oldVal) {
            if (newVal) {
                if ((newVal !== oldVal) || forceTemplateLoad) {
                    $scope.templateUrl = pathUtil.GetTemplate('/project/step4/task.default.html') + '?_=' + newVal;
                }
            } else {
                $scope.templateUrl = undefined;
            }
        });
        this.cancel = function () {
            $uibModalInstance.dismiss();
        }
        this.ok = function () {
            if ($scope.frmTaskAdd.$invalid) return;
            $uibModalInstance.close($scope.task);
        }
        this.init = function () {
            if (_.isEmpty(taskPlugins)) {
                svc.PluginHelp.GetTaskPlugins().then(function (res) {
                    taskPlugins = res.data;
                    $scope.taskPlugins = taskPlugins;
                });
            }
            if (items.mode === 'mod') {
                $scope.task = items.item;
                forceTemplateLoad = true;
            }
        }();
    }])
    .controller('project.step4.tasks.default.ctrl', ['$scope', '$rootScope', 'project.svc', 'pathUtil', function ($scope, $rootScope, svc, pathUtil) {
        var default_attrs_force_show = ['description', 'workingDirectory'];
        var default_attrs_force_required = ['description'];
        $scope.attrs = [];
        $scope.custom = {};
        $scope.dynamicCtrl = {};

        this.init = function () {
            svc.SourceControlTemplate.get($scope.task['@type']).then(function (res) {
                $scope.attrs = res.data;
                _.each(_.filter(res.data, function (item) { return item.attr.Required; }), function (v) { v.attr.$show = true; });
                on_attrs_force_show(default_attrs_force_show);
                on_attrs_force_required(default_attrs_force_required);
                customCompile();
            });
            onCookieWatch();
        }();
        function onCookieWatch() {
            $rootScope.$watch('_cookie.showAllModuleConfig', function (newVal) {
                if (newVal == '1') {
                    on_attrs_force_show([]);
                } else {
                    _.each(_.filter(res.data, function (item) { return item.attr.Required; }), function (v) { v.attr.$show = true; });
                    on_attrs_force_show(default_attrs_force_show);
                    on_attrs_force_required(default_attrs_force_required);
                    onCustomSetting();
                }
            });
        }
        function on_attrs_force_show(names) {
            if ($rootScope._cookie.showAllModuleConfig === "1") {
                _.each($scope.attrs, function (v) { v.attr.$show = true; });
            } else {
                _.each(_.filter($scope.attrs, function (item) { return _.contains(names, item.attr.Name); }), function (v) { v.attr.$show = true; });
            }
        }
        function on_attrs_force_required(names) {
            _.each(_.filter($scope.attrs, function (item) { return _.contains(names, item.attr.Name); }), function (v) { v.attr.Required = true; });
        }
        function on_defaultValueSet() {
            angular.extend($scope.task, angular.extend({}, $scope.custom.defaultValue || {}, $scope.task));
        }
        function onCustomSetting() {
            on_attrs_force_show($scope.custom.attrs_force_show);
            on_attrs_force_required($scope.custom.attrs_force_required);
            on_defaultValueSet();
        }
        function customCompile() {
            require([pathUtil.GetCustomTaskJsPath($scope.task['@type'])], function () {
                $scope.dynamicCtrl.compile('project.step4.tasks.' + $scope.task['@type'] + '.customctrl');
                onCustomSetting();
            }, function () {
                $scope.dynamicCtrl.compile('project.step4.tasks.abstract.customctrl');
                onCustomSetting();
            })
        }
    }])
    .controller('project.step4.tasks.abstract.customctrl', ['$scope', function ($scope) {
        $scope.custom.attrs_force_show = [];
        $scope.custom.attrs_force_required = [];
        $scope.custom.template = {};
        $scope.custom.init = function () { };
    }]);
});