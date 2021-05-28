//2021.05.08
(function () {
    Array.prototype.find || (Array.prototype.find = function (fun) {
        for (var i = 0; i < this.length; i++) {
            if (fun.call(this, this[i], i, this) === true) {
                return this[i];
            }
        }
        return null;
    })

    Array.prototype.exists || (Array.prototype.exists = function (fun) {
        return this.find(fun) != null;
    })

    Array.prototype.findAll || (Array.prototype.findAll = function (fun) {
        var r = [];
        for (var i = 0; i < this.length; i++) {
            if (fun.call(this, this[i], i, this) === true) {
                r.push(this[i]);
            }
        }
        return r;
    })

    Array.prototype.remove || (Array.prototype.remove = function (fun) {
        for (var i = 0; i < this.length; i++) {
            if (fun.call(this, this[i], i, this) === true) {
                this.splice(i--, 1);
                break;
            }
        }
    })

    Array.prototype.removeAll || (Array.prototype.removeAll = function () {
        this.splice(0, this.length);
    })

    Array.prototype.forEach || (Array.prototype.forEach = function (fun) {
        for (var i = 0; i < this.length; i++) {
            fun.call(this, this[i], i, this);
        }
    })

    Array.prototype.clear || (Array.prototype.clear = function () {
        this.length = 0;
    })

    Array.prototype.sum || (Array.prototype.sum = function (fun) {
        var r = 0;
        for (var i = 0; i < this.length; i++) {
            r += fun.call(this, this[i], i, this);
        }
        return r;
    })

    Array.prototype.select || (Array.prototype.select = function (fun) {
        var r = [];
        for (var i = 0; i < this.length; i++) {
            r.push(fun.call(this, this[i], i, this));
        }
        return r;
    })

    String.prototype.splice = function (start, newStr) {
        return this.slice(0, start) + newStr + this.slice(start);
    };

    Promise.prototype.finally = function (callback) {
        let P = this.constructor;
        return this.then(
            value => P.resolve(callback()).then(() => value),
            reason => P.resolve(callback()).then(() => { throw reason })
        );
    };

    //Date
    Date.prototype.format = function(fmt) {
        var o = {
            "M+": this.getMonth() + 1, //月份           
            "d+": this.getDate(), //日           
            "h+": this.getHours() % 12 == 0 ? 12 : this.getHours() % 12, //小时           
            "H+": this.getHours(), //小时           
            "m+": this.getMinutes(), //分           
            "s+": this.getSeconds(), //秒           
            "q+": Math.floor((this.getMonth() + 3) / 3), //季度           
            "S": this.getMilliseconds() //毫秒           
        };
        var week = {
            "0": "/u65e5",
            "1": "/u4e00",
            "2": "/u4e8c",
            "3": "/u4e09",
            "4": "/u56db",
            "5": "/u4e94",
            "6": "/u516d"
        };
        if(/(y+)/.test(fmt)) {
            fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
        }
        if(/(E+)/.test(fmt)) {
            fmt = fmt.replace(RegExp.$1, ((RegExp.$1.length > 1) ? (RegExp.$1.length > 2 ? "/u661f/u671f" : "/u5468") : "") + week[this.getDay() + ""]);
        }
        for(var k in o) {
            if(new RegExp("(" + k + ")").test(fmt)) {
                fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
            }
        }
        return fmt;
    }
    
    Date.convertFromDotNet = function(dateStr) {
        dateStr = dateStr.replace(/\//g, '');
        var date = eval(dateStr);
        return new Date(date);
    }
})()