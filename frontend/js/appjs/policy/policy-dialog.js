import { clearAlert, escapeHtml, showAlert } from "../helpers/helpers.js"
import { api } from "../../api.js";
import { ApiError } from "../errors/apiError.js";
import { loadPartners } from "../partner/partner-list.js"

function populatePolicyPartnerDropdown() {
    const select = document.getElementById("policyPartnerId");
    select.innerHTML = '<option value="" disabled selected>Odaberite partnera...</option>' +
        currentPartners.map(function (partner) {
            return '<option value="' + partner.id + '">' + escapeHtml(partner.fullName) + "</option>";
        }).join("");
}

export function openPolicyDialog() {
    populatePolicyPartnerDropdown();
    document.getElementById("policyForm").reset();
    document.getElementById("policyForm").classList.remove("was-validated");
    clearAlert("policyAlert");
    $("#policyDialog").modal("show");
}

export async function submitPolicyForm(e) {
    e.preventDefault();
    const form = e.target;

    if (!form.checkValidity()) {
        form.classList.add("was-validated");
        return;
    }

    const request = {
        policyNumber: document.getElementById("policyNumber").value.trim(),
        amount: Number(document.getElementById("policyAmount").value),
        partnerId: Number(document.getElementById("policyPartnerId").value)
    };

    clearAlert("policyAlert");

    const submitButton = form.querySelector('button[type="submit"]');
    submitButton.disabled = true;
    submitButton.textContent = "Spremanje...";

    try {
        await api.createPolicy(request);
        $("#policyDialog").modal("hide");
        await loadPartners();
    } catch (error) {
        if (error instanceof ApiError && error.status === 401) {
            return;
        }
        showAlert("policyAlert", error.errors || ["Greška pri spremanju police."]);
    } finally {
        submitButton.disabled = false;
        submitButton.textContent = "Spremi policu";
    }
}
