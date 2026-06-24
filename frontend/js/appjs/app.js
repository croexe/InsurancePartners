function initApp() {
    document.getElementById("btnLogout").addEventListener("click", () => auth.logout());

    $("#partnerDetailModal").on("hide.bs.modal", function () {
        document.activeElement.blur();
    });

    $("#policyDialog").on("hide.bs.modal", function () {
        document.activeElement.blur();
    });

    document.getElementById("btnGoToPartnerForm").addEventListener("click", () => {
        resetPartnerForm();
        showView("view-partner-form");
    });

    document.getElementById("btnBackToListFromForm").addEventListener("click", () => {
        showView("view-list");
    });

    document.getElementById("partnerForm").addEventListener("submit", submitPartnerForm);

    document.getElementById("btnOpenPolicyDialog").addEventListener("click", openPolicyDialog);

    document.getElementById("policyForm").addEventListener("submit", submitPolicyForm);

    wireSignalRListener();

    loadPartners();
}


loadAllPartials()
    .then(initApp)
    .catch(function (err) {
        console.error("Greška kod učitavanja partiala:", err);
    });