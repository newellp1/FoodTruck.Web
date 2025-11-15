// Handles inline cart quantity updates and removal via AJAX.
document.addEventListener("DOMContentLoaded", () => {
    const cartLinesContainer = document.getElementById("cart-lines");
    if (!cartLinesContainer) return;

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

    cartLinesContainer.addEventListener("change", e => {
        if (e.target.classList.contains("js-qty")) {
            const index = e.target.dataset.index;
            const qty = parseInt(e.target.value) || 1;
            const url = "/Cart/UpdateQuantity";
            updateCartLines(url, { index, quantity: qty });
        }
    });
});