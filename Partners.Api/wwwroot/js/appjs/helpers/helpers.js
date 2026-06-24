function showView(viewId) {
    document.querySelectorAll(".view").forEach((el) => el.classList.add("d-none"));
    document.getElementById(viewId).classList.remove("d-none");
}

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

function escapeHtml(value) {
    const div = document.createElement("div");
    div.textContent = value == null ? "" : value;
    return div.innerHTML;
}