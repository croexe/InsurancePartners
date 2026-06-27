function createPaginator(options) {
    const pageSize = options.pageSize;
    const infoElementId = options.infoElementId;
    const controlsElementId = options.controlsElementId;
    const itemNoun = options.itemNoun;
    const windowed = options.windowed === true;
    const onRender = options.onRender;

    let items = [];
    let currentPage = 1;

    function totalPages() {
        return Math.max(1, Math.ceil(items.length / pageSize));
    }

    function currentPageItems() {
        const startIndex = (currentPage - 1) * pageSize;
        return items.slice(startIndex, startIndex + pageSize);
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
        if (items.length === 0) {
            infoElement.textContent = "";
            return;
        }
        const startIndex = (currentPage - 1) * pageSize + 1;
        const endIndex = Math.min(currentPage * pageSize, items.length);
        infoElement.textContent = startIndex + "–" + endIndex + " od " + items.length + " " + itemNoun;
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

    function render() {
        if (currentPage > totalPages()) {
            currentPage = totalPages();
        }
        onRender(currentPageItems());
        renderInfo();
        renderControls();
    }

    function goToPage(page) {
        currentPage = Math.min(Math.max(1, page), totalPages());
        render();
    }

    function setItems(newItems) {
        items = newItems ?? [];
        currentPage = 1;
        render();
    }

    return { setItems, goToPage, render };
}
