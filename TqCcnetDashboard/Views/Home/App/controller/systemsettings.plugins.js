define(['app'], function (app) {
    app.service('systemsettings.plugins.svc', ['$rootScope', '$http', '$q', 'svcUtil', function ($rootScope, $http, $q, svcUtil) {
        var svc = {
            OwnedPlugin: svcUtil.create('OwnedPlugin'),
            TasksPluginSet: svcUtil.create('TasksPluginSet'),
            ViewReady: function () {
                return $q.all([this.OwnedPlugin.get(), this.TasksPluginSet.get()]).then(function (res) {
                    $rootScope._view.ready = true;
                    return {
                        ownedPlugins: res[0].data,
                        installPlugins: res[1].data
                    };
                });
            }
        };
        return svc;
    }])
    .controller('systemsettings.plugins.ctrl', ['$scope', '$http', 'systemsettings.plugins.svc', function ($scope, $http, svc) {
        $scope.ownedPlugins = [];
        $scope.installPlugins = [];

        svc.ViewReady().then(function (res) {
            $scope.ownedPlugins = res.ownedPlugins;
            $scope.installPlugins = res.installPlugins;
        });

        this.addPlugin = function (item) {
            svc.TasksPluginSet.postf(item.fullName).then(refrehInstallPlugins);
        }
        this.delPlugin = function (item) {
            svc.TasksPluginSet.del(item.id).then(refrehInstallPlugins);
        }

        function refrehInstallPlugins() {
            return svc.TasksPluginSet.get().then(function (res) {
                $scope.installPlugins = res.data;
                return res;
            });
        }
    }]);
});