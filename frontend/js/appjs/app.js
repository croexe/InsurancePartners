import { auth } from "../appjs/config/auth.js"
import { showView } from "./helpers/helpers.js";
import { resetPartnerForm } from "./partner/partner-form.js";
import { wireSignalRListener } from "./partner/partner-list.js";
import { loadPartners } from "./partner/partner-list.js";
import { submitPartnerForm } from "../appjs/partner/partner-form.js";
import { openPolicyDialog } from "./policy/policy-dialog.js";
import { submitPolicyForm } from "./policy/policy-dialog.js";

export function initApp() {
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
