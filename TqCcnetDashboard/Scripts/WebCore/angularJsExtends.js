'use strict';
define(function () {
    /* angularJs 확장 함수 모음 */
    var angularJsExtends = angular.module('angularJsExtends', []);

    /* ====== XHR error handling ====== */
    (function (angularJsExtends) {
        var responseErrorRejectorFactory = ['$q', '$injector', '$rootScope', function ($q, $injector, $rootScope) {
            return {
                responseError: function (response) {
                    // config 설정에 따라 이 기능의 사용 여부처리
                    if (response.config.responseErrorRejector == false) return $q.reject(response);

                    switch (response.status) {
                        case 555:
                            alert(response.data.MessageDetail);
                            break;
                        case 556:
                            if (response.config.form) {
                                response.config.form.$submitted = true;
                                $rootScope.$broadcast('$http.error.556', { form: response.config.form, data: response.data });
                            }
                            break;
                        default:
                            alert(response.statusText);
                            break;
                    }
                    return $q.reject(response);
                }
            }
        }];
        angularJsExtends.factory('responseErrorRejector', responseErrorRejectorFactory).config(['$httpProvider', function ($httpProvider) {
            $httpProvider.interceptors.push('responseErrorRejector');
        }]);
    })(angularJsExtends);

    /* ====== functions ====== */
    /* underscore */
    (function (angularJsExtends) {
        var underscore = ['$rootScope', function ($rootScope) {
            if (!$rootScope['_']) {
                if (window._) {
                    window._.isImage = function (obj) {
                        if (obj === null) return false;
                        var IMG_FORMAT = "\.(bmp|gif|jpg|jpeg|png)$";
                        if (new RegExp(IMG_FORMAT, 'i').test(obj)) return true;
                        return false;
                    };

                    $rootScope['_'] = window._;
                }
            }
        }];
        angularJsExtends.run(underscore);
    })(angularJsExtends);

    /* $http extends */
    (function (angularJsExtends) {
        var httpExtends = ['$http', '$httpParamSerializerJQLike', function ($http, $httpParamSerializerJQLike) {
            if (!$http['postf']) {
                $http['postf'] = function (url, data, config) {
                    var defConfig = {
                        url: url, method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                        data: $httpParamSerializerJQLike(data, true),
                    };
                    if (!config) {
                        defConfig = angular.extend(defConfig, config);
                    }
                    return $http(defConfig);
                }
            }
        }];
        angularJsExtends.run(httpExtends);
    })(angularJsExtends);

    /* ====== services ====== */
    /* svcUtil */
    (function (angularJsExtends) {
        var svcUtilService = ['$http', function ($http) {
            var cache = {};
            return {
                create: function (controller, isNotApiController) {
                    var urlPrefix;
                    if (isNotApiController) urlPrefix = '/';
                    else urlPrefix = '/api/';
                    return {
                        url: urlPrefix + controller,
                        get: function (id, config) {
                            if (config === undefined) config = {};
                            if (id) {
                                if (_.isObject(id)) {
                                    config.params = _.extend(config.params || {}, id);
                                    return $http.get(this.url, config);
                                } else {
                                    return $http.get(this.url + '/' + id, config);
                                }
                            }
                            else return $http.get(this.url, config);
                        },
                        post: function (data, config) {
                            if (config === undefined) config = {};
                            var clone = angular.copy(data);
                            return $http.post(this.url, clone, config);
                        },
                        put: function (id, data, config) {
                            if (config === undefined) config = {};
                            var clone = angular.copy(data);
                            return $http.put(this.url + '/' + id, data, config);
                        },
                        postf: function (data, config) {
                            if (config === undefined) config = {};
                            var clone = angular.copy(data);
                            return $http.postf(this.url, clone, config);
                        },
                        'delete': function (id, config) {
                            if (config === undefined) config = {};
                            return $http.delete(this.url + '/' + id, null, config);
                        },
                        $action: function (route) {
                            this[route] = function (data, config) {
                                if (config === undefined) config = {};
                                var clone = angular.copy(data);
                                return $http.post(this.url + '/' + route, clone, config);
                            }
                            return this;
                        }
                    };
                }
            };
        }];
        angularJsExtends.service('svcUtil', svcUtilService);
    })(angularJsExtends);

    /* ====== filters ====== */
    (function (angularJsExtends) {
        var tqDate = ['$filter', function ($filter) {
            function GetConverted(value) {
                if (angular.isDate(value)) return Number(value);
                if (angular.isString(value)) {
                    if (value.indexOf("/Date") == 0) return Number(value.replace(/[^0-9]/g, ""));
                }
                if (angular.isObject(value)) {
                    if (value.Value != undefined) return GetConverted(value.Value);
                }
                return value;
            }

            return function (date, format, timezone) {
                var conv = GetConverted(date);
                conv = angular.isNumber(conv) ? $filter('date')(conv, format, timezone) : conv;
                if (conv == "2186-12-31") return "";
                return conv;
            }
        }];
        var sanitize = ['$sce', function ($sce) {
            return function (htmlCode) {
                return $sce.trustAsHtml(htmlCode);
            }
        }];
        angularJsExtends.filter('tqDate', tqDate).filter('sanitize', sanitize);
    })(angularJsExtends);

    /* ====== directives ====== */
    /* onEnter */
    (function (angularJsExtends) {
        function onEnterDirective() {
            return {
                restrict: 'A',
                link: onEnterLink
            };
        }
        function onEnterLink(scope, element, attrs) {
            element.bind('keydown keypress', function (event) {
                if (event.which === 13) {
                    scope.$apply(function () {
                        scope.$eval(attrs.onEnter);
                    });
                    event.preventDefault();
                }
            });
        }
        angularJsExtends.directive('onEnter', onEnterDirective);
    })(angularJsExtends);
    /* formCheck */
    (function (angularJsExtends) {
        function formCheckDirective() {
            return {
                restrict: 'A',
                priority: -1,
                link: formCheckLink
            };
        }
        function formCheckLink(scope, element, attrs) {
            var $el = angular.element(element);
            var formName = attrs.name;

            scope.$watch(formName + '.$submitted', function (newval) {
                error(getErrorNames())
            });
            scope.$watchCollection(getErrorNames, function (newval) {
                error(newval);
            });

            function getErrorNames() {
                var names = [];
                var error;
                for (var prop in scope[formName].$error) {
                    for (var i = 0; i < scope[formName].$error[prop].length; i++) {
                        error = scope[formName].$error[prop][i];
                        if (names.indexOf(error.$name) === -1) names.push(error.$name);
                    }
                }
                return names;
            }
            function error(names) {
                $el.find('.has-error').removeClass('has-error');
                var name, $element;
                for (var i = 0; i < names.length; i++) {
                    name = names[i];
                    if (scope[formName].$submitted || scope[formName][name].$$element[0].hasAttribute('form-check-force')) {
                        $(scope[formName][name].$$element).closest('.form-group').addClass('has-error');
                    }
                }
            }
        }
        angularJsExtends.directive('formCheck', formCheckDirective);
    })(angularJsExtends);
    /* isEquals - formValidation */
    (function (angularJsExtends) {
        function isEqualsDirective() {
            return {
                restrict: 'A',
                require: 'ngModel',
                scope: { isEquals: '=' },
                link: isEqualsLink
            };
        }
        function isEqualsLink(scope, element, attrs, ctrl) {
            if (!ctrl.$validators.isEquals) {
                ctrl.$validators.isEquals = function (modelValue) {
                    return modelValue === scope.isEquals;
                }
            }
        }
        angularJsExtends.directive('isEquals', isEqualsDirective);
    })(angularJsExtends);
    /* check556 - formValidation */
    (function (angularJsExtends) {
        function check556Directive() {
            return {
                restrict: 'A',
                require: ['ngModel', '^form'],
                link: check556Link
            };
        }
        function check556Link(scope, element, attrs, ctrl) {
            var ngModel = ctrl[0], frm = ctrl[1];
            var errors = {}; // { 'errorKind' : [ /* values */ ] }
            if (!ngModel.$validators.check556) {
                ngModel.$validators.check556 = function (modelValue) {
                    for (var prop in errors) {
                        if (errors[prop].indexOf(modelValue) !== -1) {
                            ngModel.$setValidity('check556_' + prop, false);
                        } else {
                            ngModel.$setValidity('check556_' + prop, true);
                        }
                    }
                    return true;
                };;
            }

            var errorKind, errorValue;
            scope.$on('$http.error.556', function (e, data) {
                if (data.form == frm && data.data.Message == ngModel.$name) {
                    errorKind = data.data.MessageDetail;
                    errorValue = ngModel.$viewValue;
                    ngModel.$setValidity('check556', false);
                    ngModel.$setValidity('check556_' + errorKind, false);
                    if (errors[errorKind] === undefined) errors[errorKind] = [];
                    if (errors[errorKind].indexOf(errorValue) === -1) {
                        errors[errorKind].push(errorValue);
                    }
                }
            });
        }
        angularJsExtends.directive('check556', check556Directive);
    })(angularJsExtends);
});