import { apiRequest } from "../js/appjs/services/http-client.js"

export const api = {
    login: (email, password) => apiRequest("POST", "/auth/login", { email, password }),
    getPartners: (page, pageSize) => apiRequest("GET", `/partners?page=${page}&pageSize=${pageSize}`),
    getPartnerById: (id) => apiRequest("GET", `/partners/${id}`),
    createPartner: (request) => apiRequest("POST", "/partners", request),
    createPolicy: (request) => apiRequest("POST", "/policies", request)
};