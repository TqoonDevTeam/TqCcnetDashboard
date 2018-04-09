define(['app'], function (app) {
    app.service('project.svc', ['$http', 'svcUtil', function ($http, svcUtil) {
        var svc = {
            CcnetProject: svcUtil.create('CcnetProject'),
            SourceControlTemplate: svcUtil.create('SourceControlTemplate'),
            OwnedInnerTask: svcUtil.create('OwnedInnerTask'),
            GitHub: svcUtil.create('GitHub', true).$action('CheckRepository'),
            SvnHub: svcUtil.create('SvnHub', true).$action('CheckRepository'),
            PluginHelp: svcUtil.create('PluginHelp', true).$action('GetTaskPlugins').$action('GetScPlugins').$action('GetPublishPlugins')
        };
        return svc;
    }]);
    app.value('project.value', {
        taskPlugins: [],
        publishPlugins: [],
        toggle: {
            all_attrs: false,
            task_debug: false
        },
    });
    app.service('projectDesc', function () {
        return {
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
        };
    });
    app.controller('project.plugins.ctrl', ['$scope', 'items', '$uibModalInstance', 'pathUtil', 'project.value', 'project.svc', function ($scope, items, $uibModalInstance, pathUtil, pluginValue, svc) {
        var ctrl = this;

        // event
        ctrl.evt = {
            cancel: function () {
                $uibModalInstance.dismiss();
            },
            ok: function () {
                if ($scope.frmTaskAdd.$invalid) return;
                $uibModalInstance.close($scope.task);
            }
        }

        // function
        function onTaskWatch() {
            $scope.$watch('task["@type"]', function (newVal, oldVal) {
                if (newVal) {
                    if ((newVal !== oldVal) || forceTemplateLoad) {
                        $scope.templateUrl = pathUtil.GetTemplate('/project/plugins.default.tmpl.html' + "?_=" + newVal);
                    }
                } else {
                    $scope.templateUrl = undefined;
                }
            });
        }
        function setPluginValue() {
            if ($scope.config.pluginType == 'task') {
                if (_.isEmpty(ctrl.val.taskPlugins)) {
                    svc.PluginHelp.GetTaskPlugins().then(function (res) {
                        ctrl.val.taskPlugins = res.data;
                    });
                }
            }
            if ($scope.config.pluginType == 'publish') {
                if (_.isEmpty(ctrl.val.publishPlugins)) {
                    svc.PluginHelp.GetPublishPlugins().then(function (res) {
                        ctrl.val.publishPlugins = res.data;
                    });
                }
            }
        }
        ctrl.getPlugins = function () {
            if ($scope.config.pluginType == 'task') return ctrl.val.taskPlugins;
            return ctrl.val.publishPlugins;
        }

        // init
        ctrl.init = function () {
            ctrl.val = pluginValue;
            $scope.templateUrl = undefined;
            $scope.task = {};
            $scope.config = {
                mode: items.mode,
                pluginType: items.pluginType
            }
            if (items.mode == 'mod') {
                $scope.task = items.task;
            }
            setPluginValue();
            onTaskWatch();
        }; ctrl.init();
    }]);
    app.controller('project.plugins.panel.ctrl', ['$scope', '$rootScope', 'project.svc', 'pathUtil', 'project.value', function ($scope, $rootScope, svc, pathUtil, projectValue) {
        var default_attrs_force_show = ['description', 'workingDirectory'];
        var default_attrs_force_required = ['description'];
        $scope.attrs = [];
        $scope.custom = {};
        $scope.dynamicCtrl = {};

        // function
        function onToggleWatch() {
            $rootScope.$watch(function () { return projectValue.toggle.all_attrs; }, function (newVal) {
                if (newVal) {
                    on_attrs_force_show([]);
                } else {
                    _.each($scope.attrs, function (item) {
                        item.attr.$show = item.attr.$old_show || false;
                    });
                    on_attrs_force_show(default_attrs_force_show);
                    on_attrs_force_required(default_attrs_force_required);
                    onCustomSetting();
                }
            });
        }
        function on_attrs_force_show(names) {
            if (projectValue.toggle.all_attrs) {
                _.each($scope.attrs, function (v) { v.attr.$old_show = v.attr.$show; v.attr.$show = true; });
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
                $scope.dynamicCtrl.compile('project.plugins.' + $scope.task['@type'] + '.customctrl');
                onCustomSetting();
            }, function () {
                $scope.dynamicCtrl.compile('project.plugins.abstract.customctrl');
                onCustomSetting();
            })
        }

        // init
        this.init = function () {
            svc.SourceControlTemplate.get($scope.task['@type']).then(function (res) {
                $scope.attrs = res.data;
                _.each(_.filter(res.data, function (item) { return item.attr.Required; }), function (v) { v.attr.$show = true; });
                on_attrs_force_show(default_attrs_force_show);
                on_attrs_force_required(default_attrs_force_required);
                customCompile();
            });
            onToggleWatch();
        }();
    }]);
    app.controller('project.plugins.abstract.customctrl', ['$scope', function ($scope) {
        $scope.custom.attrs_force_show = [];
        $scope.custom.attrs_force_required = [];
        $scope.custom.template = {};
        $scope.custom.init = function () { };
    }]);
});