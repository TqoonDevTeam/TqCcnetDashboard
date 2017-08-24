'use strict';
define(function () {
    var utils = angular.module('Home.App.Utils', []);
    /* ====== services ====== */
    /* pathUtil */
    (function pathUtil(utils) {
        function pathUtilService() {
            return {
                Combine: function (paths) {
                    var path = [];
                    _.each(paths, function (v) {
                        if (!v) return;
                        if (v.startsWith('/')) v = v.substring(1);
                        if (v.endsWith('/')) v = v.substring(0, v.length - 1);
                        path.push(v);
                    });
                    return path.join('/');
                },
                GetDirectoryName: function (path) {
                    var split = path.split('.');
                    return _.initial(split, 1).join('.');
                },
                GetFileName: function (path) {
                    var split = path.split('/');
                    return split[split.length - 1];
                },
                GetTemplate: function (name) {
                    return '/Views/Home/App/views/' + name;
                },
                GetCustomTaskJsPath: function (name) {
                    return '/Views/Home/App/controller/project/modules/tasks/' + name + '.js';
                }
            };
        }
        utils.service('pathUtil', pathUtilService);
    })(utils);
    /* dirView */
    (function dirView(utils) {
        utils.service('dirView.svc', ['$http', 'svcUtil', function ($http, svcUtil) {
            var svc = {
                DirView: svcUtil.create('DirView', true).$action('Search')
            };
            return svc;
        }]);
        var dirViewDirective = [function () {
            return {
                restrict: 'E',
                replace: true,
                require: '?ngModel',
                templateUrl: '/Views/Home/App/utils/dirView.tmpl.html',
                scope: { ngModel: '=?', currentDir: '=?' },
                controller: dirViewController,
                controllerAs: 'ctrl',
                link: dirViewLink
            };
        }];
        var dirViewController = ['$scope', 'dirView.svc', function ($scope, svc) {
            $scope.dirPath = [];
            this.ngClass = function (item) {
                var css = [];
                if ($scope.ngModel === item) css.push('list-group-item-success');
                if (item.mime !== 'DIRECTORY') {
                    if ($scope.selFilter) {
                        if (!$scope.selFilter.test(item.name)) css.push('disabled');
                    }
                }
                return css.join(' ');
            };
            this.onDirChange = function (item) {
                search(item.path);
            }
            this.onItemClick = function (item) {
                if (item.mime === 'DIRECTORY') {
                    search(item.path).then(function () {
                        $scope.ngModelChange(item);
                    });
                } else {
                    $scope.ngModelChange(item);
                }
            }
            function resetDirPath() {
                if (!$scope.currentDir.path) {
                    $scope.dirPath = [{ name: '..', path: '' }];
                    return;
                }

                var path = [];
                var split = $scope.currentDir.path.split('/');
                for (var i = 0; i < split.length; i++) {
                    if (i === 0) path.push({ name: '..', path: '' });
                    else path.push({ name: split[i], path: _.initial(split, split.length - (i + 1)).join('/') });
                }
                $scope.dirPath = path;
            }
            function search(path) {
                return svc.DirView.Search({ path: path }).then(function (res) {
                    $scope.currentDir = res.data;
                    resetDirPath();
                    return res;
                });
            }
            search('');
        }];
        var dirViewLink = function (scope, element, attrs, ctrls) {
            if (attrs.selFilter) {
                scope.selFilter = new RegExp(attrs.selFilter);
            }
            scope.ngModelChange = function (item) {
                if (ctrls) {
                    ctrls.$setViewValue(item);
                } else {
                    scope.ngModel = item;
                }
            }
        }
        utils.directive('dirView', dirViewDirective);
    })(utils);
    /* fileSelector*/
    (function (utils) {
        var fileSelectorService = ['$uibModal', function ($uibModal) {
            return {
                open: function (resolve) {
                    return $uibModal.open({
                        templateUrl: '/Views/Home/App/utils/fileSelector.tmpl.html',
                        controller: 'fileSelector.ctrl',
                        controllerAs: 'ctrl',
                        resolve: {
                            items: function () {
                                return resolve;
                            }
                        }
                    }).result;
                }
            };
        }];
        var fileSelectorController = ['$scope', 'items', '$uibModalInstance', function ($scope, items, $uibModalInstance) {
            $scope.suggestList = items.suggestList;
            $scope.filter = items.filter;
            $scope.selected = {};
            $scope.dir = {};
            this.onSuggestClick = function (item) {
                $scope.selected = item;
            }
            this.ngClass = function (item) {
                if ($scope.selected === item) return 'list-group-item-success';
            };
            this.ok = function () {
                if (!_.isEmpty($scope.selected)) {
                    if ($scope.selected !== 'DIRECTORY') {
                        if ($scope.filter) {
                            var regex = new RegExp($scope.filter);
                            if (regex.test($scope.selected.name)) {
                                $uibModalInstance.close($scope.selected);
                            }
                        }
                    }
                }
            }
            this.cancel = function () {
                $uibModalInstance.dismiss();
            }
        }]
        utils.controller('fileSelector.ctrl', fileSelectorController);
        utils.service('fileSelector', null);
    })(utils);

    /* ====== directives ====== */
    /* reflectorType */
    (function reflectorType(utils) {
        function reflectorTypeDirective() {
            return {
                restrict: 'E',
                templateUrl: '/Views/Home/App/utils/reflectorType.tmpl.html',
                scope: {
                    typeSource: '=',
                    model: '='
                },
                controller: reflectorTypeController,
                controllerAs: 'ctrl'
            }
        }
        var reflectorTypeController = ['$templateCache', function ($templateCache) {
            this.getSrc = function (item) {
                var url = 'reflectorTypeDirective.' + item.propTypeName + '.html';
                if ($templateCache.get(url)) {
                    return url;
                } else {
                    return 'reflectorTypeDirective.default.html';
                }
            }
        }];
        utils.directive('reflectorType', reflectorTypeDirective);
    })(utils);
    /* isWorkingDirectory - formValidation */
    (function isWorkingDirectory(utils) {
        function isWorkingDirectoryDirective() {
            return {
                restrict: 'A',
                require: 'ngModel',
                link: isWorkingDirectoryLink
            };
        }
        function isWorkingDirectoryLink(scope, element, attrs, ctrl) {
            if (!ctrl.$validators.isWorkingDirectory) {
                ctrl.$validators.isWorkingDirectory = function (modelValue) {
                    if (modelValue) {
                        if (modelValue.startsWith('/')) return false;
                        if (modelValue.indexOf('../') !== -1) return false;
                        if (modelValue.indexOf('./') !== -1) return false;
                    }
                    return true;
                }
            }
        }
        utils.directive('isWorkingDirectory', isWorkingDirectoryDirective)
    })(utils);
    /* scSelector */
    (function scSelector(utils) {
        var scSelectorDirective = [function () {
            return {
                restrict: 'E',
                replace: true,
                require: 'ngModel',
                templateUrl: '/Views/Home/App/utils/scSelector.tmpl.html',
                scope: {
                    ngModel: '=',
                    source: '=',
                    selected: '='
                },
                controller: scSelectorController,
                controllerAs: 'ctrl',
                link: scSelectorLink
            };
        }];
        var scSelectorController = ['$scope', function ($scope) {
            this.onScSelected = function (sc) {
                $scope.selected = sc;
                $scope.ngModelChange(sc.$MD5);
            }
            this.hasSelect = function () {
                return !_.isEmpty($scope.selected);
            };
            var find = _.findWhere($scope.source, { "$MD5": $scope.ngModel });
            if (find) {
                $scope.selected = find;
            } else {
                $scope.selected = {};
            }
        }];
        var scSelectorLink = function (scope, element, attrs, ctrls) {
            scope.ngModelChange = function (md5) {
                if (ctrls) {
                    ctrls.$setViewValue(md5);
                }
            }
        };
        utils.directive('scSelector', scSelectorDirective)
    })(utils);
    /* taskSelector */
    (function taskSelector(utils) {
        var taskSelectorDirective = [function () {
            return {
                restrict: 'E',
                replace: true,
                require: 'ngModel',
                templateUrl: '/Views/Home/App/utils/taskSelector.tmpl.html',
                scope: {
                    ngModel: '=',
                    source: '=',
                    selected: '=',
                    typeFilter: '@',
                },
                controller: taskSelectorController,
                controllerAs: 'ctrl',
                link: taskSelectorLink
            };
        }];
        var taskSelectorController = ['$scope', function ($scope) {
            this.onScSelected = function (task) {
                $scope.selected = task;
                $scope.ngModelChange(task.$MD5);
            }
            this.hasSelect = function () {
                return !_.isEmpty($scope.selected);
            };
            this.getSource = function () {
                if (scope.typeFilter) {
                    var split = scope.typeFilter.split(',');
                    return _.filter($scope.source, function (task) {
                        return _.contains(split, task.type);
                    });
                } else {
                    return $scope.source;
                }
            }

            var find = _.findWhere($scope.source, { "$MD5": $scope.ngModel });
            if (find) {
                $scope.selected = find;
            } else {
                $scope.selected = {};
            }
        }];
        var taskSelectorLink = function (scope, element, attrs, ctrls) {
            scope.ngModelChange = function (md5) {
                if (ctrls) {
                    ctrls.$setViewValue(md5);
                }
            }
        };
        utils.directive('taskSelector', taskSelectorDirective)
    })(utils);
    /* dynamicController */
    (function dynamicCtrl(utils) {
        function dynamicCtrlDirective($compile, $parse) {
            return {
                restrict: 'A',
                terminal: true,
                priority: 100000,
                link: function (scope, element, attrs) {
                    if (element.ngController) throw 'already has controller. ' + attrs.ngController;
                    scope.dynamicCtrl.compile = function (ctrlName) {
                        element.removeAttr('dynamic-ctrl');
                        element.attr('ng-controller', ctrlName);
                        $compile(element)(scope);
                        scope.custom.init();
                    }
                }
            }
        }
        utils.directive('dynamicCtrl', ['$compile', '$parse', dynamicCtrlDirective]);
    })(utils);
});
define('guid', function () {
    function s4() {
        return Math.floor((1 + Math.random()) * 0x10000).toString(16).substring(1);
    }
    return function (hasDash) {
        var guid = [
            s4() + s4(),
            s4(),
            s4(),
            s4(),
            s4() + s4() + s4()
        ];
        if (hasDash === true) {
            return guid.join('-');
        } else {
            return guid.join('');
        }
    };
});
define('projectdataChecker', ['underscore'], function (_) {
    var checker = {
        checkAndFix: function (project) {
            if (project.triggers) {
                if (_.isArray(project.triggers)) {
                    _.each(project.triggers, function (v) { checkTrigger(v); });
                }
            }
        }
    };

    function checkTrigger(data) {
        var type = data['@type'];
        if (type === 'scheduleTrigger') {
            if (!_.isArray(data.weekDays)) {
                if (data.weekDays.string) {
                    data.weekDays.string = [data.weekDays.string];
                }
            }
        } else if (type === 'multiTrigger') {
            _.each(data.triggers, function (v) { checkTrigger(v); });
        }
    }

    return checker;
});