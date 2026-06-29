import { auth } from "./appjs/config/auth.js";
import { api } from "./api.js";

if (auth.isAuthenticated()) {
    window.location.href = "index.html";
}

document.getElementById("loginForm").addEventListener("submit", async (e) => {
    e.preventDefault();

    const btn = document.getElementById("btnLogin");
    const errorEl = document.getElementById("loginError");

    const email = document.getElementById("email").value.trim();
    const password = document.getElementById("password").value;

    errorEl.style.display = "none";
    btn.disabled = true;
    btn.textContent = "Prijava...";

    try {
        const data = await api.login(email, password);
        auth.saveToken(data.token);
        window.location.href = "index.html";
    } catch {
        errorEl.textContent = "Pogrešan e-mail ili lozinka.";
        errorEl.style.display = "block";
    } finally {
        btn.disabled = false;
        btn.textContent = "Prijava";
    }
});