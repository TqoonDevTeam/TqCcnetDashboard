'use strict';
define(['angularAMD', 'json!/SystemSetting/CheckEnvironmentVariable'], function (angularAMD, CheckEnvironmentVariableJson) {
    var app = angular.module('app', ['ngAnimate', 'ngFileUpload', 'ngRoute', 'angularJsExtends', 'ui.bootstrap', 'ngMessages', 'Home.App.Utils', 'ngCookies', 'ui.select', 'ngSanitize'])
    .constant('CheckEnvironmentVariable', CheckEnvironmentVariableJson)
    .config(['$uibTooltipProvider', function ($uibTooltipProvider) {
        $uibTooltipProvider.options({
            appendToBody: true
        });
    }])
    .config(['$routeProvider', function ($routeProvider) {
        var appBase = '/Views/Home/App/views/';
        $routeProvider.when('/dashboard', angularAMD.route({
            templateUrl: appBase + 'dashboard/dashboard.html',
            controller: 'dashboard.ctrl as ctrl',
            controllerUrl: 'controller/dashboard/dashboard',
            $data: { title: '대시보드' }
        }))
        .when('/project/create', angularAMD.route({
            templateUrl: appBase + 'project/create.html',
            controller: 'project.create.ctrl as ctrl',
            controllerUrl: 'controller/project/create',
            $data: { title: '프로젝트 - 생성' }
        }))
        .when('/project/update', angularAMD.route({
            templateUrl: appBase + 'project/update.html',
            controller: 'project.update.ctrl as ctrl',
            controllerUrl: 'controller/project/update',
            reloadOnSearch: false,
            $data: { title: '프로젝트 - 수정' }
        }))
        .when('/project/list', angularAMD.route({
            templateUrl: appBase + 'project/list.html',
            controller: 'project.list.ctrl as ctrl',
            controllerUrl: 'controller/project/list',
            $data: { title: '프로젝트 - 리스트' }
        }))
        .when('/systemsettings/server', angularAMD.route({
            templateUrl: appBase + 'systemsettings/server.html',
            controller: 'systemsettings.server.ctrl as ctrl',
            controllerUrl: 'controller/systemsettings/server',
            $data: { title: '시스템설정 - Server' }
        }))
        .when('/systemsettings/environment', angularAMD.route({
            templateUrl: appBase + 'systemsettings/environment.html',
            controller: 'systemsettings.environment.ctrl as ctrl',
            controllerUrl: 'controller/systemsettings/environment',
            $data: { title: '시스템설정 - Environment' }
        }))
        .otherwise({ redirectTo: '/dashboard' });
        ;
    }])
    .run(['$rootScope', function ($rootScope) {
        $rootScope._ProjectStatus = {};
        $rootScope._SystemUpdate = {};
        // signalR
        var phub = $.connection.TqoonDevTeamMessageHub;
        function init() {
            phub.server.getAllProjectStatus().done(function (res) {
                extend_ProjectStatus(res);
            });
            phub.server.getSystemUpdate().done(function (res) {
                extend_SystemUpdate(res);
            });
        }
        function extend_ProjectStatus(res) {
            $rootScope.$apply(function () {
                angular.extend($rootScope._ProjectStatus, res);
                _.each($rootScope._ProjectStatus, function (v, p) {
                    _.each(v, function (item) {
                        item.$host = p;
                    });
                });
            });
        }
        function extend_SystemUpdate(res) {
            $rootScope.$apply(function () {
                $rootScope._SystemUpdate = res;
                $rootScope.$emit("system.msg.SystemUpdate", res.Msg);
            });
        }

        phub.client.updateProjectStatus = function (res) {
            extend_ProjectStatus(res);
        }
        phub.client.systemUpdate = function (res) {
            extend_SystemUpdate(res);
        }
        $.connection.hub.logging = true;
        $.connection.hub.start().done(init);
    }])
    .run(['$rootScope', '$route', '$cookies', '$location', function ($rootScope, $route, $cookies, $location) {
        $rootScope._cookie = { modeDebug: '0' };
        $rootScope._location = {};

        // global events handling
        $rootScope.$on('$routeChangeSuccess', function (evt, current, previos) {
            angular.element('title').text(current.$$route.$data.title || '');
            $rootScope._location.paths = $location.path().split('/');
            $('.dev-menu').find('a').removeClass('active');
            $('.dev-menu').find('a[href="#!' + $location.path() + '"]').addClass('active');
        });

        $rootScope.$watch('_cookie', function (newVal) {
            if (newVal) {
                _.each(newVal, function (v, p) {
                    $cookies.put('_cookie.' + p, v);
                }, true);
            }
        }, true);

        function init() {
            var c = $cookies.getAll();
            _.each(c, function (v, p) {
                if (p.startsWith('_cookie.')) {
                    $rootScope._cookie[p.replace('_cookie.', '')] = v
                }
            });
        } init();
    }])
    .controller('root.left.navbar.ctrl', ['$scope', 'CheckEnvironmentVariable', function ($scope, CheckEnvironmentVariable) {
        this.hasEnvironmentVariableError = function () {
            if (_.any(CheckEnvironmentVariable, function (v) { return v === false })) {
                return 'label-danger navbar-label-danger-fix';
            } else {
                return '';
            }
        }
    }])
    ;
    return angularAMD.bootstrap(app);
});