window.APP_CONFIG = (() => {
    const baseUrl = "https://localhost:7146";
    return {
        baseUrl,
        apiBaseUrl: `${baseUrl}/api`,
        hubUrl: `${baseUrl}/hubs/partners`
    };
})();
