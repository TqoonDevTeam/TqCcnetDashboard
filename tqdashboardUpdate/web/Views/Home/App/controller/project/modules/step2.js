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
        }])
        .controller('project.step2.sc.add.ctrl', ['$scope', '$uibModalInstance', 'items', 'pathUtil', function ($scope, $uibModalInstance, items, pathUtil) {
            $scope.mode = items.mode;
            $scope.sc = {};
            $scope.scTemplate = {
                multi: pathUtil.GetTemplate('/project/step2/sc.multi.tmpl.html'),
                git: pathUtil.GetTemplate('/project/step2/sc.git.tmpl.html'),
                svn: pathUtil.GetTemplate('/project/step2/sc.svn.tmpl.html'),
            };
            $scope.abstract = {
                beforeOK: function () { }
            };
            this.cancel = function () {
                $uibModalInstance.dismiss();
            }
            this.ok = function () {
                if ($scope.frmScAdd.$invalid) return;
                $scope.abstract.beforeOK();
                $uibModalInstance.close($scope.sc);
            }
            this.init = function () {
                if ($scope.mode === 'mod') {
                    $scope.sc = items.item;
                }
            }();
        }])
        .controller('project.step2.sc.svn.ctrl', ['$scope', 'project.svc', function ($scope, svc) {
            var attrs_required_forced_key = ['trunkUrl', 'username', 'password', 'forceUpdate', 'cleanCopy', 'cleanUp', 'workingDirectory'];
            $scope.attrs = [];
            $scope.attrs_required = [];
            $scope.attrs_required_forced = [];

            $scope.abstract.beforeOK = function () {
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

            this.init = function () {
                svc.SourceControlTemplate.get('svn').then(function (res) {
                    $scope.attrs = res.data;
                    $scope.attrs_required = _.filter(res.data, function (item) { return item.attr.Required; });
                    $scope.attrs_required_forced = _.filter(res.data, function (item) { return _.contains(attrs_required_forced_key, item.attr.Name); });
                    _.each($scope.attrs_required_forced, function (v) { v.attr.Required = true });
                });
                if ($scope.mode === 'mod') {
                    $scope.sc.password = $scope.sc.dynamicValues.directValue['default'];
                }
            }();
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
        .controller('project.step2.sc.multi.ctrl', ['$scope', '$uibModal', 'pathUtil', function ($scope, $uibModal, pathUtil) {
            this.getDesc = function (sc) {
                return sourcecontrolDesc[sc['@type']](sc);
            };
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
            this.init = function () {
                $scope.sc.sourceControls = $scope.sc.sourceControls || [];
            }();
        }])
    });