define(['app'], function (app) {
    app.service('project.svc', ['$http', 'svcUtil', function ($http, svcUtil) {
        var svc = {
            CcnetProject: svcUtil.create('CcnetProject'),
            SourceControlTemplate: svcUtil.create('SourceControlTemplate'),
            OwnedInnerTask: svcUtil.create('OwnedInnerTask'),
            GitHub: svcUtil.create('GitHub', true).$action('CheckRepository'),
            SvnHub: svcUtil.create('SvnHub', true).$action('CheckRepository')
        };
        return svc;
    }])
});