const DEFAULT_ERROR_MESSAGE = "Nepoznata greska." 

class ApiError extends Error {
    constructor(status, errors) {
        super(errors[0] ?? DEFAULT_ERROR_MESSAGE);
        this.name = "ApiError";
        this.status = status;
        this.errors = errors;
    }
}

function extractErrorMessages(data) {
    if (!data) {
        return [DEFAULT_ERROR_MESSAGE];
    }
    if (Array.isArray(data.errors)) {
        return data.errors;
    }
    if (data.errors && typeof data.errors === "object") {
        return Object.values(data.errors).flat();
    }
    if (data.message) {
        return [data.message];
    }
    return [DEFAULT_ERROR_MESSAGE];
}