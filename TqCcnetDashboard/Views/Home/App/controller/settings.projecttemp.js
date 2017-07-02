define(['app'], function (app) {
    app.service('systemsettings.projecttempSvc', ['$rootScope', 'svcUtil', '$http', '$q', function ($rootScope, svcUtil, $http, $q) {
        var ProjectTempSet = svcUtil.CreateAPI('ProjectTempSet');
        var TasksTempSet = svcUtil.CreateAPI('TasksTempSet');
        var TasksPluginSet = svcUtil.CreateAPI('TasksPluginSet');
        var Enum = svcUtil.CreateAPI('Enum');
        Enum.getCiTriggers = function () {
            return $http.get(Enum.url + '/CI_Triggers');
        }

        var svc = {
            ProjectTempSet: ProjectTempSet,
            TasksTempSet: TasksTempSet,
            TasksPluginSet: TasksPluginSet,
            ViewReady: function () {
                return $q.all([ProjectTempSet.get(), Enum.getCiTriggers()]).then(function (res) {
                    $rootScope._view.ready = true;
                    return {
                        projecttemplist: res[0].data,
                        enumCiTriggers: res[1].data
                    };
                });
            }
        };
        return svc;
    }])
    .controller('settings.projecttempCtrl', ['$scope', '$http', 'systemsettings.projecttempSvc', '$mdDialog', function ($scope, $http, $svc, $mdDialog) {
        $scope.projecttemplist = [];
        $scope.enumCiTriggers = [];
        $scope.tasktemplist = [];
        $scope.prgsShow = true;
        $scope.selectedProject = null;

        $svc.ViewReady().then(function (res) {
            $scope.projecttemplist = res.projecttemplist;
            $scope.enumCiTriggers = res.enumCiTriggers;
        });

        $scope.addProjectTemp = function (ev) {
            var dialog = $mdDialog.show({
                controller: 'settings.projecttemp.add.project',
                templateUrl: '/Views/Home/App/views/settings.projecttemp.add.project.tmpl.html',
                parent: angular.element(document.body),
                targetEvent: ev,
                locals: { enumCiTriggers: $scope.enumCiTriggers },
                clickOutsideToClose: false
            });
            dialog.then(function () {
                $svc.ProjectTempSet.get().then(function (res) {
                    $scope.projecttemplist = res.data;
                });
            });
        }
        $scope.deleteProjectTemp = function (ev, item) {
            var confirm = $mdDialog.confirm()
           .title('삭제 하시겠습니까?')
           .textContent(item.name)
           .targetEvent(ev)
           .ok('확인')
           .cancel('취소');

            $mdDialog.show(confirm).then(function () {
                $svc.ProjectTempSet.del(item.id).then(function () {
                    $svc.ProjectTempSet.get().then(function (res) {
                        $scope.projecttemplist = res.data;
                        $scope.selectedProject = null;
                    });
                });
            });
        }

        $scope.addTasksTemp = function (ev) {
            var dialog = $mdDialog.show({
                controller: 'settings.projecttemp.add.tasks',
                templateUrl: '/Views/Home/App/views/settings.projecttemp.add.tasks.tmpl.html',
                parent: angular.element(document.body),
                targetEvent: ev,
                clickOutsideToClose: false
            });
            dialog.then(function () {
            });
        }

        $scope.selectProject = function (item) {
            $scope.selectedProject = item;
            $svc.TasksTempSet.get(item.id).then(function (res) {
                $scope.tasktemplist = res.data;
            });
        }
    }])
    .controller('settings.projecttemp.add.project', ['$scope', '$mdDialog', 'systemsettings.projecttempSvc', 'enumCiTriggers', '$timeout', function ($scope, $mdDialog, $svc, enumCiTriggers, $timeout) {
        $scope.enumCiTriggers = enumCiTriggers;
        $scope.p = {
            name: '', desc: '', triggers: []
        };

        $scope.mode = 'new';
        $scope.tabIndex = 0;

        $scope.newTrigger = { trtype: '', virtualFunction: origin_virtualFunction };
        function origin_virtualFunction() { };

        $scope.addTrigger = function () {
            $scope.mode = 'trigger_new';
            $scope.newTrigger.trtype = '';
            $scope.tabIndex = 1;
        }

        $scope.getTriggerTemplateUrl = function () {
            if ($scope.newTrigger.trtype) {
                return '/Views/Home/App/views/settings.projecttemp.add.ci.trigger.' + $scope.newTrigger.trtype + '.tmpl.html';
            }
            return null;
        };
        $scope.getConfirmButtonName = function () {
            if ($scope.mode === 'trigger_new') return '트리거 추가';
            return '프로젝트 추가';
        }
        $scope.getTriggerIcon = function (tr) {
            if (tr.trtype === 'scheduleTrigger') return 'action:today'
            if (tr.trtype === 'intervalTrigger') return 'image:timer'
            if (tr.trtype === 'cronTrigger') return 'editor:text_fields'
            return 'action:today';
        }

        $scope.cancel = function (force) {
            if (force) {
                $mdDialog.cancel();
                return;
            }

            if ($scope.mode === 'new') {
                $mdDialog.cancel();
                return;
            }

            goProjectMode();
        }
        $scope.answer = function () {
            if ($scope.mode === 'trigger_new') {
                trigger_new_end();
            }
            if ($scope.mode === 'new') {
                if ($scope.frmProject.$invalid) return;
                prgsShow = true;
                $svc.ProjectTempSet.post($scope.p).then(function () {
                    prgsShow = false;
                    $mdDialog.hide();
                });
            }
        }

        function trigger_new_end() {
            $scope.newTrigger.virtualFunction().then(function (data) {
                data.trtype = $scope.newTrigger.trtype;
                $scope.p.triggers.push(data);
                goProjectMode();
            }, function () { });
        }
        function goProjectMode() {
            $scope.mode = 'new';
            $scope.tabIndex = 0;
            $scope.newTrigger.trtype = '';
        }
    }])
    .controller('settings.projecttemp.add.ci.trigger.scheduleTriggerCtrl', ['$scope', '$q', function ($scope, $q) {
        $scope.ntr = {
            time: '', buildCondition: 'IfModificationExists', weekDays: ''
        };
        $scope.weekDays = { Sunday: false, Monday: false, Tuesday: false, Wednesday: false, Thursday: false, Friday: false, Saturday: false };
        $scope.$parent.newTrigger.virtualFunction = function () {
            var deferred = $q.defer();
            if ($scope.frmProject.$invalid) {
                deferred.reject();
            } else {
                deferred.resolve($scope.ntr);
            }
            return deferred.promise;
        }
        $scope.$watch('weekDays', function () {
            var weeks = [];
            _.each($scope.weekDays, function (v, i) {
                if (v) weeks.push(i);
            });
            $scope.ntr.weekDays = weeks.join(',');
        }, true);
    }])
    .controller('settings.projecttemp.add.ci.trigger.intervalTriggerCtrl', ['$scope', '$q', function ($scope, $q) {
        $scope.ntr = {
            buildCondition: 'IfModificationExists', initialSeconds: 0, seconds: 60
        };
        $scope.$parent.newTrigger.virtualFunction = function () {
            var deferred = $q.defer();
            if ($scope.frmProject.$invalid) {
                deferred.reject();
            } else {
                deferred.resolve($scope.ntr);
            }
            return deferred.promise;
        }
    }])
    .controller('settings.projecttemp.add.ci.trigger.cronTriggerCtrl', ['$scope', '$q', function ($scope, $q) {
        $scope.ntr = {
            buildCondition: 'IfModificationExists', cronExpression: ''
        };
        $scope.$parent.newTrigger.virtualFunction = function () {
            var deferred = $q.defer();
            if ($scope.frmProject.$invalid) {
                deferred.reject();
            } else {
                deferred.resolve($scope.ntr);
            }
            return deferred.promise;
        }
    }])
    .controller('settings.projecttemp.add.tasks', ['$scope', '$mdDialog', 'systemsettings.projecttempSvc', '$timeout', function ($scope, $mdDialog, $svc, $timeout) {
        $scope.tabIndex = 0;
        $scope.viewReady = false;
        $scope.taskspluginlist = [];
        $scope.selectedTasksPlugin = null;
        $scope.selectedTasksPluginArguments = [];
        $scope.newTasks = { arguments: {} };

        $svc.TasksPluginSet.get().then(function (res) {
            $scope.taskspluginlist = res.data;
            $scope.viewReady = true;
        });

        $scope.selectTaksPlugin = function (plugin) {
            $scope.selectedTasksPlugin = plugin;
            $scope.selectedTasksPluginArguments = JSON.parse(plugin.argument);
            $scope.tabIndex = 1;
        }

        $scope.getConfirmButtonName = function () {
            switch ($scope.tabIndex) {
                case 0:
                    return '플러그인 선택';
                case 1:
                    return '파라메터 설정';
                case 2:
                    return '태스크 등록';
                default:
                    return 'undefined'
            }
        }

        $scope.answer = function () {
            if ($scope.tabIndex == 1) {
                if ($scope.frmTasks.$invalid) return;
                $scope.tabIndex = 2;
            }
        }
        $scope.cancel = function (force) {
            if (force) {
                $mdDialog.cancel();
                return;
            }

            if ($scope.tabIndex == 1) {
                $scope.tabIndex = 0;
                $scope.selectedTasksPlugin = null;
                $scope.selectedTasksPluginArguments = [];
            }
        }
    }])
    ;
});