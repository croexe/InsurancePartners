export function createPaginator(options) {
    const pageSize = options.pageSize;
    const infoElementId = options.infoElementId;
    const controlsElementId = options.controlsElementId;
    const itemNoun = options.itemNoun;
    const windowed = options.windowed === true;
    const onPageChange = options.onPageChange;

    let currentPage = 1;
    let totalCount = 0;

    function totalPages() {
        return Math.max(1, Math.ceil(totalCount / pageSize));
    }

    function pageButton(page, label, disabled, active) {
        return '<button type="button" class="page-btn' + (active ? " active" : "") +
            '" data-page="' + page + '"' + (disabled ? " disabled" : "") + ">" + label + "</button>";
    }

    function renderInfo() {
        const infoElement = document.getElementById(infoElementId);
        if (!infoElement) {
            return;
        }
        if (totalCount === 0) {
            infoElement.textContent = "";
            return;
        }
        const startIndex = (currentPage - 1) * pageSize + 1;
        const endIndex = Math.min(currentPage * pageSize, totalCount);
        infoElement.textContent = startIndex + "–" + endIndex + " od " + totalCount + " " + itemNoun;
    }

    function renderControls() {
        const controlsElement = document.getElementById(controlsElementId);
        if (!controlsElement) {
            return;
        }
        const pages = totalPages();
        if (pages <= 1) {
            controlsElement.innerHTML = "";
            return;
        }
        const buttons = [];
        buttons.push(pageButton(currentPage - 1, "&lsaquo;", currentPage === 1, false));
        for (let page = 1; page <= pages; page++) {
            if (windowed) {
                const isEdge = page === 1 || page === pages;
                const isNearCurrent = Math.abs(page - currentPage) <= 1;
                if (isEdge || isNearCurrent) {
                    buttons.push(pageButton(page, page, false, page === currentPage));
                } else if (Math.abs(page - currentPage) === 2) {
                    buttons.push('<span class="page-btn page-btn--ellipsis">…</span>');
                }
            } else {
                buttons.push(pageButton(page, page, false, page === currentPage));
            }
        }
        buttons.push(pageButton(currentPage + 1, "&rsaquo;", currentPage === pages, false));
        controlsElement.innerHTML = buttons.join("");
        controlsElement.querySelectorAll(".page-btn[data-page]").forEach((button) => {
            button.addEventListener("click", () => goToPage(Number(button.dataset.page)));
        });
    }

    function goToPage(page) {
        const target = Math.min(Math.max(1, page), totalPages());
        if (target === currentPage) {
            return;
        }
        onPageChange(target);
    }

    function update(state) {
        totalCount = state.totalCount ?? 0;
        currentPage = state.page ?? 1;
        renderInfo();
        renderControls();
    }

    return { update };
}
