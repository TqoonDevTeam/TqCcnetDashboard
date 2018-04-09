define(['app', 'guid', 'urijs/URI',
'controller/project/modules/svc'], function (app, guid, URI) {
    app.controller('project.step5.ctrl', ['$scope', '$uibModal', 'pathUtil', 'projectDesc', function ($scope, $uibModal, pathUtil, projectDesc) {
        this.getDesc = function (item) {
            return (projectDesc[item['@type']] || projectDesc['default'])(item);
        }
        this.onDefaultPubAddClick = function () {
            if (!_.find($scope.p.publishers, function (item) { return item['@type'] === 'xmllogger'; })) {
                $scope.p.publishers.push({
                    '@type': 'xmllogger'
                });
            }
            if (!_.find($scope.p.publishers, function (item) { return item['@type'] === 'artifactcleanup'; })) {
                $scope.p.publishers.push({
                    '@type': 'artifactcleanup',
                    'cleanUpMethod': 'KeepLastXBuilds',
                    'cleanUpValue': '20'
                });
            }
        }
        this.onPubAddClick = function () {
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
                            pluginType: 'publish',
                        };
                    }
                }
            });
            instance.result.then(function (item) {
                $scope.p.publishers.push(item);
            }, function () { });
        }
        this.onPubDelClick = function (item) {
            $scope.p.publishers.remove(item);
        }
        this.onPubModClick = function (item) {
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
                var idx = _.indexOf($scope.p.publishers, item);
                if (idx > -1) {
                    $scope.p.publishers[idx] = modItem;
                }
            }, function () { });
        }
        this.onOrderUpClick = function (item) {
            var idx = _.indexOf($scope.p.publishers, item) - 1;
            if (idx > -1) {
                $scope.p.publishers.remove(item);
                $scope.p.publishers.insert(idx, item);
            }
        }
        this.onOrderDownClick = function (item) {
            var idx = _.indexOf($scope.p.publishers, item) + 1;
            if (idx < $scope.p.publishers.length) {
                $scope.p.publishers.remove(item);
                $scope.p.publishers.insert(idx, item);
            }
        }
        this.init = function () {
            $scope.p.publishers = $scope.p.publishers || [];
        }();
    }]);
    //.controller('project.step5.publishers.add.ctrl', ['$scope', '$uibModalInstance', 'items', 'project.svc', 'pathUtil', function ($scope, $uibModalInstance, items, svc, pathUtil) {
    //    $scope.mode = items.mode;
    //    $scope.task = {};
    //    $scope.taskTemplate = {
    //        xmllogger: pathUtil.GetTemplate('/project/step5/publishers.xmllogger.html'),
    //        artifactcleanup: pathUtil.GetTemplate('/project/step5/publishers.artifactcleanup.html'),
    //        forcebuild: pathUtil.GetTemplate('/project/step5/publishers.forcebuild.html'),
    //        'default': pathUtil.GetTemplate('/project/step5/publishers.default.html'),
    //    };

    //    this.getTemplateUrl = function () {
    //        if (!$scope.task['@type']) return undefined;
    //        return $scope.taskTemplate[$scope.task['@type']] || $scope.taskTemplate['default'];
    //    }
    //    this.cancel = function () {
    //        $uibModalInstance.dismiss();
    //    }
    //    this.ok = function () {
    //        if ($scope.frmTaskAdd.$invalid) return;
    //        $uibModalInstance.close($scope.task);
    //    }
    //    this.init = function () {
    //        if (items.mode === 'mod') {
    //            console.log(items);
    //            $scope.task = items.item;
    //        }
    //    }();
    //}])
    //.controller('project.step5.publishers.add.default.ctrl', ['$scope', 'project.svc', function ($scope, svc) {
    //    var attrs_required_forced_key = ['description'];
    //    $scope.attrs = [];
    //    $scope.attrs_required = [];
    //    $scope.attrs_required_forced_key = [];

    //    this.init = function () {
    //        svc.SourceControlTemplate.get($scope.task['@type']).then(function (res) {
    //            $scope.attrs = res.data;
    //            $scope.attrs_required = _.filter(res.data, function (item) { return item.attr.Required; });
    //            $scope.attrs_required_forced = _.filter(res.data, function (item) { return _.contains(attrs_required_forced_key, item.attr.Name); });
    //            _.each($scope.attrs_required_forced, function (v) { v.attr.Required = true });
    //        });
    //    }();
    //}])
    //.controller('project.step5.publishers.add.forcebuild.ctrl', ['$scope', 'project.svc', function ($scope, svc) {
    //    var defaultValue = { integrationStatus: 'Success', enforcerName: 'Forcer' };
    //    var attrs_required_forced_key = ['description', 'serverUri', 'enforcerName'];
    //    $scope.attrs = [];
    //    $scope.attrs_required = [];
    //    $scope.attrs_required_forced_key = [];

    //    this.init = function () {
    //        svc.SourceControlTemplate.get($scope.task['@type']).then(function (res) {
    //            $scope.attrs = res.data;
    //            $scope.attrs_required = _.filter(res.data, function (item) { return item.attr.Required; });
    //            $scope.attrs_required_forced = _.filter(res.data, function (item) { return _.contains(attrs_required_forced_key, item.attr.Name); });
    //            _.each($scope.attrs_required_forced, function (v) { v.attr.Required = true });
    //        });
    //        angular.extend($scope.task, angular.extend({}, defaultValue, $scope.task));
    //    }();
    //}])
});