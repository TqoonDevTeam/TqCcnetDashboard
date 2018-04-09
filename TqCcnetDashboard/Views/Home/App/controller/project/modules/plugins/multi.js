define(['app'], function (app) {
    'use strict';
    app.controller('project.plugins.multi.customctrl', ['$scope', '$uibModal', 'pathUtil', function ($scope, $uibModal, pathUtil) {
        $scope.custom.attrs_force_show = [];
        $scope.custom.attrs_force_required = [];
        $scope.custom.template = {
            'query': [
                '<div class="col-sm-10">',
                    '<textarea class="form-control" name="query" ng-model="task.query" ng-required="item.attr.Required" style="min-height:150px"></textarea>',
                '</div>'].join(''),
            'sourceControls': [
                '<div class="col-sm-10">',
                    '<div class="row">',
                        '<div class="col-sm-12">',
                            '<button type="button" class="btn btn-success btn-sm" uib-tooltip="SourceControl 추가" ng-click="customCtrl.onScAddClick()"><i class="fa fa-plus"></i></button>',
                        '</div>',
                        '<div class="col-sm-12" style="margin-top:10px">',
                            '<div class="list-group">',
                                '<div class="list-group-item" ng-repeat="item in sc.sourceControls">',
                                    '<span class="label label-primary">{{item["@type"]}}</span>',
                                    '<span>{{customCtrl.getDesc(item)}}</span>',
                                    '<div class="pull-right">',
                                        '<button type="button" ng-click="customCtrl.onScModClick(item)" class="btn btn-xs btn-warning"><i class="fa fa-pencil"></i></button>',
                                        ' <button type="button" ng-click="customCtrl.onScDelClick(item)" class="btn btn-xs btn-danger"><i class="fa fa-times"></i></button>',
                                    '</div>',
                                '</div>',
                            '</div>',
                        '</div>',
                    '</div>',
                '</div>'].join('')
        };
        $scope.custom.defaultValue = {}
        $scope.custom.init = function () {
            $scope.sc.sourceControls = $scope.sc.sourceControls || [];
        }

        var sourcecontrolDesc = {
            'svn': function (item) {
                return item.trunkUrl;
            },
            'git': function (item) {
                return item.repository + ' ' + item.branch;
            },
            'multi': function (item) {
                var kv = {};
                var k;
                _.each(item.sourceControls, function (v) {
                    k = v['@type'];
                    kv[k] = (kv[k] || 0) + 1;
                });
                return _.map(kv, function (v, p) {
                    return p + '(' + v + ')';
                }).join(', ');
            }
        }

        this.getDesc = function (item) {
            return sourcecontrolDesc[item['@type']](item);
        }
        this.onScAddClick = function () {
            var instance = $uibModal.open({
                templateUrl: pathUtil.GetTemplate('/project/step2/sc.add.tmpl.html'),
                controller: 'project.step2.sc.add.ctrl',
                controllerAs: 'ctrl',
                scope: $scope,
                size: 'lg', backdrop: 'static',
                resolve: {
                    items: function () {
                        return {
                            mode: 'new'
                        };
                    }
                }
            });
            instance.result.then(function (item) {
                $scope.sc.sourceControls.push(item);
            }, function () { });
        }
        this.onScDelClick = function (item) {
            $scope.sc.sourceControls.remove(item);
        }
        this.onScModClick = function (item) {
            var instance = $uibModal.open({
                templateUrl: pathUtil.GetTemplate('/project/step2/sc.add.tmpl.html'),
                controller: 'project.step2.sc.add.ctrl',
                controllerAs: 'ctrl',
                scope: $scope,
                size: 'lg', backdrop: 'static',
                resolve: {
                    items: function () {
                        return {
                            mode: 'mod',
                            item: angular.copy(item)
                        };
                    }
                }
            });
            instance.result.then(function (sc) {
                var idx = _.indexOf($scope.sc.sourceControls, item);
                if (idx > -1) {
                    $scope.sc.sourceControls[idx] = sc;
                }
            }, function () { });
        }
    }]);
});