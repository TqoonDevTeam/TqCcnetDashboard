define(['app'], function (app) {
    app.service('dashboard.svc', ['$http', 'svcUtil', function ($http, svcUtil) {
        var svc = {
            Dashboard: svcUtil.create('Dashboard', true).$action('GetProjectStatus').$action('ForceBuild').$action('AbortBuild')
        };
        return svc;
    }])
});