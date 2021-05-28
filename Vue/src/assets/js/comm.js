import axios from 'axios'
import lang from '@assets/js/language.js'
import config from '@/appconfig.json'
import { HttpError, NetError, WebApiError } from '@assets/js/yj.error.js'
import '@assets/js/yj.extend.js'
import qs from "qs"

//浏览器测试的时候，vue.config 配置了devServer.proxy， api使用相对地址才能使用代理
var urlBase = process.env.NODE_ENV == 'production' ? config.urlBase[config.language] : "";
var apiBase = urlBase + config.apiUrl;
console.log("api地址:" + apiBase);

var defaultRequestOpts = function () {
    return {
        headers: {}
    }
}

function consoleLog(msg) {
    console.log(msg)
}

function getRequest(url, requestOpts) {
    requestOpts = { ...defaultRequestOpts(), ...requestOpts };
    return new Promise((resolve, reject) => {
        console.log(apiBase + url);
        axios.get(apiBase + url, requestOpts).then(function (res) {
            consoleLog('get请求：' + apiBase + url + "  成功")
            consoleLog(res.data)
            if (res.data.Success) {
                resolve(res.data.Data)
            } else {
                reject(createErrorFromApiRes(res.data))
            }
        }).catch((err) => {
            consoleLog('get请求：' + apiBase + url + "  出错")
            reject(createErrorFromAxios(err));
        })
    })
}

function postRequest(url, params, requestOpts) {
    requestOpts = { ...defaultRequestOpts(), ...requestOpts };
    var useFormData = typeof params != 'object' && params != null;
    if (useFormData) {
        requestOpts.headers = { ...requestOpts.headers, ...{ 'content-type': 'application/x-www-form-urlencoded' } }
    }
    return new Promise((resolve, reject) => {
        var submitData = useFormData ? qs.stringify({ "": params }) : params;
        axios.post(apiBase + url, submitData, requestOpts).then(function (res) {
            consoleLog('post请求：' + apiBase + url + "  成功")
            if (useFormData) {
                consoleLog('post原始数据:');
                consoleLog(params);
            }
            consoleLog('post实际提交数据:');
            consoleLog(submitData);
            consoleLog(res.data);
            if (res.data.Success) {
                resolve(res.data.Data)
            } else {
                reject(createErrorFromApiRes(res.data))
            }
        }).catch((err) => {
            consoleLog('post请求：' + apiBase + url + "  出错")
            consoleLog(params)
            reject(createErrorFromAxios(err));
        })
    })
}

function createErrorFromAxios(axiosErr) {
    if (axiosErr.isAxiosError) {
        if (axiosErr.response == null) {
            return new NetError(lang.NetError + (axiosErr.code == null ? "" : " - " + axiosErr.code));
        }
        else {
            return new HttpError(axiosErr.response.data.Message == null ? axiosErr.response.data : axiosErr.response.data.Message, axiosErr.response.status);
        }
    } else {
        return axiosErr;
    }
}

//ErrorType, 0 Handled，1 Unhandled，2 HttpError, 3 UnknownErrorCode
function createErrorFromApiRes(apiRes) {
    var message = apiRes.ErrorMessage || (apiRes.ErrorType == 1 ? lang.UnkonwError : "")
    var err;
    switch (apiRes.ErrorType) {
        case 0:
            err = new WebApiError(message, apiRes.ErrorCode);
            break;
        case 1:
            err = new Error(message);
            break;
        case 2:
            err = new HttpError(message, apiRes.ErrorCode);
            break;
        default:
            err = new Error(message);
            break;
    }
    return err;
}

class comm {
    urlBase() {
        return urlBase;
    }
    apiBase() {
        return apiBase;
    }
    log(msg) {
        consoleLog(msg)
    }
    get(url, requestOpts) {
        return getRequest(url, requestOpts);
    }
    post(url, parms, requestOpts) {
        return postRequest(url, parms, requestOpts);
    }
}
export default new comm()