const TOKEN_KEY = "ip_token";

const auth = {
    getToken() {
        return localStorage.getItem(TOKEN_KEY);
    },

    saveToken(token) {
        localStorage.setItem(TOKEN_KEY, token);
    },

    clearToken() {
        localStorage.removeItem(TOKEN_KEY);
    },

    isAuthenticated() {
        return !!this.getToken();
    },

    requireAuth() {
        if (!this.isAuthenticated()) {
            window.location.href = "login.html";
        }
    },

    logout() {
        this.clearToken();
        window.location.href = "login.html";
    }
};
