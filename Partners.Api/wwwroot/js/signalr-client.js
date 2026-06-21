const HUB_URL = "https://localhost:7146/hubs/partners";

const partnerHubConnection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL)
    .withAutomaticReconnect()
    .build();

function onPartnerFlagChanged(callback) {
    partnerHubConnection.on("PartnerFlagChanged", callback);
}

function setConnectionStatus(text, state) {
    const el = document.getElementById("connectionStatus");
    if (!el) return;
    el.querySelector(".status-text").textContent = text;
    el.classList.remove("is-live", "is-down");
    if (state) el.classList.add(state);
}

partnerHubConnection.onreconnecting(() => setConnectionStatus("Ponovno spajanje...", null));
partnerHubConnection.onreconnected(() => setConnectionStatus("Uživo", "is-live"));
partnerHubConnection.onclose(() => setConnectionStatus("Veza prekinuta", "is-down"));

partnerHubConnection
    .start()
    .then(() => setConnectionStatus("Uživo", "is-live"))
    .catch((err) => {
        console.error("SignalR connection error:", err);
        setConnectionStatus("Nije moguće spojiti se", "is-down");
    });