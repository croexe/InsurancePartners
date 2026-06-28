const API_BASE_URL = window.APP_CONFIG.apiBaseUrl;
const REQUEST_TIMEOUT_MS = 15000;

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
            throw new ApiError(0, ["Zahtjev je istekao."]);          // ← iz errors.js
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
        throw new ApiError(response.status, extractErrorMessages(data));  // ← iz errors.js
    }

    return data;
}