import { auth } from "./appjs/config/auth.js";
import { loadAllPartials } from "./appjs/config/partials-loader.js";
import { initApp } from "./appjs/app.js";
import { startHub } from "./appjs/services/signalr-client.js"

auth.requireAuth();

loadAllPartials()
    .then(() => {
        initApp();
        startHub();
    })
    .catch(function (err) {
        console.error("Error loading partials:", err);
    });