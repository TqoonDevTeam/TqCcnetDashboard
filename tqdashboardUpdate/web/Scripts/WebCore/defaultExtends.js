'use strict';
define(['underscore'], function () {
    /* 전역 유틸리티 입니다.*/
    // 배열에서 해당 인덱스에 전달된 값을 삽입한다.
    Array.prototype.insert = function (index, item) {
        if (_.isArray(item)) {
            var idx = 0;
            var that = this;
            _.each(array, function (item) {
                that.splice(index + idx, 0, item);
                idx++;
            });
        } else {
            this.splice(index, 0, item);
        }
    }
    if (typeof Array.prototype.clear !== 'function') {
        Array.prototype.clear = function () {
            while (this.length) {
                this.pop();
            }
        }
    }
    // 배열에서 해당 인덱스 또는 오브젝트의 값을 제거 한다.
    Array.prototype.removeAt = function (index) {
        var type = typeof index;
        if (type == "number") {
            this.splice(index, 1);
        }
        else if (type == "object") {
            index = this.indexOf(index);
            if (index > -1) this.splice(index, 1);
        }
    }
    if (!Array.prototype.remove) {
        Array.prototype.remove = function (obj) {
            var type = typeof obj;
            if (type === "object" || type === "string") {
                var index = this.indexOf(obj);
                if (index > -1) this.splice(index, 1);
            }
        }
    }
    Array.prototype.removeAt = function (index) {
        var type = typeof index;
        if (type == "number") {
            this.splice(index, 1);
        }
        else if (type == "object") {
            index = this.indexOf(index);
            if (index > -1) this.splice(index, 1);
        }
    }
    // 지정된 길이 이상의 텍스트를 감춘다.(... 로 대체한다.)
    String.prototype.abbr = function (n) {
        return (this.length > n) ? this.substr(0, n - 1) + '&hellip;' : this;
    }
    // Url 호출시 브라우저 캐시를 회피 하기 위해 힌트 파라메터를 추가한다.
    String.prototype.tqNocache = function () {
        var str = this.toString();
        var time = Number(new Date());
        if (str.indexOf("?") > -1) {
            return str + "&.=" + time;
        } else {
            return str + "?.=" + time;
        }
    }
    // endsWith UPDATE (Nov 24th, 2015)
    if (typeof String.prototype.endsWith !== 'function') {
        String.prototype.endsWith = function (suffix) {
            return this.indexOf(suffix, this.length - suffix.length) !== -1;
        };
    }
    // startsWith UPDATE (Nov 24th, 2015)
    if (!String.prototype.startsWith) {
        String.prototype.startsWith = function (searchString, position) {
            position = position || 0;
            return this.substr(position, searchString.length) === searchString;
        };
    }
    // Left 함수
    if (!String.prototype.left) {
        String.prototype.left = function (length) {
            return this.substr(0, length);
        };
    }
    // Right 함수
    if (!String.prototype.right) {
        String.prototype.right = function (length) {
            var start = this.length - length;
            return this.substr(start < 0 ? 0 : start);
        };
    }
});