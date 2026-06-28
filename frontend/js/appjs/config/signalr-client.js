const HUB_URL = window.APP_CONFIG.hubUrl;

const partnerHubConnection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, {
        accessTokenFactory: () => auth.getToken()
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000])
    .configureLogging(signalR.LogLevel.Warning)
    .build();

function setConnectionStatus(text, state) {
    const element = document.getElementById("connectionStatus");
    if (!element) {
        return;
    }
    element.querySelector(".status-text").textContent = text;
    element.classList.remove("is-live", "is-down");
    if (state) {
        element.classList.add(state);
    }
}

partnerHubConnection.onreconnecting(() => setConnectionStatus("Ponovno spajanje...", null));
partnerHubConnection.onreconnected(() => setConnectionStatus("Uživo", "is-live"));
partnerHubConnection.onclose(() => setConnectionStatus("Veza prekinuta", "is-down"));

function onPartnerFlagChanged(callback) {
    partnerHubConnection.on("PartnerFlagChanged", callback);
    return () => partnerHubConnection.off("PartnerFlagChanged", callback);
}

async function startHub() {
    if (partnerHubConnection.state !== signalR.HubConnectionState.Disconnected) {
        return;
    }
    try {
        await partnerHubConnection.start();
        setConnectionStatus("Uživo", "is-live");
    } catch (error) {
        console.error("SignalR start error:", error);
        setConnectionStatus("Nije moguće spojiti se", "is-down");
        setTimeout(startHub, 5000);
    }
}

if (auth.isAuthenticated()) {
    startHub();
}
