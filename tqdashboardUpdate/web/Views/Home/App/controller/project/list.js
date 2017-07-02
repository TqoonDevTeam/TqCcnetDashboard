define(['app',
    'controller/project/modules/svc'
], function (app) {
    app.controller('project.list.ctrl', ['$scope', '$http', 'project.svc', '$location', function ($scope, $http, svc, $location) {
        $scope.searchResult = [];
        $scope.searchValue = { s: '' };

        function resetSearchValue() {
            var searchObject = $location.search();
            _.each($scope.searchValue, function (v, p) {
                $scope.searchValue[p] = searchObject[p] || $scope.searchValue[p];
            });
        }
        function search() {
            svc.CcnetProject.get($scope.searchValue).then(function (res) {
                $scope.searchResult = res.data;
            });
        }

        this.getDesc = function (item) {
            return item['<Description>k__BackingField'];
        }

        this.onSearch = function () {
            $location.search($scope.searchValue);
        }
        this.onTmplModClick = function (item) {
            $location.path('/project/update').search({ id: item.name });
        }
        this.onTmplDelClick = function (item) {
            if (!confirm('템플릿을 삭제 하시겠습니까?')) return;
            svc.CcnetProject.delete(item.name).then(function (res) {
                search();
            });
        }
        this.onTmplDupClick = function (item) {
            $location.path('/project/create').search({ id: item.name });
        }

        this.init = function () {
            resetSearchValue();
            search();
        }();
    }]);
});