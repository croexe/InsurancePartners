let currentPartners = [];
let currentPage = 1;
const PAGE_SIZE = 10;

let currentModalPolicies = [];
let currentPolicyPage = 1;
const POLICY_PAGE_SIZE = 5;

// ===================== VIEW SWITCHING =====================

function showView(viewId) {
    document.querySelectorAll(".view").forEach((el) => el.classList.add("d-none"));
    document.getElementById(viewId).classList.remove("d-none");
}

// ===================== ALERT HELPERS =====================

function showAlert(containerId, message, type = "danger") {
    const container = document.getElementById(containerId);
    container.innerHTML = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                <span aria-hidden="true">&times;</span>
            </button>
        </div>`;
}

function clearAlert(containerId) {
    document.getElementById(containerId).innerHTML = "";
}

// ===================== PARTNER LIST =====================

async function loadPartners() {
    try {
        currentPartners = await api.getPartners();
        currentPage = 1;
        renderPartnerTable();
    } catch (err) {
        showAlert("listAlert", "Greška kod dohvaćanja partnera: " + (err.errors || []).join(", "));
    }
}

function getTotalPages() {
    return Math.max(1, Math.ceil(currentPartners.length / PAGE_SIZE));
}

function goToPage(page) {
    const totalPages = getTotalPages();
    currentPage = Math.min(Math.max(1, page), totalPages);
    renderPartnerTable();
}

function renderPartnerTable() {
    const tbody = document.getElementById("partnerTableBody");

    if (currentPartners.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" class="empty-cell">Nema unesenih partnera.</td></tr>';
        renderPaginationControls();
        return;
    }

    const totalPages = getTotalPages();
    if (currentPage > totalPages) {
        currentPage = totalPages;
    }

    const startIndex = (currentPage - 1) * PAGE_SIZE;
    const pagePartners = currentPartners.slice(startIndex, startIndex + PAGE_SIZE);

    tbody.innerHTML = pagePartners
        .map((p) => {
            const flag = p.isFlagged ? '<span class="flag-badge">*</span>' : "";
            const dateStr = new Date(p.createdAtUtc).toLocaleString("hr-HR");
            return [
                '<tr class="partner-row" data-id="' + p.id + '">',
                "<td><span class=\"partner-name-cell\">" + flag + escapeHtml(p.fullName) + "</span></td>",
                "<td>" + escapeHtml(p.partnerNumber) + "</td>",
                "<td>" + escapeHtml(p.croatianPIN || "—") + "</td>",
                "<td>" + escapeHtml(p.partnerTypeName) + "</td>",
                "<td>" + dateStr + "</td>",
                "<td>" + (p.isForeign ? "Da" : "Ne") + "</td>",
                "<td>" + escapeHtml(p.gender) + "</td>",
                "</tr>"
            ].join("");
        })
        .join("");

    document.querySelectorAll(".partner-row").forEach((row) => {
        row.addEventListener("click", () => openPartnerDetail(row.dataset.id));
    });

    renderPaginationControls();
}

function renderPaginationControls() {
    const totalPages = getTotalPages();
    const total = currentPartners.length;

    const infoEl = document.getElementById("paginationInfo");
    if (total === 0) {
        infoEl.textContent = "";
    } else {
        const startIndex = (currentPage - 1) * PAGE_SIZE + 1;
        const endIndex = Math.min(currentPage * PAGE_SIZE, total);
        infoEl.textContent = startIndex + "–" + endIndex + " od " + total + " partnera";
    }

    const controlsEl = document.getElementById("paginationControls");

    if (totalPages <= 1) {
        controlsEl.innerHTML = "";
        return;
    }

    const buttons = [];

    buttons.push(
        '<button type="button" class="page-btn" data-page="' + (currentPage - 1) + '"' +
        (currentPage === 1 ? " disabled" : "") + '>&lsaquo;</button>'
    );

    for (let page = 1; page <= totalPages; page++) {
        const isEdge = page === 1 || page === totalPages;
        const isNearCurrent = Math.abs(page - currentPage) <= 1;

        if (isEdge || isNearCurrent) {
            buttons.push(
                '<button type="button" class="page-btn' + (page === currentPage ? " active" : "") + '" data-page="' + page + '">' + page + '</button>'
            );
        } else if (Math.abs(page - currentPage) === 2) {
            buttons.push('<span class="page-btn page-btn--ellipsis">…</span>');
        }
    }

    buttons.push(
        '<button type="button" class="page-btn" data-page="' + (currentPage + 1) + '"' +
        (currentPage === totalPages ? " disabled" : "") + '>&rsaquo;</button>'
    );

    controlsEl.innerHTML = buttons.join("");

    controlsEl.querySelectorAll(".page-btn[data-page]").forEach((btn) => {
        btn.addEventListener("click", () => goToPage(Number(btn.dataset.page)));
    });
}

function escapeHtml(value) {
    const div = document.createElement("div");
    div.textContent = value == null ? "" : value;
    return div.innerHTML;
}

function highlightNewPartnerRow(partnerId) {
    setTimeout(() => {
        const row = document.querySelector('.partner-row[data-id="' + partnerId + '"]');
        if (row) row.classList.add("newly-added-row");
    }, 50);
}

// ===================== PARTNER DETAIL MODAL =====================

async function openPartnerDetail(id) {
    try {
        const partner = await api.getPartnerById(id);
        document.getElementById("partnerDetailTitle").textContent = partner.fullName;

        const statusPill = partner.isFlagged
            ? '<span class="status-pill status-pill--flagged">&#9733; Oznacen partner</span>'
            : '<span class="status-pill status-pill--clear">Bez oznake</span>';

        document.getElementById("partnerDetailBody").innerHTML =
            '<div class="mb-3">' + statusPill + "</div>" +
            '<dl class="detail-grid">' +
            "<dt>Adresa</dt><dd>" + escapeHtml(partner.address || "—") + "</dd>" +
            "<dt>Partner broj</dt><dd>" + escapeHtml(partner.partnerNumber) + "</dd>" +
            "<dt>OIB</dt><dd>" + escapeHtml(partner.croatianPIN || "—") + "</dd>" +
            "<dt>Vrsta</dt><dd>" + escapeHtml(partner.partnerTypeName) + "</dd>" +
            "<dt>Datum unosa</dt><dd>" + new Date(partner.createdAtUtc).toLocaleString("hr-HR") + "</dd>" +
            "<dt>Unio</dt><dd>" + escapeHtml(partner.createByUser) + "</dd>" +
            "<dt>Strani partner</dt><dd>" + (partner.isForeign ? "Da" : "Ne") + "</dd>" +
            "<dt>Eksterni kod</dt><dd>" + escapeHtml(partner.externalCode || "—") + "</dd>" +
            "<dt>Spol</dt><dd>" + escapeHtml(partner.gender) + "</dd>" +
            "</dl>" +
            '<p class="form-section-label">Police</p>' +
            '<table class="policy-table">' +
            '<thead><tr><th>Broj police</th><th class="amount">Iznos</th></tr></thead>' +
            '<tbody id="policyTableBody"></tbody>' +
            "</table>" +
            '<div class="pagination-bar" id="policyPaginationBar">' +
            '<span class="pagination-info" id="policyPaginationInfo"></span>' +
            '<div class="pagination-controls" id="policyPaginationControls"></div>' +
            "</div>";

        currentModalPolicies = partner.policies;
        currentPolicyPage = 1;
        renderPolicyTable();

        $("#partnerDetailModal").modal("show");
    } catch (err) {
        showAlert("listAlert", "Greška kod dohvaćanja detalja partnera.");
    }
}

function getTotalPolicyPages() {
    return Math.max(1, Math.ceil(currentModalPolicies.length / POLICY_PAGE_SIZE));
}

function goToPolicyPage(page) {
    const totalPages = getTotalPolicyPages();
    currentPolicyPage = Math.min(Math.max(1, page), totalPages);
    renderPolicyTable();
}

function renderPolicyTable() {
    const tbody = document.getElementById("policyTableBody");

    if (currentModalPolicies.length === 0) {
        tbody.innerHTML = '<tr><td colspan="2" class="text-muted">Nema unesenih polica.</td></tr>';
        renderPolicyPaginationControls();
        return;
    }

    const totalPages = getTotalPolicyPages();
    if (currentPolicyPage > totalPages) {
        currentPolicyPage = totalPages;
    }

    const startIndex = (currentPolicyPage - 1) * POLICY_PAGE_SIZE;
    const pagePolicies = currentModalPolicies.slice(startIndex, startIndex + POLICY_PAGE_SIZE);

    tbody.innerHTML = pagePolicies
        .map(function (pol) {
            return "<tr><td>" + escapeHtml(pol.policyNumber) + "</td><td class=\"amount\">" + pol.amount.toFixed(2) + " kn</td></tr>";
        })
        .join("");

    renderPolicyPaginationControls();
}

function renderPolicyPaginationControls() {
    const totalPages = getTotalPolicyPages();
    const total = currentModalPolicies.length;

    const infoEl = document.getElementById("policyPaginationInfo");
    if (total === 0) {
        infoEl.textContent = "";
    } else {
        const startIndex = (currentPolicyPage - 1) * POLICY_PAGE_SIZE + 1;
        const endIndex = Math.min(currentPolicyPage * POLICY_PAGE_SIZE, total);
        infoEl.textContent = startIndex + "–" + endIndex + " od " + total + " polica";
    }

    const controlsEl = document.getElementById("policyPaginationControls");

    if (totalPages <= 1) {
        controlsEl.innerHTML = "";
        return;
    }

    const buttons = [];

    buttons.push(
        '<button type="button" class="page-btn" data-page="' + (currentPolicyPage - 1) + '"' +
        (currentPolicyPage === 1 ? " disabled" : "") + '>&lsaquo;</button>'
    );

    for (let page = 1; page <= totalPages; page++) {
        buttons.push(
            '<button type="button" class="page-btn' + (page === currentPolicyPage ? " active" : "") + '" data-page="' + page + '">' + page + '</button>'
        );
    }

    buttons.push(
        '<button type="button" class="page-btn" data-page="' + (currentPolicyPage + 1) + '"' +
        (currentPolicyPage === totalPages ? " disabled" : "") + '>&rsaquo;</button>'
    );

    controlsEl.innerHTML = buttons.join("");

    controlsEl.querySelectorAll(".page-btn[data-page]").forEach((btn) => {
        btn.addEventListener("click", () => goToPolicyPage(Number(btn.dataset.page)));
    });
}

// ===================== PARTNER FORM =====================

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

// ===================== POLICY DIALOG HELPERS =====================

function populatePolicyPartnerDropdown() {
    const select = document.getElementById("policyPartnerId");
    select.innerHTML = '<option value="" disabled selected>Odaberite partnera...</option>' +
        currentPartners.map(function (p) {
            return '<option value="' + p.id + '">' + escapeHtml(p.fullName) + "</option>";
        }).join("");
}

// ===================== SIGNALR - REAL-TIME * AŽURIRANJE =====================

function wireSignalRListener() {
    onPartnerFlagChanged(function (data) {
        const partner = currentPartners.find(function (p) { return p.id === data.partnerId; });
        if (partner) {
            partner.isFlagged = data.isFlagged;
            renderPartnerTable();
            const badge = document.querySelector('.partner-row[data-id="' + data.partnerId + '"] .flag-badge');
            if (badge) badge.classList.add("pulse");
        }
    });
}

// ===================== INIT - vezivanje event listenera na ucitane partiale =====================

function initApp() {
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

    document.getElementById("partnerForm").addEventListener("submit", async function (e) {
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

        try {
            const result = await api.createPartner(request);
            await loadPartners();
            showView("view-list");
            highlightNewPartnerRow(result.id);
        } catch (err) {
            showAlert("formAlert", (err.errors || ["Error while saving partner."]).join("<br>"));
        }
    });

    document.getElementById("btnOpenPolicyDialog").addEventListener("click", function () {
        populatePolicyPartnerDropdown();
        document.getElementById("policyForm").reset();
        document.getElementById("policyForm").classList.remove("was-validated");
        clearAlert("policyAlert");
        $("#policyDialog").modal("show");
    });

    document.getElementById("policyForm").addEventListener("submit", async function (e) {
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
            showAlert("policyAlert", (err.errors || ["Error while saving policy."]).join("<br>"));
        }
    });

    wireSignalRListener();

    loadPartners();
}

loadAllPartials()
    .then(initApp)
    .catch(function (err) {
        console.error("Greška kod učitavanja partiala:", err);
    });