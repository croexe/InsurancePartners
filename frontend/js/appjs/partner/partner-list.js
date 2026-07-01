import { createPaginator } from "../helpers/pagination.js";
import { escapeHtml, showAlert } from "../helpers/helpers.js";
import { api } from "../../api.js";
import { ApiError } from "../errors/apiError.js";
import { openPartnerDetail } from "./partner-detail.js";
import { onPartnerFlagChanged } from "../services/signalr-client.js";

const pageSize = 10;
let currentPartners = [];

const partnerPaginator = createPaginator({
    pageSize: pageSize,
    infoElementId: "paginationInfo",
    controlsElementId: "paginationControls",
    itemNoun: "partnera",
    windowed: true,
    onPageChange: loadPartners
});

export async function loadPartners(page = 1) {
    try {
        const result = await api.getPartners(page, pageSize);
        currentPartners = result.items;
        renderPartnerRows(currentPartners);
        partnerPaginator.update({ page: result.page, totalCount: result.totalCount });
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

export function highlightNewPartnerRow(partnerId) {
    setTimeout(() => {
        const row = document.querySelector('.partner-row[data-id="' + partnerId + '"]');
        if (row) {
            row.classList.add("newly-added-row");
        }
    }, 50);
}

export function wireSignalRListener() {
    onPartnerFlagChanged(function (data) {
        const partner = currentPartners.find(function (candidate) { return candidate.id === data.partnerId; });
        if (partner) {
            partner.isFlagged = data.isFlagged;
            renderPartnerRows(currentPartners);
            const badge = document.querySelector('.partner-row[data-id="' + data.partnerId + '"] .flag-badge');
            if (badge) {
                badge.classList.add("pulse");
            }
        }
    });
}
