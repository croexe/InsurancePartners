const API_BASE_URL = "https://localhost:7146/api";
const REQUEST_TIMEOUT_MS = 15000;

class ApiError extends Error {
    constructor(status, errors) {
        super(errors[0] ?? "Nepoznata greška.");
        this.name = "ApiError";
        this.status = status;
        this.errors = errors;
    }
}

function buildHeaders(hasBody) {
    const headers = new Headers();
    if (hasBody) {
        headers.set("Content-Type", "application/json");
    }
    const token = auth.getToken();
    if (token) {
        headers.set("Authorization", `Bearer ${token}`);
    }
    return headers;
}

async function parseResponseBody(response) {
    if (response.status === 204) {
        return null;
    }
    const contentType = response.headers.get("content-type") ?? "";
    if (contentType.includes("application/json")) {
        return response.json().catch(() => null);
    }
    return response.text();
}

function extractErrorMessages(data) {
    if (!data) {
        return ["Nepoznata greška."];
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
    return ["Nepoznata greška."];
}

async function apiRequest(method, path, body) {
    const hasBody = body !== undefined && body !== null;

    const request = new Request(`${API_BASE_URL}${path}`, {
        method,
        headers: buildHeaders(hasBody),
        body: hasBody ? JSON.stringify(body) : undefined,
        mode: "cors",
        signal: AbortSignal.timeout(REQUEST_TIMEOUT_MS)
    });

    let response;
    try {
        response = await fetch(request);
    } catch (error) {
        if (error.name === "TimeoutError") {
            throw new ApiError(0, ["Zahtjev je istekao."]);
        }
        throw new ApiError(0, ["Nije moguće dohvatiti podatke (mreža)."]);
    }

    if (response.status === 401) {
        if (auth.getToken()) {
            auth.logout();
        }
        throw new ApiError(401, ["Sesija je istekla. Prijavite se ponovno."]);
    }

    const data = await parseResponseBody(response);

    if (!response.ok) {
        throw new ApiError(response.status, extractErrorMessages(data));
    }

    return data;
}

const api = {
    login: (email, password) => apiRequest("POST", "/auth/login", { email, password }),
    getPartners: () => apiRequest("GET", "/partners"),
    getPartnerById: (id) => apiRequest("GET", `/partners/${id}`),
    createPartner: (request) => apiRequest("POST", "/partners", request),
    createPolicy: (request) => apiRequest("POST", "/policies", request)
};
