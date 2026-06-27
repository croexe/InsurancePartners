function showView(viewId) {
    document.querySelectorAll(".view").forEach((el) => el.classList.add("d-none"));
    document.getElementById(viewId).classList.remove("d-none");
}

function showAlert(containerId, messages, type = "danger") {
    const container = document.getElementById(containerId);
    const list = Array.isArray(messages) ? messages : [messages];
    const safeMessage = list.map(escapeHtml).join("<br>");
    container.innerHTML =
        '<div class="alert alert-' + type + ' alert-dismissible fade show" role="alert">' +
        safeMessage +
        '<button type="button" class="close" data-dismiss="alert" aria-label="Close">' +
        '<span aria-hidden="true">&times;</span>' +
        "</button>" +
        "</div>";
}

function clearAlert(containerId) {
    document.getElementById(containerId).innerHTML = "";
}

function escapeHtml(value) {
    const div = document.createElement("div");
    div.textContent = value == null ? "" : value;
    return div.innerHTML;
}