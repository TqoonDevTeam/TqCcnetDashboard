'use strict';
define(['angularAMD'], function (angularAMD) {
    var app = angular.module('app', ['ngAnimate', 'ngFileUpload', 'ngRoute', 'angularJsExtends', 'ngMessages'])
    .service('login.svc', ['$http', function ($http) {
        var svc = {
            login: function (id, pw) {
                return $http.post('/Login/SignIn', { id: id, pw: pw });
            },
            join: function (id, pw) {
                return $http.post('/Login/SignUp', { id: id, pw: pw });
            }
        }
        return svc;
    }])
    .controller('loginCtrl', ['$scope', 'login.svc', function ($scope, $svc) {
        $scope.prgsShow = false;
        $scope.mode = 'login';
        $scope.user = { id: '', pw: '' };
        console.log($scope);
        this.modeChange = function () {
            $scope.user.id = '';
            $scope.user.pw = '';
            $scope.user.pw2 = '';
            if ($scope.mode === 'login') $scope.mode = 'join';
            else $scope.mode = 'login';
        }
        this.login = function () {
            if ($scope.frmLogin.$invalid) return;
            $scope.prgsShow = true;
            $svc.login($scope.user.id, $scope.user.pw).then(function (res) {
                location.href = res.data;
            }, function () {
                $scope.prgsShow = false;
            });
        }
        this.join = function () {
            if ($scope.frmLogin.$invalid) return;
            $scope.prgsShow = true;
            $svc.join($scope.user.id, $scope.user.pw).then(function (res) {
                location.href = res.data;
            }, function () {
                $scope.prgsShow = false;
            });
        }
    }])
    ;
    return angularAMD.bootstrap(app);
});