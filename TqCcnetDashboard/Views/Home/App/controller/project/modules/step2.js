define(['app', 'guid', 'urijs/URI',
'controller/project/modules/svc'], function (app, guid, URI) {
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

    app.controller('project.step2.ctrl', ['$scope', '$uibModal', 'pathUtil', function ($scope, $uibModal, pathUtil) {
        this.getDesc = function (sc) {
            return sourcecontrolDesc[sc['@type']](sc);
        }

        this.onScAddClick = function () {
            if ($scope.p.sourcecontrol) {
                if ($scope.p.sourcecontrol['@type'] !== 'multi') {
                    alert('기 등록된 SourceControl 이 있습니다.\n계속 추가 하시면 현재 SourceControl 은 multi 형으로 변경됩니다.');
                }
            }

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
            instance.result.then(function (sc) {
                if ($scope.p.sourcecontrol) {
                    var type = $scope.p.sourcecontrol['@type'];
                    if (type === 'multi') {
                        $scope.p.sourcecontrol.sourceControls.push(sc);
                    } else {
                        var oldItem = $scope.p.sourcecontrol;
                        $scope.p.sourcecontrol = {
                            '@type': 'multi',
                            sourceControls: [oldItem, sc]
                        }
                    }
                } else {
                    $scope.p.sourcecontrol = sc;
                }
            }, function () { });
        }
        this.onScDelClick = function (item) {
            delete $scope.p.sourcecontrol;
        }
        this.onScModClick = function (item) {
            var instance = $uibModal.open({
                templateUrl: pathUtil.GetTemplate('/project/step2/sc.add.tmpl.html'),
                controller: 'project.step2.sc.add.ctrl',
                controllerAs: 'ctrl',
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
                $scope.p.sourcecontrol = sc;
            }, function () { });
        }
    }]);
    var scPlugins = [];
    app.controller('project.step2.sc.add.ctrl', ['$scope', '$uibModalInstance', 'items', 'project.svc', 'pathUtil', function ($scope, $uibModalInstance, items, svc, pathUtil) {
        var forceTemplateLoad = false;
        $scope.mode = items.mode;
        $scope.scPlugins = scPlugins;
        $scope.sc = {};
        $scope.templateUrl = undefined;
        $scope.$watch('sc["@type"]', function (newVal, oldVal) {
            if (newVal) {
                if ((newVal !== oldVal) || forceTemplateLoad) {
                    $scope.templateUrl = pathUtil.GetTemplate('/project/step2/sc.default.html') + '?_=' + newVal;
                }
            } else {
                $scope.templateUrl = undefined;
            }
        });
        $scope.custom = { beforeOK: function () { } };

        this.cancel = function () {
            $uibModalInstance.dismiss();
        }
        this.ok = function () {
            if ($scope.frmScAdd.$invalid) return;
            $scope.custom.beforeOK();
            $uibModalInstance.close($scope.sc);
        }
        this.init = function () {
            if (_.isEmpty(scPlugins)) {
                svc.PluginHelp.GetScPlugins().then(function (res) {
                    scPlugins = res.data;
                    $scope.scPlugins = scPlugins;
                });
            }
            if ($scope.mode === 'mod') {
                $scope.sc = items.item;
                forceTemplateLoad = true;
            }
        }();
    }])
    .controller('project.step2.sc.default.ctrl', ['$scope', 'project.svc', 'pathUtil', function ($scope, svc, pathUtil) {
        var default_attrs_force_show = ['description', 'workingDirectory'];
        var default_attrs_force_required = ['description'];
        $scope.attrs = [];
        $scope.dynamicCtrl = {};

        this.init = function () {
            svc.SourceControlTemplate.get($scope.sc['@type']).then(function (res) {
                $scope.attrs = res.data;
                _.each(_.filter(res.data, function (item) { return item.attr.Required; }), function (v) { v.attr.$show = true; });
                on_attrs_force_show(default_attrs_force_show);
                on_attrs_force_required(default_attrs_force_required);
                customCompile();
            });
        }();
        function on_attrs_force_show(names) {
            _.each(_.filter($scope.attrs, function (item) { return _.contains(names, item.attr.Name); }), function (v) { v.attr.$show = true; });
        }
        function on_attrs_force_required(names) {
            _.each(_.filter($scope.attrs, function (item) { return _.contains(names, item.attr.Name); }), function (v) { v.attr.Required = true; });
        }
        function on_defaultValueSet() {
            angular.extend($scope.sc, angular.extend({}, $scope.custom.defaultValue || {}, $scope.sc));
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
    }])
    .controller('project.step2.sc.git.ctrl', ['$scope', 'project.svc', function ($scope, svc) {
        var attrs_required_forced_key = ['branch', 'cleanUntrackedFiles', 'workingDirectory'];
        $scope.attrs = [];
        $scope.attrs_required = [];
        $scope.attrs_required_forced = [];

        this.init = function () {
            var defaultValue = { maxAmountOfModificationsToFetch: 1 };
            angular.extend($scope.sc, defaultValue, $scope.sc);

            svc.SourceControlTemplate.get('git').then(function (res) {
                $scope.attrs = res.data;
                $scope.attrs_required = _.filter(res.data, function (item) { return item.attr.Required; });
                $scope.attrs_required_forced = _.filter(res.data, function (item) { return _.contains(attrs_required_forced_key, item.attr.Name); });
                _.each($scope.attrs_required_forced, function (v) { v.attr.Required = true });
            });
        }();
    }])
});