import { ERROR_MESSAGES } from "./error-messages.js";

export class ApiError extends Error {
    constructor(status, errors) {
        super(errors[0] ?? ERROR_MESSAGES.unknown);
        this.name = "ApiError";
        this.status = status;
        this.errors = errors;
    }
}

export function extractErrorMessages(data) {
    if (!data) {
        return [ERROR_MESSAGES.unknown];
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
    return [ERROR_MESSAGES.unknown];
}