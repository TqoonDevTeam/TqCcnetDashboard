define(['app', 'md5', 'urijs/URI'], function (app, md5, URI) {
    app.controller('project.step1.ctrl', ['$scope', '$rootScope', function ($scope, $rootScope) {
        $scope.useQueue = false;

        $rootScope.$on('svc.CcnetProject.get', function () {
            checkQueue();
        });
        $scope.$watch('p.name', function () {
            checkQueue();
        });
        $scope.$watch('p.queue', function () {
            checkQueue();
        });

        function checkQueue() {
            if ($scope.p.name === $scope.p.queue) {
                $scope.useQueue = false;
                $scope.p.queuePriority = 0;
            } else {
                $scope.useQueue = true;
            }
        }

        this.useQueueChanged = function () {
            if (!$scope.useQueue) {
                $scope.p.queue = $scope.p.name;
                $scope.p.queuePriority = 0;
            }
        }
    }])
});