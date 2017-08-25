'use strict';
define(['angularAMD'], function (angularAMD) {
    var app = angular.module('app', ['ngAnimate', 'ngFileUpload', 'ngRoute', 'angularJsExtends', 'ngMessages'])
    .service('login.svc', ['$http', function ($http) {
        var svc = {
            SignIn: function (id, pw) {
                return $http.post('/Login/SignIn', { id: id, pw: pw });
            }
        }
        return svc;
    }])
    .controller('loginCtrl', ['$scope', 'login.svc', function ($scope, $svc) {
        $scope.prgsShow = false;
        $scope.user = { id: '', pw: '' };
        this.login = function () {
            if ($scope.frmLogin.$invalid) return;
            $scope.prgsShow = true;
            $svc.SignIn($scope.user.id, $scope.user.pw).then(function (res) {
                location.href = res.data;
            }, function () {
                $scope.prgsShow = false;
            });
        }
    }])
    ;
    return angularAMD.bootstrap(app);
});