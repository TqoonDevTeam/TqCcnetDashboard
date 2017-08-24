define(['app'], function (app) {
    app.service('systemsettings.svc', ['$http', 'svcUtil', function ($http, svcUtil) {
        var svc = {
            SystemSetting: svcUtil.create('SystemSetting', true)
                .$action('CheckEnvironmentVariable')
                .$action('GetServerCheckInformation')
                .$action('SystemUpdate')
                .$action('GetRemoteVersion')
                .$action('SetToken'),
        };
        return svc;
    }])
});