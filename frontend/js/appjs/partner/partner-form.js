function resetPartnerForm() {
    const form = document.getElementById("partnerForm");
    form.reset();
    form.classList.remove("was-validated");
    clearAlert("formAlert");
}

function validateOptionalPartnerFields() {
    let allValid = true;

    const oibInput = document.getElementById("croatianPIN");
    const oibValue = oibInput.value.trim();
    if (oibValue.length > 0 && !/^\d{11}$/.test(oibValue)) {
        oibInput.setCustomValidity("invalid");
        allValid = false;
    } else {
        oibInput.setCustomValidity("");
    }

    const externalCodeInput = document.getElementById("externalCode");
    const externalCodeValue = externalCodeInput.value.trim();
    if (externalCodeValue.length > 0 && (externalCodeValue.length < 10 || externalCodeValue.length > 20)) {
        externalCodeInput.setCustomValidity("invalid");
        allValid = false;
    } else {
        externalCodeInput.setCustomValidity("");
    }

    return allValid;
}

async function submitPartnerForm(e) {
    e.preventDefault();
    const form = e.target;

    const optionalFieldsValid = validateOptionalPartnerFields();

    if (!form.checkValidity() || !optionalFieldsValid) {
        form.classList.add("was-validated");
        return;
    }

    const request = {
        firstName: document.getElementById("firstName").value.trim(),
        lastName: document.getElementById("lastName").value.trim(),
        address: document.getElementById("address").value.trim() || null,
        partnerNumber: document.getElementById("partnerNumber").value.trim(),
        croatianPIN: document.getElementById("croatianPIN").value.trim() || null,
        partnerTypeId: Number(document.getElementById("partnerTypeId").value),
        createByUser: document.getElementById("createByUser").value.trim(),
        isForeign: document.getElementById("isForeign").value === "true",
        externalCode: document.getElementById("externalCode").value.trim() || null,
        gender: document.getElementById("gender").value
    };

    clearAlert("formAlert");

    const submitButton = form.querySelector('button[type="submit"]');
    submitButton.disabled = true;
    submitButton.textContent = "Spremanje...";

    try {
        const result = await api.createPartner(request);
        await loadPartners();
        showView("view-list");
        highlightNewPartnerRow(result.id);
    } catch (error) {
        if (error instanceof ApiError && error.status === 401) {
            return;
        }
        showAlert("formAlert", error.errors || ["Greška pri spremanju partnera."]);
    } finally {
        submitButton.disabled = false;
        submitButton.textContent = "Spremi partnera";
    }
}