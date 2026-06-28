const TOKEN_KEY = "ip_token";
const ROLE_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

function decodeJwt(token) {
    try {
        const payload = token.split(".")[1];
        const base64 = payload.replace(/-/g, "+").replace(/_/g, "/");
        return JSON.parse(atob(base64));
    } catch {
        return null;
    }
}

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

    getClaims() {
        const token = this.getToken();
        return token ? decodeJwt(token) : null;
    },

    isAuthenticated() {
        const claims = this.getClaims();
        if (!claims || !claims.exp) {
            return false;
        }
        const nowSeconds = Math.floor(Date.now() / 1000);
        return claims.exp > nowSeconds + 5;
    },

    getUserEmail() {
        const claims = this.getClaims();
        return claims?.email ?? claims?.sub ?? null;
    },

    hasRole(role) {
        const claims = this.getClaims();
        if (!claims) {
            return false;
        }
        const roles = claims.role ?? claims.roles ?? claims[ROLE_CLAIM] ?? [];
        return Array.isArray(roles) ? roles.includes(role) : roles === role;
    },

    requireAuth() {
        if (!this.isAuthenticated()) {
            this.clearToken();
            window.location.replace("login.html");
        }
    },

    logout() {
        this.clearToken();
        window.location.replace("login.html");
    }
};
