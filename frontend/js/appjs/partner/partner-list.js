let currentPartners = [];

const partnerPaginator = createPaginator({
    pageSize: 10,
    infoElementId: "paginationInfo",
    controlsElementId: "paginationControls",
    itemNoun: "partnera",
    windowed: true,
    onRender: renderPartnerRows
});

async function loadPartners() {
    try {
        currentPartners = await api.getPartners();
        partnerPaginator.setItems(currentPartners);
    } catch (error) {
        if (error instanceof ApiError && error.status === 401) {
            return;
        }
        showAlert("listAlert", error.errors || ["Greška pri dohvaćanju partnera."]);
    }
}

function renderPartnerRows(pagePartners) {
    const tbody = document.getElementById("partnerTableBody");

    if (pagePartners.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" class="empty-cell">Nema unesenih partnera.</td></tr>';
        return;
    }

    tbody.innerHTML = pagePartners
        .map((partner) => {
            const flag = partner.isFlagged ? '<span class="flag-badge">*</span>' : "";
            const dateString = new Date(partner.createdAtUtc).toLocaleString("hr-HR");
            return [
                '<tr class="partner-row" data-id="' + partner.id + '">',
                "<td><span class=\"partner-name-cell\">" + flag + escapeHtml(partner.fullName) + "</span></td>",
                "<td>" + escapeHtml(partner.partnerNumber) + "</td>",
                "<td>" + escapeHtml(partner.croatianPIN || "—") + "</td>",
                "<td>" + escapeHtml(partner.partnerTypeName) + "</td>",
                "<td>" + dateString + "</td>",
                "<td>" + (partner.isForeign ? "Da" : "Ne") + "</td>",
                "<td>" + escapeHtml(partner.gender) + "</td>",
                "</tr>"
            ].join("");
        })
        .join("");

    document.querySelectorAll(".partner-row").forEach((row) => {
        row.addEventListener("click", () => openPartnerDetail(row.dataset.id));
    });
}

function highlightNewPartnerRow(partnerId) {
    setTimeout(() => {
        const row = document.querySelector('.partner-row[data-id="' + partnerId + '"]');
        if (row) {
            row.classList.add("newly-added-row");
        }
    }, 50);
}

function wireSignalRListener() {
    onPartnerFlagChanged(function (data) {
        const partner = currentPartners.find(function (candidate) { return candidate.id === data.partnerId; });
        if (partner) {
            partner.isFlagged = data.isFlagged;
            partnerPaginator.render();
            const badge = document.querySelector('.partner-row[data-id="' + data.partnerId + '"] .flag-badge');
            if (badge) {
                badge.classList.add("pulse");
            }
        }
    });
}
