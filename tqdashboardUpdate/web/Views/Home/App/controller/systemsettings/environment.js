define(['app',
    'controller/systemsettings/modules/svc'], function (app) {
        app.controller('systemsettings.environment.ctrl', ['$scope', 'systemsettings.svc', 'CheckEnvironmentVariable', function ($scope, svc, CheckEnvironmentVariable) {
            $scope.CheckEnvironmentVariable = CheckEnvironmentVariable;
        }]);
    });