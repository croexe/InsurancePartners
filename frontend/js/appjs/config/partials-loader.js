async function loadPartial(targetId, url) {
    const response = await fetch(url);
    if (!response.ok) {
        throw new Error("Partial loading failed: " + url);
    }
    const html = await response.text();
    document.getElementById(targetId).innerHTML = html;
}

export async function loadAllPartials() {
    await Promise.all([
        loadPartial("partial-partner-list", "partials/partner/partner-list.html"),
        loadPartial("partial-partner-form", "partials/partner/partner-form.html"),
        loadPartial("partial-partner-detail-modal", "partials/partner/modals/partner-detail-modal.html"),
        loadPartial("partial-policy-dialog", "partials/policy/dialogs/policy-dialog.html")
    ]);
}