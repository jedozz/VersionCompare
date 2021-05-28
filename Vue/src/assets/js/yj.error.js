//2021.05.07
function getMsg(message) {
    var msg;
    if (message instanceof Error) {
        msg = message.message;
    } else if (typeof message == 'string') {
        msg = message;
    }
    return msg;
}

class ApplicationError extends Error {
    constructor(message) {
        var msg = getMsg(message);
        super(msg);
        this.name = this.constructor.name;
    }
}
class NetError extends ApplicationError {
    constructor(message) {
        var msg = getMsg(message);
        super(msg);
        this.name = this.constructor.name;
    }
}
class HttpError extends ApplicationError {
    constructor(message, status) {
        var msg = getMsg(message);
        super(msg);
        this.name = this.constructor.name;
        this.status = status
    }
}
class WebApiError extends ApplicationError {
    constructor(message, errorCode) {
        var msg = getMsg(message);
        super(msg);
        this.name = this.constructor.name;
        this.errorCode = errorCode
    }
}
export { ApplicationError, HttpError, NetError, WebApiError }
