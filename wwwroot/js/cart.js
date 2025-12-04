// Handles inline cart quantity updates and removal via AJAX.
document.addEventListener("DOMContentLoaded", () => {
    const cartLinesContainer = document.getElementById("cart-lines");
    if (!cartLinesContainer) return;

    // Updates cart lines by sending AJAX requests to the server.
    // Expects a URL and form data object.
    async function updateCartLines(url, formData) {
        const resp = await fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8"
            },
            body: new URLSearchParams(formData).toString()
        });

        if (resp.ok) {
            const html = await resp.text();
            cartLinesContainer.innerHTML = html;
        } else {
            alert("Unable to update cart.");
        }
    }

    // Handles click events for quantity adjustments and line removals.
    // Delegates events to the container for efficiency.
    // Also handles direct quantity input changes.
    cartLinesContainer.addEventListener("click", e => {
        if (e.target.classList.contains("js-qty-minus") ||
            e.target.classList.contains("js-qty-plus")) {
            const index = e.target.dataset.index;
            const qtyInput = cartLinesContainer.querySelector(`.js-qty[data-index="${index}"]`);
            let qty = parseInt(qtyInput.value) || 1;
            if (e.target.classList.contains("js-qty-minus")) qty--;
            else qty++;
            const url = "/Cart/UpdateQuantity";
            updateCartLines(url, { index, quantity: qty });
        }

        if (e.target.classList.contains("js-remove-line")) {
            const index = e.target.dataset.index;
            const url = "/Cart/Remove";
            updateCartLines(url, { index });
        }
    });

    // Handles direct quantity input changes.
    // Sends an update request when the quantity input value changes.
    // Listens for 'change' events on quantity input fields.
    cartLinesContainer.addEventListener("change", e => {
        if (e.target.classList.contains("js-qty")) {
            const index = e.target.dataset.index;
            const qty = parseInt(e.target.value) || 1;
            const url = "/Cart/UpdateQuantity";
            updateCartLines(url, { index, quantity: qty });
        }
    });
});