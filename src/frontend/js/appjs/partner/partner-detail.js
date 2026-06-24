let currentModalPolicies = [];
let currentPolicyPage = 1;
const POLICY_PAGE_SIZE = 5;

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
        showAlert("listAlert", "Error fetching partner details.");
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