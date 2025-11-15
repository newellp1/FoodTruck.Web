// Polls order status for tracking page.
document.addEventListener("DOMContentLoaded", () => {
    const statusContainer = document.getElementById("order-status");
    const statusText = document.getElementById("order-status-text");
    if (!statusContainer || !statusText) return;

    const orderId = statusContainer.dataset.orderId;
    const apiUrl = `/api/Orders/${orderId}/status`;

    async function pollStatus() {
        try {
            const resp = await fetch(apiUrl);
            if (!resp.ok) return;

            const data = await resp.json();
            statusText.textContent = data.status;

            if (data.status === "Ready") {
                statusContainer.classList.add("alert", "alert-success");
            }

            if (data.status === "Cancelled" || data.status === "Rejected") {
                statusContainer.classList.add("alert", "alert-danger");
            }
        } catch (e) {
            console.error(e);
        }
    }

    pollStatus();
    setInterval(pollStatus, 7000); // 7 seconds
});