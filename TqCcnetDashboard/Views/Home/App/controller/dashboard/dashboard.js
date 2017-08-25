define(['app',
    'controller/dashboard/modules/svc'], function (app) {
        app.filter('buildStatus', [function () {
            return function (status) {
                switch (status) {
                    case 0:
                        return 'Success';
                    case 1:
                        return 'Failure';
                    case 2:
                        return 'Exception';
                    case 3:
                        return 'Unknown';
                    case 4:
                        return 'Cancelled';
                    default:
                        return status;
                }
            };
        }]);
        app.filter('ccnetStatus', [function () {
            return function (status) {
                switch (status) {
                    case 0:
                        return 'Running';
                    case 1:
                        return 'Stopping';
                    case 2:
                        return 'Stopped';
                    case 3:
                        return 'Unknown';
                    default:
                        return status;
                }
            };
        }]);
        app.controller('dashboard.ctrl', ['$scope', '$rootScope', '$http', 'dashboard.svc', function ($scope, $rootScope, $http, svc) {
            $scope.align = 'list';
            $scope.projectList = [];
            $scope.categoryList = [];

            $rootScope.$watch('_ProjectStatus', refreshProjectList, true);
            $rootScope.$watch('_cookie', refreshProjectList, true);

            function refreshProjectList() {
                var dashboardCookieEmpty = isEmptyDashboardServerCookie();
                var projectListTemp = [];
                _.each($rootScope._cookie, function (v, p) {
                    if (p.startsWith('dashboard.server:')) {
                        if (v === 'true' || dashboardCookieEmpty) {
                            var key = p.replace('dashboard.server:', '');
                            _.each($rootScope._ProjectStatus[key], function (v) {
                                projectListTemp.push(v);
                            })
                        }
                    }
                });
                $scope.projectList = projectListTemp;
            }

            function isEmptyDashboardServerCookie() {
                return !_.some($rootScope._cookie, function (v, p) {
                    return p.startsWith('dashboard.server:') && v === 'true';
                })
            }

            this.getHeartBeatStype = function (item) {
                switch (item.BuildStatus) {
                    case 0:
                        return { 'color': '#337ab7' };
                    case 1:
                    case 2:
                        return { 'color': 'red' };
                    case 4:
                        return { 'color': '#f0ad4e' };
                    default:
                        return {};
                }
            }
            this.isBuilding = function (item) {
                if (item.Activity.Type === 'CheckingModifications') return true;
                else if (item.Activity.Type === 'Building') return true;
                else return false;
            }

            this.onAllServerClick = function () {
                _.each($rootScope._cookie, function (v, p) {
                    if (p.startsWith('dashboard.table:')) {
                        $rootScope._cookie[p] = 'true';
                    }
                });
            }

            this.showForceBuild = function (item) {
                switch (item.Activity.Type) {
                    case 'CheckingModifications':
                    case 'Building':
                        return false;
                    default:
                        return true;
                }
            }
            this.canForceBuild = function (item) {
                return item.ShowForceBuildButton && item.Activity.Type !== 'CheckingModifications' && item.Activity.Type !== 'Building';
            }
            this.onForceBuild = function (item) {
                if (confirm('빌드하시겠습니까?')) {
                    svc.Dashboard.ForceBuild({ projectName: item.Name, host: item.$host }).then(function () {
                    });
                }
            }
            this.onAbortBuild = function (item) {
                if (confirm('빌드를 취소하시겠습니까?')) {
                    svc.Dashboard.AbortBuild({ projectName: item.Name, host: item.$host }).then(function () {
                    });
                }
            }

            refreshProjectList();
        }]);
        app.controller('dashboard.table.ctrl', ['$scope', '$rootScope', function ($scope, $rootScope) {
            this.hasBreakers = function (p) {
                return _.some(p.Messages, function (v) { return v.Kind === 1; });
            }
            this.hasFailingTasks = function (p) {
                return _.some(p.Messages, function (v) { return v.Kind === 3; });
            }
            this.getBreakers = function (p) {
                return _.filter(p.Messages, function (v) { return v.Kind === 1; });
            }
            this.getFailingTasks = function (p) {
                return _.filter(p.Messages, function (v) { return v.Kind === 3; });
            }
        }]);
        app.controller('dashboard.panel.ctrl', ['$scope', '$rootScope', function ($scope, $rootScope) {
            this.getFilteredList = function () {
                var tempList = [];
                _.each($rootScope._cookie, function (v, p) {
                    if (p.startsWith('dashboard.table:')) {
                        if (v === 'true') {
                            var key = p.replace('dashboard.table:', '');
                            _.each($rootScope._ProjectStatus[key], function (v) {
                                tempList.push(v);
                            })
                        }
                    }
                });

                var maxDepth = _.max(tempList, function (p) { p.$child = [], p.$depth = p.Name.split('▶').length - 1; return p.$depth; });
                _.each(tempList, function (p) {
                    if (p.$depth > 0) {
                        var parentProjectName = _.initial(p.Name.split('▶')).join('▶');
                        var find = _.findWhere(tempList, { 'Name': parentProjectName });
                        if (find) {
                            find.$child.push(p);
                        }
                    }
                });
                return _.where(tempList, { '$depth': 0 })
            }
        }]);
    });