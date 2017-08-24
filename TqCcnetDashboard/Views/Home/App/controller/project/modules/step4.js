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
        //$scope.taskTemplate = {
        //    TqNunit: pathUtil.GetTemplate('/project/step4/task.tqnunit.html'),
        //    TqIIS: pathUtil.GetTemplate('/project/step4/task.tqiis.html'),
        //    nuget: pathUtil.GetTemplate('/project/step4/task.nuget.html'),
        //    msbuild: pathUtil.GetTemplate('/project/step4/task.msbuild.html'),
        //    exec: pathUtil.GetTemplate('/project/step4/task.exec.html'),
        //    TqForeachFromDB: pathUtil.GetTemplate('/project/step4/task.tqforeachfromdb.html'),
        //    TqDBExecutor: pathUtil.GetTemplate('/project/step4/task.tqdbexecutor.html'),
        //    TqText: pathUtil.GetTemplate('/project/step4/task.tqtext.html'),
        //    TqRsync: pathUtil.GetTemplate('/project/step4/task.tqrsync.html'),
        //    'default': pathUtil.GetTemplate('/project/step4/task.default.html'),
        //};
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
    .controller('project.step4.tasks.default.ctrl', ['$scope', 'project.svc', 'pathUtil', function ($scope, svc, pathUtil) {
        var attrs_force_show = ['description', 'workingDirectory'];
        $scope.attrs = [];
        $scope.custom = {};
        $scope.dynamicCtrl = {};

        this.init = function () {
            svc.SourceControlTemplate.get($scope.task['@type']).then(function (res) {
                $scope.attrs = res.data;
                _.each(_.filter(res.data, function (item) { return item.attr.Required; }), function (v) { v.attr.$show = true; });
                _.each(_.filter(res.data, function (item) { return _.contains(attrs_force_show, item.attr.Name); }), function (v) { v.attr.$show = true; });
                customCompile();
            });
        }();

        function customCompile() {
            require([pathUtil.GetCustomTaskJsPath($scope.task['@type'])], function () {
                $scope.dynamicCtrl.compile('project.step4.tasks.' + $scope.task['@type'] + '.customctrl');
            }, function () {
                $scope.dynamicCtrl.compile('project.step4.tasks.default.customctrl');
            })
        }
    }])
    .controller('project.step4.tasks.default.customctrl', ['$scope', function ($scope) {
        $scope.custom.force_show = [];
        $scope.custom.forcer_equired = [];
        $scope.custom.init = function () {
            console.log('init default');
        }
    }])
    .controller('project.step4.tasks.add.exec.ctrl', ['$scope', 'project.svc', function ($scope, svc) {
        var defaultValue = { buildTimeoutSeconds: 120 };
        var attrs_required_forced_key = ['description', 'buildArgs', 'buildTimeoutSeconds', 'successExitCodes', 'baseDirectory'];
        $scope.attrs = [];
        $scope.attrs_required = [];
        $scope.attrs_required_forced_key = [];

        this.init = function () {
            svc.SourceControlTemplate.get($scope.task['@type']).then(function (res) {
                $scope.attrs = res.data;
                $scope.attrs_required = _.filter(res.data, function (item) { return item.attr.Required; });
                $scope.attrs_required_forced = _.filter(res.data, function (item) { return _.contains(attrs_required_forced_key, item.attr.Name); });
                _.each($scope.attrs_required_forced, function (v) { v.attr.Required = true });
            });
            angular.extend($scope.task, angular.extend({}, defaultValue, $scope.task));
        }();
    }])
    .controller('project.step4.tasks.add.msbuild.ctrl', ['$scope', 'project.svc', function ($scope, svc) {
        var defaultValue_force = { executable: 'msbuild.exe' };
        var attrs_required_forced_key = ['description', 'projectFile', 'targets', 'buildArgs'];
        $scope.attrs = [];
        $scope.attrs_required = [];
        $scope.attrs_required_forced_key = [];

        this.init = function () {
            svc.SourceControlTemplate.get($scope.task['@type']).then(function (res) {
                $scope.attrs = res.data;
                $scope.attrs_required = _.filter(res.data, function (item) { return item.attr.Required; });
                $scope.attrs_required_forced = _.filter(res.data, function (item) { return _.contains(attrs_required_forced_key, item.attr.Name); });
                _.each($scope.attrs_required_forced, function (v) { v.attr.Required = true });
            });
            angular.extend($scope.task, defaultValue_force);
        }();
    }])
    .controller('project.step4.tasks.add.tqnunit.ctrl', ['$scope', 'project.svc', function ($scope, svc) {
        var attrs_required_forced_key = ['description', 'workingDirectory'];
        $scope.attrs = [];
        $scope.attrs_required = [];
        $scope.attrs_required_forced_key = [];
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

        this.init = function () {
            svc.SourceControlTemplate.get($scope.task['@type']).then(function (res) {
                $scope.attrs = res.data;
                $scope.attrs_required = _.filter(res.data, function (item) { return item.attr.Required; });
                $scope.attrs_required_forced = _.filter(res.data, function (item) { return _.contains(attrs_required_forced_key, item.attr.Name); });
                _.each($scope.attrs_required_forced, function (v) { v.attr.Required = true });
            });
        }();

        $scope.task.excludedCategories = $scope.task.excludedCategories || { string: [] };
        $scope.task.includedCategories = $scope.task.includedCategories || { string: [] };
        $scope.task.assemblies = $scope.task.assemblies || { string: [] };
    }])
    .controller('project.step4.tasks.add.tqiis.ctrl', ['$scope', 'project.svc', function ($scope, svc) {
        var attrs_required_forced_key = ['description'];
        $scope.attrs = [];
        $scope.attrs_required = [];
        $scope.attrs_required_forced_key = [];

        this.init = function () {
            svc.SourceControlTemplate.get($scope.task['@type']).then(function (res) {
                $scope.attrs = res.data;
                $scope.attrs_required = _.filter(res.data, function (item) { return item.attr.Required; });
                $scope.attrs_required_forced = _.filter(res.data, function (item) { return _.contains(attrs_required_forced_key, item.attr.Name); });
                _.each($scope.attrs_required_forced, function (v) { v.attr.Required = true });
            });
        }();
    }])
    .controller('project.step4.tasks.add.tqforeachfromdb.ctrl', ['$scope', '$uibModal', 'pathUtil', 'project.svc', function ($scope, $uibModal, pathUtil, svc) {
        var attrs_required_forced_key = ['description'];
        $scope.attrs = [];
        $scope.attrs_required = [];
        $scope.attrs_required_forced_key = [];

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

        this.init = function () {
            svc.SourceControlTemplate.get($scope.task['@type']).then(function (res) {
                $scope.attrs = res.data;
                $scope.attrs_required = _.filter(res.data, function (item) { return item.attr.Required; });
                $scope.attrs_required_forced = _.filter(res.data, function (item) { return _.contains(attrs_required_forced_key, item.attr.Name); });
                _.each($scope.attrs_required_forced, function (v) { v.attr.Required = true });
            });
            $scope.task.tasks = $scope.task.tasks || [];
        }();
    }])
    .controller('project.step4.tasks.add.tqtext.ctrl', ['$scope', 'project.svc', function ($scope, svc) {
        var defaultValue = { saveType: 'Text', saveCondition: 'IfChanged' };
        var attrs_required_forced_key = ['description', 'saveEncoding'];
        $scope.attrs = [];
        $scope.attrs_required = [];
        $scope.attrs_required_forced_key = [];
        $scope.attrs_un_required_forced_key = ['source'];

        this.init = function () {
            svc.SourceControlTemplate.get($scope.task['@type']).then(function (res) {
                $scope.attrs = res.data;
                _.each($scope.attrs_un_required_forced_key, function (v) {
                    var find = _.find(res.data, function (item) { return item.attr.Name === v; });
                    if (find) {
                        find.attr.Required = false;
                    }
                });
                $scope.attrs_required = _.filter(res.data, function (item) { return item.attr.Required; });
                $scope.attrs_required_forced = _.filter(res.data, function (item) { return _.contains(attrs_required_forced_key, item.attr.Name); });
                _.each($scope.attrs_required_forced, function (v) { v.attr.Required = true });
            });
            angular.extend($scope.task, angular.extend({}, defaultValue, $scope.task));
        }();
    }])
    .controller('project.step4.tasks.add.tqrsync.ctrl', ['$scope', 'project.svc', function ($scope, svc) {
        var defaultValue = { options: '-avrzP --chmod=ugo=rwX' };
        var attrs_required_forced_key = ['description', 'workingDirectory'];
        var attrs_required_forced_key_not_required = ['workingDirectory'];
        $scope.attrs = [];
        $scope.attrs_required = [];
        $scope.attrs_required_forced_key = [];

        this.init = function () {
            svc.SourceControlTemplate.get($scope.task['@type']).then(function (res) {
                $scope.attrs = res.data;
                $scope.attrs_required = _.filter(res.data, function (item) { return item.attr.Required; });
                $scope.attrs_required_forced = _.filter(res.data, function (item) { return _.contains(attrs_required_forced_key, item.attr.Name); });
                _.each($scope.attrs_required_forced, function (v) {
                    if (!_.contains(attrs_required_forced_key_not_required, item.attr.Name)) {
                        v.attr.Required = true
                    }
                });
            });
            angular.extend($scope.task, angular.extend({}, defaultValue, $scope.task));
        }();
    }])
    .controller('project.step4.tasks.add.tqdbexecutor.ctrl', ['$scope', 'project.svc', function ($scope, svc) {
        var defaultValue = {};
        var attrs_required_forced_key = ['description'];
        $scope.attrs = [];
        $scope.attrs_required = [];
        $scope.attrs_required_forced_key = [];

        this.init = function () {
            svc.SourceControlTemplate.get($scope.task['@type']).then(function (res) {
                $scope.attrs = res.data;
                $scope.attrs_required = _.filter(res.data, function (item) { return item.attr.Required; });
                $scope.attrs_required_forced = _.filter(res.data, function (item) { return _.contains(attrs_required_forced_key, item.attr.Name); });
                _.each($scope.attrs_required_forced, function (v) { v.attr.Required = true });
            });
            angular.extend($scope.task, angular.extend({}, defaultValue, $scope.task));
        }();
    }])
});