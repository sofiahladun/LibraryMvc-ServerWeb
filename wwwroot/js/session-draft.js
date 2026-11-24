document.addEventListener("DOMContentLoaded", function () {
    const draftForm = document.querySelector("form[data-draft-url]");
    if (!draftForm) return;

    const draftUrl = draftForm.getAttribute("data-draft-url");
    const statusBadge = document.getElementById("session-autosave-status");
    let debounceTimer = null;

    function saveDraftToSession() {
        const formData = new FormData(draftForm);

        fetch(draftUrl, {
            method: "POST",
            body: formData
        })
            .then(response => {
                if (response.ok && statusBadge) {
                    const now = new Date().toLocaleTimeString("uk-UA");
                    statusBadge.textContent = `Чернетку автоматично збережено в сесію о ${now}`;
                    statusBadge.classList.remove("d-none");
                }
            })
            .catch(() => { /* ігноруємо мережеві переривання при переході */ });
    }

    const inputs = draftForm.querySelectorAll("input, select, textarea");
    inputs.forEach(input => {
        input.addEventListener("input", function () {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(saveDraftToSession, 350);
        });
        input.addEventListener("change", saveDraftToSession);
    });
});