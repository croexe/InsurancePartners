const api = {
    login: (email, password) => apiRequest("POST", "/auth/login", { email, password }),
    getPartners: () => apiRequest("GET", "/partners"),
    getPartnerById: (id) => apiRequest("GET", `/partners/${id}`),
    createPartner: (request) => apiRequest("POST", "/partners", request),
    createPolicy: (request) => apiRequest("POST", "/policies", request)
};