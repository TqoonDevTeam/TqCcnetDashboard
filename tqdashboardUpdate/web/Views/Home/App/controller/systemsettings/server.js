define(['app',
    'controller/systemsettings/modules/svc'], function (app) {
        app.controller('systemsettings.server.ctrl', ['$scope', '$rootScope', 'systemsettings.svc', function ($scope, $rootScope, svc) {
            $scope.info = {};
            $scope.remoteVersion = '';
            $scope.msgs = [];
            $scope.systemUpdateBusy = false;
            $rootScope.$watch('_SystemUpdate', function (newVal) {
                if (newVal && newVal.e) {
                    $scope.systemUpdateBusy = newVal.busy;
                    var find = _.find($scope.msgs, function (item) {
                        return item.e.Desc === newVal.e.Desc;
                    });
                    if (find) {
                        find.busy = newVal.busy;
                        find.e = newVal.e;
                    } else {
                        $scope.msgs.push(_.clone(newVal));
                    }
                }
            });
            this.systemUpdate = function () {
                if (!$scope.systemUpdateBusy) {
                    if (confirm('업데이트 하시겠습니까?')) {
                        $scope.systemUpdateBusy = true;
                        $scope.msgs = [];
                        svc.SystemSetting.SystemUpdate();
                    }
                }
            }
            this.gittokenSave = function () {
                if (confirm('토큰을 저장 하시겠습니까?')) {
                    svc.SystemSetting.SetToken({ key: 'gittoken', value: $scope.info.gittoken });
                }
            }
            this.init = function () {
                svc.SystemSetting.GetServerCheckInformation().then(function (res) {
                    $scope.info = res.data;
                });
                svc.SystemSetting.GetRemoteVersion().then(function (res) {
                    $scope.remoteVersion = res.data;
                });
            }();
        }]);
    });