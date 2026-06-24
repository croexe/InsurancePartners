const API_BASE_URL = "https://localhost:7146/api";

async function apiRequest(method, path, body) {
    const token = auth.getToken();
    const headers = { "Content-Type": "application/json" };

    if (token) {
        headers["Authorization"] = `Bearer ${token}`;
    }

    const response = await fetch(`${API_BASE_URL}${path}`, {
        method,
        headers,
        body: body ? JSON.stringify(body) : undefined
    });

    if (response.status === 401) {
        auth.logout();
        return;
    }

    if (response.status === 204) {
        return null;
    }

    const data = await response.json().catch(() => null);

    if (!response.ok) {
        const errors = extractErrorMessages(data);
        throw { status: response.status, errors };
    }

    return data;
}

function extractErrorMessages(data) {
    if (!data) {
        return ["Unknown error occured."];
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

    return ["Unknown error occured."];
}

const api = {
    login: (email, password) => apiRequest("POST", "/auth/login", { email, password }),
    getPartners: () => apiRequest("GET", "/partners"),
    getPartnerById: (id) => apiRequest("GET", `/partners/${id}`),
    createPartner: (request) => apiRequest("POST", "/partners", request),
    createPolicy: (request) => apiRequest("POST", "/policies", request)
};
