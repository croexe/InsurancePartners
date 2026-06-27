let currentModalPolicies = [];

const policyPaginator = createPaginator({
    pageSize: 5,
    infoElementId: "policyPaginationInfo",
    controlsElementId: "policyPaginationControls",
    itemNoun: "polica",
    windowed: false,
    onRender: renderPolicyRows
});

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
        policyPaginator.setItems(currentModalPolicies);

        $("#partnerDetailModal").modal("show");
    } catch (error) {
        if (error instanceof ApiError && error.status === 401) {
            return;
        }
        showAlert("listAlert", error.errors || ["Greška pri dohvaćanju detalja partnera."]);
    }
}

function renderPolicyRows(pagePolicies) {
    const tbody = document.getElementById("policyTableBody");

    if (pagePolicies.length === 0) {
        tbody.innerHTML = '<tr><td colspan="2" class="text-muted">Nema unesenih polica.</td></tr>';
        return;
    }

    tbody.innerHTML = pagePolicies
        .map(function (policy) {
            return "<tr><td>" + escapeHtml(policy.policyNumber) + "</td><td class=\"amount\">" + policy.amount.toFixed(2) + " kn</td></tr>";
        })
        .join("");
}
