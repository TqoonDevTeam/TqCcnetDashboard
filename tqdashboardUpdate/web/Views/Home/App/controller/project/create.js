define(['app', 'md5', 'urijs/URI',
    'controller/project/modules/svc',
    'controller/project/modules/step1',
    'controller/project/modules/step2',
    'controller/project/modules/step3',
    'controller/project/modules/step4'], function (app, md5, URI) {
        app.controller('project.create.ctrl', ['$scope', '$rootScope', '$http', '$uibModal', 'project.svc', 'pathUtil', '$location', function ($scope, $rootScope, $http, $uibModal, svc, pathUtil, $location) {
            $scope.p = {};
            $scope.activeTab = 0;
            $scope.step = [
                { heading: '기본정보', include: pathUtil.GetTemplate('project/step1/tmpl.html') },
                { heading: 'SourceControl', include: pathUtil.GetTemplate('project/step2/tmpl.html') },
                { heading: 'Triggers', include: pathUtil.GetTemplate('project/step3/tmpl.html') },
                { heading: 'Tasks', include: pathUtil.GetTemplate('project/step4/tmpl.html') },
            ];

            this.ok = function () {
                if ($scope.frmProject.$invalid) {
                    if ($scope.frmProject.name.$invalid) $scope.activeTab = 0;
                    return;
                }

                svc.CcnetProject.post($scope.p).then(function (res) {
                    $location.path('/project/list');
                });
            }

            this.init = function () {
                var query = $location.search();
                if (query.id) {
                    svc.CcnetProject.get(query.id || 0).then(function (res) {
                        angular.extend($scope.p, res.data.project);
                    });
                }
            }();
        }])
    });