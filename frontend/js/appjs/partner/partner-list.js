let currentPartners = [];
let currentPage = 1;
const PAGE_SIZE = 10;

async function loadPartners() {
    try {
        currentPartners = await api.getPartners();
        currentPage = 1;
        renderPartnerTable();
    } catch (err) {
        showAlert("listAlert", "Error fetching partner: " + (err.errors || []).join(", "));
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

function highlightNewPartnerRow(partnerId) {
    setTimeout(() => {
        const row = document.querySelector('.partner-row[data-id="' + partnerId + '"]');
        if (row) row.classList.add("newly-added-row");
    }, 50);
}

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