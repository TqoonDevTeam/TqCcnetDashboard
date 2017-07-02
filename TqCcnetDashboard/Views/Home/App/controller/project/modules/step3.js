define(['app', 'md5', 'urijs/URI'], function (app, md5, URI) {
    var triggerDesc = {
        'intervalTrigger': function (item) {
            return item.buildCondition + ' ' + item.seconds + ' ' + item.initialSeconds;
        },
        'scheduleTrigger': function (item) {
            var weeks = item.weekDays.string.join(', ');
            return item.buildCondition + ' ' + item.time + ' ' + weeks;
        },
        'cronTrigger': function (item) {
            return item.buildCondition + ' ' + item.cronExpression;
        },
        'projectTrigger': function (item) {
            return 'Project dependency... ' + item.project;
        },
        'multiTrigger': function (item) {
            var kv = {};
            var k;
            _.each(item.triggers, function (v) {
                k = v['@type'];
                kv[k] = (kv[k] || 0) + 1;
            });
            return _.map(kv, function (v, p) {
                return p + '(' + v + ')';
            }).join(', ');
        }
    }

    app.controller('project.step3.ctrl', ['$scope', '$uibModal', 'pathUtil', function ($scope, $uibModal, pathUtil) {
        this.getDesc = function (item) {
            return triggerDesc[item['@type']](item);
        }
        this.onTriggersAddClick = function () {
            var instance = $uibModal.open({
                templateUrl: pathUtil.GetTemplate('/project/step3/triggers.add.tmpl.html'),
                controller: 'project.step3.triggers.add.ctrl',
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
            instance.result.then(function (tr) {
                $scope.p.triggers.push(tr);
            }, function () { });
        }
        this.onTriggersDelClick = function (item) {
            $scope.p.triggers.remove(item);
        }
        this.onTriggersModClick = function (item) {
            var instance = $uibModal.open({
                templateUrl: pathUtil.GetTemplate('/project/step3/triggers.add.tmpl.html'),
                controller: 'project.step3.triggers.add.ctrl',
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
            instance.result.then(function (tr) {
                var idx = _.indexOf($scope.p.triggers, item);
                if (idx > -1) {
                    $scope.p.triggers[idx] = tr;
                }
            }, function () { });
        }
        this.init = function () {
            $scope.p.triggers = $scope.p.triggers || [];
        }();
    }])
    .controller('project.step3.triggers.add.ctrl', ['$scope', '$uibModalInstance', 'items', '$filter', 'pathUtil', function ($scope, $uibModalInstance, items, $filter, pathUtil) {
        $scope.mode = items.mode;
        $scope.trigger = { buildCondition: 'IfModificationExists' };
        $scope.schedule = { weekDays: { Sunday: false, Monday: false, Tuesday: false, Wednesday: false, Thursday: false, Friday: false, Saturday: false }, time: new Date(1984, 0, 25, 12, 0) };
        $scope.triggerTemplate = {
            intervalTrigger: pathUtil.GetTemplate('/project/step3/triggers.intervalTrigger.tmpl.html'),
            scheduleTrigger: pathUtil.GetTemplate('/project/step3/triggers.scheduleTrigger.tmpl.html'),
            cronTrigger: pathUtil.GetTemplate('/project/step3/triggers.cronTrigger.tmpl.html'),
            multiTrigger: pathUtil.GetTemplate('/project/step3/triggers.multiTrigger.tmpl.html'),
            projectTrigger: pathUtil.GetTemplate('/project/step3/triggers.projectTrigger.tmpl.html'),
            'default': pathUtil.GetTemplate('/project/step3/triggers.default.tmpl.html')
        };
        $scope.buildConditions = [
        { key: 'IfModificationExists', val: 'IfModificationExists' },
        { key: 'ForceBuild', val: 'ForceBuild' }
        ];
        this.getTemplateUrl = function () {
            if (!$scope.trigger['@type']) return undefined;
            return $scope.triggerTemplate[$scope.trigger['@type']] || $scope.triggerTemplate['default'];
        }
        this.cancel = function () {
            $uibModalInstance.dismiss();
        }
        this.ok = function () {
            if ($scope.frmTriggerAdd.$invalid) return;
            $uibModalInstance.close($scope.trigger);
        }
        this.init = function () {
            if ($scope.mode === 'mod') {
                $scope.trigger = items.item;
            }
        }();
    }])
    .controller('project.step3.triggers.default.ctrl', ['$scope', '$uibModal', 'pathUtil', function ($scope, $uibModal, pathUtil) {
        this.getDesc = function (k, v) {
            return triggerDesc[k](v);
        }
        this.init = function () {
            $scope.p.triggers = $scope.p.triggers || {};
        }();
    }])
    .controller('project.step3.triggers.scheduleTrigger.ctrl', ['$scope', '$filter', function ($scope, $filter) {
        $scope.weeks = {};
        $scope.time = null;

        $scope.$watchCollection('weeks', function () {
            $scope.trigger.weekDays.string = [];
            _.each($scope.weeks, function (v, p) {
                if (v) $scope.trigger.weekDays.string.push(p);
            });
        });
        $scope.$watch('time', function (newVal) {
            if (newVal) {
                $scope.trigger.time = $filter('date')($scope.time, 'HH:mm').substring(0, 5);
            }
        });

        this.init = function () {
            if ($scope.mode === 'mod') {
                _.each($scope.trigger.weekDays.string, function (v) {
                    $scope.weeks[v] = true;
                });
                var t = $scope.trigger.time.split(':');
                var h = parseInt(t[0]);
                var m = parseInt(t[1]);
                $scope.time = new Date(1984, 0, 25, h, m);
            } else {
                $scope.trigger.weekDays = { string: [] };
                $scope.time = new Date(1984, 0, 25, 12, 0);
            }
        }();
    }])
    .controller('project.step3.triggers.projectTrigger.ctrl', ['$scope', function ($scope) {
        var defaultValue = { serverUri: 'tcp://localhost:21234/CruiseManager.rem' };

        this.init = function () {
            var defaultMerge = {};
            angular.extend($scope.trigger, angular.extend({}, defaultValue, $scope.trigger));
        }();
    }])
    .controller('project.step3.triggers.multiTrigger.ctrl', ['$scope', '$uibModal', 'pathUtil', function ($scope, $uibModal, pathUtil) {
        var defaultValue = { operator: 'Or', triggers: [] };
        this.getDesc = function (item) {
            return triggerDesc[item['@type']](item);
        }
        this.onTriggersAddClick = function () {
            var instance = $uibModal.open({
                templateUrl: pathUtil.GetTemplate('/project/step3/triggers.add.tmpl.html'),
                controller: 'project.step3.triggers.add.ctrl',
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
            instance.result.then(function (tr) {
                $scope.trigger.triggers.push(tr);
            }, function () { });
        }
        this.onTriggersDelClick = function (item) {
            $scope.trigger.triggers.remove(item);
        }
        this.onTriggersModClick = function (item) {
            var instance = $uibModal.open({
                templateUrl: pathUtil.GetTemplate('/project/step3/triggers.add.tmpl.html'),
                controller: 'project.step3.triggers.add.ctrl',
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
            instance.result.then(function (tr) {
                var idx = _.indexOf($scope.trigger.triggers, item);
                if (idx > -1) {
                    $scope.trigger.triggers[idx] = tr;
                }
            }, function () { });
        }
        this.init = function () {
            angular.extend($scope.trigger, angular.extend({}, defaultValue, $scope.trigger));
        }();
    }])
});