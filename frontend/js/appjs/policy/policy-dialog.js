// policy-dialog.js - dijalog za unos nove police, vezan na partnera
// odabranog iz dropdowna (popunjenog iz currentPartners, vidi partner-list.js).

function populatePolicyPartnerDropdown() {
    const select = document.getElementById("policyPartnerId");
    select.innerHTML = '<option value="" disabled selected>Odaberite partnera...</option>' +
        currentPartners.map(function (p) {
            return '<option value="' + p.id + '">' + escapeHtml(p.fullName) + "</option>";
        }).join("");
}

function openPolicyDialog() {
    populatePolicyPartnerDropdown();
    document.getElementById("policyForm").reset();
    document.getElementById("policyForm").classList.remove("was-validated");
    clearAlert("policyAlert");
    $("#policyDialog").modal("show");
}

async function submitPolicyForm(e) {
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

    try {
        await api.createPolicy(request);
        $("#policyDialog").modal("hide");
        await loadPartners();
    } catch (err) {
        showAlert("policyAlert", (err.errors || ["Error saving policy."]).join("<br>"));
    }
}