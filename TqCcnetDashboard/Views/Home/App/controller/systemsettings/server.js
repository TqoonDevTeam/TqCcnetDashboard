define(['app',
    'controller/systemsettings/modules/svc'], function (app) {
        app.controller('systemsettings.server.ctrl', ['$scope', '$rootScope', 'systemsettings.svc', 'Upload', function ($scope, $rootScope, svc, Upload) {
            $scope.info = {};
            $scope.remoteVersion = '';
            $scope.systemUpdating = false;
            $scope.systemUpdatingMsg = [];
            $scope.pluginUploading = false;
            $scope.uploadProcess = {};

            $rootScope.$on('system.msg.SystemUpdate', function (e, msg) {
                $scope.systemUpdatingMsg.push(msg);
                if (msg === 'WARN SystemUpdate Complete') {
                    location.reload(true);
                }
            });

            this.resetUpdate = function () {
                $rootScope._SystemUpdate.busy = false;
            }
            this.systemUpdate = function () {
                if (confirm('업데이트 하시겠습니까?')) {
                    $scope.systemUpdating = true;
                    $scope.systemUpdatingMsg = [];
                    svc.SystemSetting.SystemUpdate();
                }
            }
            this.pluginUpload = function (file) {
                if (!confirm('외부 플러그인을 설치 하시겠습니까?')) return;
                if ($scope.pluginUploading) return;
                $scope.pluginUploading = true;
                Upload.upload({
                    url: '/SystemSetting/PluginUpload',
                    data: { file: file }
                }).then(function (res) {
                    $scope.pluginUploading = false;
                    location.reload(true);
                }, function () { }, function (evt) {
                    $scope.uploadProcess.Desc = evt.config.data.file.name;
                    $scope.uploadProcess.ProgressPercentage = parseInt(100.0 * evt.loaded / evt.total);
                });
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