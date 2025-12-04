// Handles menu search, category filter, and item detail modal.
document.addEventListener("DOMContentLoaded", () => {
    const searchInput = document.getElementById("menu-search");
    const categoryCheckboxes = document.querySelectorAll(".category-filter");
    const menuSections = document.querySelectorAll(".menu-category");
    const itemDetailModalEl = document.getElementById("item-detail-modal");
    const itemDetailContent = document.getElementById("item-detail-content");
    let itemDetailModal;

    if (itemDetailModalEl) {
        itemDetailModal = new bootstrap.Modal(itemDetailModalEl);
    }

    // Filters menu items based on search term and selected categories.
    // Hides/shows menu sections accordingly.
    // Called on search input or category checkbox change.
    function filterMenu() {
        const term = (searchInput?.value || "").toLowerCase();
        const selectedCategories = Array.from(categoryCheckboxes)
            .filter(cb => cb.checked)
            .map(cb => parseInt(cb.value));

        menuSections.forEach(section => {
            const catId = parseInt(section.dataset.categoryId);
            const rows = section.querySelectorAll(".menu-item");
            let anyVisible = false;

            rows.forEach(card => {
                const name = card.dataset.name;
                const desc = card.dataset.description;
                const matchesText = !term || name.includes(term) || desc.includes(term);
                const matchesCategory = selectedCategories.includes(catId);

                if (matchesText && matchesCategory) {
                    card.classList.remove("d-none");
                    anyVisible = true;
                } else {
                    card.classList.add("d-none");
                }
            });

            section.classList.toggle("d-none", !anyVisible);
        });
    }

    if (searchInput) {
        searchInput.addEventListener("input", filterMenu);
    }

    categoryCheckboxes.forEach(cb => cb.addEventListener("change", filterMenu));

    // Item detail modal
    document.body.addEventListener("click", async (e) => {
        const btn = e.target.closest(".js-view-item");
        if (!btn) return;

        const itemId = btn.dataset.itemId;

        const url = `${window.menuConfig.itemDetailUrl}?id=${itemId}`;
        const response = await fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } });
        const html = await response.text();
        itemDetailContent.innerHTML = html;
        itemDetailModal.show();
        attachItemDetailHandlers();
    });

    // Attaches event handlers for item detail modal interactions.
    // Handles quantity changes, modification selections, and adding to cart.
    // Updates line price dynamically based on selections.
    function attachItemDetailHandlers() {
        const basePriceEl = document.getElementById("item-base-price");
        const linePriceEl = document.getElementById("item-line-price");
        const qtyInput = document.getElementById("item-quantity");
        const addBtn = document.getElementById("btn-add-to-cart");
        const notesEl = document.getElementById("item-notes");
        const form = document.getElementById("item-detail-form");

        if (!form) return;

        const itemId = form.dataset.itemId;
        const basePrice = parseFloat(basePriceEl.textContent);
        const modCheckboxes = form.querySelectorAll(".mod-checkbox");

        function updateLinePrice() {
            const qty = parseInt(qtyInput.value) || 1;
            let delta = 0;
            modCheckboxes.forEach(cb => {
                if (cb.checked) {
                    delta += parseFloat(cb.dataset.pricedelta);
                }
            });
            const linePrice = (basePrice + delta) * qty;
            linePriceEl.textContent = linePrice.toFixed(2);
        }

        // Attaches event handlers for item detail modal interactions.
        // Handles quantity changes, modification selections, and adding to cart.
        // Updates line price dynamically based on selections.
        modCheckboxes.forEach(cb => cb.addEventListener("change", updateLinePrice));
        qtyInput.addEventListener("input", updateLinePrice);

        if (addBtn) {
            addBtn.addEventListener("click", async () => {
                const qty = parseInt(qtyInput.value) || 1;
                const notes = notesEl.value;
                const selectedMods = Array.from(modCheckboxes)
                    .filter(cb => cb.checked)
                    .map(cb => cb.value);

                const formData = new URLSearchParams();
                formData.append("menuItemId", itemId);
                formData.append("quantity", qty);
                formData.append("notes", notes);

                const resp = await fetch(window.menuConfig.addToCartUrl, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8"
                    },
                    body: formData.toString()
                });

                if (resp.ok) {
                    const html = await resp.text();
                    document.getElementById("cart-summary").innerHTML = html;
                    itemDetailModal.hide();
                } else {
                    alert("Could not add to cart.");
                }
            });
        }

        updateLinePrice();
    }
});