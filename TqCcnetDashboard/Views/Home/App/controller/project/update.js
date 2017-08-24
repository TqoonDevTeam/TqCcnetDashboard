define(['app', 'md5', 'urijs/URI',
    'projectdataChecker',
    'controller/project/modules/svc',
    'controller/project/modules/step1',
    'controller/project/modules/step2',
    'controller/project/modules/step3',
    'controller/project/modules/step4',
    'controller/project/modules/step5'], function (app, md5, URI, checker) {
        app.controller('project.update.ctrl', ['$scope', '$rootScope', '$http', '$uibModal', 'project.svc', 'pathUtil', '$location', function ($scope, $rootScope, $http, $uibModal, svc, pathUtil, $location) {
            var projectName = '';
            $scope.p = {};
            $scope.activeTab = 0;
            $scope.step = [
                { heading: 'Default', include: pathUtil.GetTemplate('project/step1/tmpl.html') },
                { heading: 'SourceControl', include: pathUtil.GetTemplate('project/step2/tmpl.html') },
                { heading: 'Triggers', include: pathUtil.GetTemplate('project/step3/tmpl.html') },
                { heading: 'Tasks', include: pathUtil.GetTemplate('project/step4/tmpl.html') },
                { heading: 'Publishers', include: pathUtil.GetTemplate('project/step5/tmpl.html') },
            ];

            this.tabSelect = function (item) {
                $location.hash(item.heading)
            }
            this.ok = function () {
                if ($scope.frmProject.$invalid) {
                    if ($scope.frmProject.name.$invalid) $scope.activeTab = 0;
                    return;
                }
                svc.CcnetProject.put(projectName, $scope.p).then(function (res) {
                    $location.path('/project/list');
                });
            }

            this.init = function () {
                var query = $location.search();
                var hash = $location.hash() || 'Default';
                var find = _.findIndex($scope.step, { heading: hash });
                $scope.activeTab = find;
                svc.CcnetProject.get(query.id || 0).then(function (res) {
                    angular.extend($scope.p, res.data.project);
                    checker.checkAndFix($scope.p);
                    projectName = res.data.project.name;
                    $rootScope.$emit('svc.CcnetProject.get');
                });
            }();
        }])
    });