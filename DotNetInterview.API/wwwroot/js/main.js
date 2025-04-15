document.addEventListener('DOMContentLoaded', () => {
    const viewItemSection = document.getElementById('view-item-section');
    const createItemSection = document.getElementById('create-item-section');
    const editItemSection = document.getElementById('edit-item-section'); 
    const viewVariationsTable = document.getElementById('view-variations-table');
    const createItemBtn = document.getElementById('create-item-btn');
    const createAddVariationBtn = document.getElementById('create-add-variation-btn');
    const createRemoveVariationBtn = document.getElementById('create-remove-variation-btn');
    const variationSection = document.getElementById('variation-section');
    const createVariationRows = document.getElementById('create-variation-rows');
    const createForm = document.getElementById('create-form');
    const createItemSubmitBtn = document.getElementById('create-submit-btn');
    const popupOkBtn = document.getElementById('popup-ok-btn');
    const editItemBtn = document.getElementById('edit-item-btn');
    const editForm = document.getElementById('edit-form');
    const editAddVariationBtn = document.getElementById('edit-add-row-btn');
    const editRemoveVariationBtn = document.getElementById('edit-remove-row-btn');
    const editVariationRows = document.getElementById('edit-variation-rows');
    const editItemSubmitBtn = document.getElementById('edit-submit-btn');

    // Hide all sections on page load
    viewItemSection.style.display = 'none';
    createItemSection.style.display = 'none';
    editItemSection.style.display = 'none';

    // Load stock table on page load
    loadStockTable()

    // View Item
    function loadViewItemDetails(itemId) {
        fetch(`/api/Items/${itemId}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error("Failed to load item details.");
                }
                return response.json();
            })
            .then(item => {
                // Bind properties in view-item-table
                document.getElementById('item-id').textContent = item.id;
                editItemBtn.dataset.itemId = item.id;   // Store the item id in the button's dataset
                document.getElementById('item-reference').textContent = item.reference;
                document.getElementById('item-name').textContent = item.name;
                document.getElementById('item-price').textContent = `£${item.price.toFixed(2)}`;

                // Load view-variations-table
                viewVariationsTable.innerHTML = ''; // Clear existing rows
                item.variations.forEach(variation => {
                    const row = document.createElement('tr');
                    row.innerHTML = `
                        <td><strong>Size:</strong></td>
                        <td>${variation.size}</td>
                        <td><strong>Quantity:</strong></td>
                        <td>${variation.quantity}</td>
                    `;
                    viewVariationsTable.appendChild(row);
                });

                // Show the view item section
                viewItemSection.style.display = 'block';
                viewItemSection.scrollIntoView({ behavior: 'smooth' });
            })
            .catch(error => {
                console.error("Error fetching item details:", error);
            });
    }

    // Load stock table
    function loadStockTable(itemId = null) {
        fetch('/api/Items')
            .then(response => {
                if (!response.ok) {
                    throw new Error("Failed to load stock items.");
                }
                return response.json();
            })
            .then(data => {
                const tbody = document.getElementById('stock-table-body');
                tbody.innerHTML = ''; // Clear existing rows

                data.forEach(item => {
                    const tr = document.createElement('tr');

                    const originalPrice = `£${item.price.toFixed(2)}`;
                    const discount = item.highestDiscount > 0
                        ? `£${item.priceAfterDiscount.toFixed(2)} (${(item.highestDiscount * 100).toFixed(0)}% off)`
                        : '-';
                    const status = item.stockStatus === "In Stock"
                        ? `In Stock (${item.stockQuantity})`
                        : "Sold Out";

                    tr.innerHTML = `
                    <td>${item.reference}</td>
                    <td>${item.name}</td>
                    <td>${originalPrice}</td>
                    <td>${discount}</td>
                    <td>${status}</td>
                    <td>
                        <a href="#" onclick="viewSelectedItem(event, '${item.id}')">View</a> |
                        <a href="#edit" onclick="editSelectedItem(event, '${item.id}')">Edit</a> |
                        <a href="#" onclick="deleteSelectedItem(event, '${item.id}')">Delete</a>
                    </td>
                `;

                // Highlight the row if it matches the passed itemId
                if (itemId && item.id === itemId) {
                    tr.classList.add('highlight-row');

                    setTimeout(() => {
                        tr.scrollIntoView({ behavior: 'smooth', block: 'center' });
                    }, 100);
                }

                tbody.appendChild(tr);
                });

                if (itemId) {
                    // show details of highlighted item
                    loadViewItemDetails(itemId);
                } else if (data.length > 0) {
                    // show the details of first item if no higlight
                    loadViewItemDetails(data[0].id);
                }
            })
            .catch(error => {
                console.error("Error fetching items:", error);
            });
    }

    // 'Create New Item' button click
    createItemBtn.addEventListener('click', (e) => {
        e.preventDefault();
        createForm.reset();

        // Remove all variation rows
        while (variationRows.firstChild) {
            variationRows.removeChild(variationRows.firstChild);
        }

        // show only create item section hiding others
        createItemSection.style.display = 'block';
        createItemSection.scrollIntoView({ behavior: 'smooth' });
        viewItemSection.style.display = 'none';
        editItemSection.style.display = 'none';
    });

    // Create '+ Variation' button click
    createAddVariationBtn.addEventListener('click', (e) => {
        e.preventDefault();

        const isHidden = window.getComputedStyle(variationSection).display === 'none';

        if (isHidden) {
            variationSection.style.display = 'block';
        }

        // Add new variation row
        const newRow = document.createElement('div');
        newRow.classList.add('variation-row');
        newRow.innerHTML = `
            <label>Size:</label>
            <input type="text" name="size[]" required>
            <label>Quantity:</label>
            <input type="number" name="quantity[]" min="0" required>
          `;
        variationRows.appendChild(newRow);
    });

    // Create '- Variation' button click
    createRemoveVariationBtn.addEventListener('click', (e) => {
        e.preventDefault();

        const rows = variationRows.getElementsByClassName('variation-row');

        if (rows.length > 0) {
            variationRows.removeChild(rows[rows.length - 1]);
        }

        // Hide the section when no variation rows are left
        if (rows.length === 0) {
            variationSection.style.display = 'none';
        }
    });

    // Show popup
    function showPopup(message, itemId = null) {
        document.getElementById('popup-message').textContent = message;

        // Store the ID in the button's dataset
        const okBtn = document.getElementById('popup-ok-btn');
        okBtn.dataset.itemId = itemId || '';

        document.getElementById('popup').style.display = 'flex';
    }

    // Popup OK button click
    popupOkBtn.addEventListener('click', (e) => {
        e.preventDefault();
        document.getElementById('popup').style.display = 'none';

        const itemId = e.target.dataset.itemId;

        // Reload the stock table and display the item's details
        if (itemId) {
            loadStockTable(itemId);
        } else {
            loadStockTable(); // fallback if no item id
        }
    });

    // Create Item
    createItemSubmitBtn.addEventListener('click', (e) => {
        e.preventDefault();

        // Gather form data
        const reference = document.getElementById('create-reference').value;
        const name = document.getElementById('create-name').value;
        const price = parseFloat(document.getElementById('create-price').value);

        // Gather variations
        const sizeInputs = document.querySelectorAll('input[name="size[]"]');
        const quantityInputs = document.querySelectorAll('input[name="quantity[]"]');
        const variations = [];

        for (let i = 0; i < sizeInputs.length; i++) {
            const size = sizeInputs[i].value;
            const quantity = parseInt(quantityInputs[i].value);
            if (size && !isNaN(quantity)) {
                variations.push({ size, quantity });
            }
        }

        // Prepare data payload
        const payload = {
            reference,
            name,
            price,
            variations
        };

        // Send POST request to create new item
        fetch('/api/Items', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to create item.');
                }
                return response.json();
            })
            .then(data => {
                const createdItemId = data.id;

                // Hide the create form
                createItemSection.style.display = 'none';

                // Clear the form fields
                createForm.reset();
                createVariationRows.innerHTML = '';

                showPopup(data.message || 'Item created successfully!', createdItemId);
            })
            .catch(error => {
                console.error('Error creating item:', error);
            });
    });

    // Delete Item
    function deleteItem(itemId) {
        // Send DELETE request to delete item
        fetch(`/api/Items/${itemId}`, {
            method: 'DELETE'
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`Failed to delete item.`);
                }
                return response.json();
            })
            .then(data => {
                const deletedItemId = data.id;
                showPopup(data.message || 'Item deleted successfully!', deletedItemId);
            })
            .catch(error => {
                console.error("Error deleting item:", error);
            });
    }

    // Edit '- Variation' button click
    editAddVariationBtn.addEventListener('click', () => {
        const newRow = document.createElement('div');
        newRow.classList.add('variation-row');
        newRow.innerHTML = `
            <label>Size:</label>
            <input type="text" name="edit-size[]" required>
            <label>Quantity:</label>
            <input type="number" name="edit-quantity[]" min="0" required>
          `;
        editVariationRows.appendChild(newRow);
    });

    // Edit '- Variation' button click
    editRemoveVariationBtn.addEventListener('click', () => {
        const rows = editVariationRows.getElementsByClassName('variation-row');
        if (rows.length > 0) {
            editVariationRows.removeChild(rows[rows.length - 1]);
        }
    });

    // Edit Item
    editItemSubmitBtn.addEventListener('click', (e) => {
        e.preventDefault();

        // Gather form data
        const id = document.getElementById('edit-id').value;
        const reference = document.getElementById('edit-reference').value;
        const name = document.getElementById('edit-name').value;
        const price = parseFloat(document.getElementById('edit-price').value);

        // Gather variations
        const sizeInputs = document.querySelectorAll('input[name="size[]"]');
        const quantityInputs = document.querySelectorAll('input[name="quantity[]"]');
        const variations = [];

        for (let i = 0; i < sizeInputs.length; i++) {
            const size = sizeInputs[i].value;
            const quantity = parseInt(quantityInputs[i].value);
            if (size && !isNaN(quantity)) {
                variations.push({ size, quantity });
            }
        }

        // Prepare data payload
        const payload = {
            id,
            reference,
            name,
            price,
            variations
        };

        // Send PUT request to edit item
        fetch(`/api/Items/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to create item.');
                }
                return response.json();
            })
            .then(data => {
                const updatedItemId = data.id;

                // Hide the edit form
                editItemSection.style.display = 'none';

                // Clear the form fields
                editForm.reset();
                editVariationRows.innerHTML = '';

                showPopup(data.message || 'Item updated successfully!', updatedItemId);
            })
            .catch(error => {
                console.error('Error creating item:', error);
            });
    });

    // Load edit form
    function loadEditItemDetails(itemId) {
        fetch(`/api/Items/${itemId}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error("Failed to load item details.");
                }
                return response.json();
            })
            .then(item => {
                // Bind properties in edit-form
                document.getElementById('edit-id').value = item.id;
                document.getElementById('edit-reference').value = item.reference;
                document.getElementById('edit-name').value = item.name;
                document.getElementById('edit-price').value = item.price.toFixed(2);

                // Clear existing variation rows
                const variationContainer = document.getElementById('edit-variation-rows');
                variationContainer.innerHTML = '';

                // Add variations
                item.variations.forEach(variation => {
                    const row = document.createElement('div');
                    row.classList.add('variation-row');
                    row.innerHTML = `
                    <label>Size:</label>
                    <input type="text" name="edit-size[]" value="${variation.size}" required>
                    <label>Quantity:</label>
                    <input type="number" name="edit-quantity[]" value="${variation.quantity}" min="0" required>
                `;
                    variationContainer.appendChild(row);
                });

                // Show the edit item section
                editItemSection.style.display = 'block';
                editItemSection.scrollIntoView({ behavior: 'smooth' });
            })
            .catch(error => {
                console.error("Error fetching item details:", error);
            });
    }

    // 'Edit' button click
    editItemBtn.addEventListener('click', (e) => {
        e.preventDefault();
        editForm.reset();

        // show only create item section hiding others
        editItemSection.style.display = 'block';
        editItemSection.scrollIntoView({ behavior: 'smooth' });
        viewItemSection.style.display = 'none';
        createItemSection.style.display = 'none';

        const itemId = e.target.dataset.itemId;
        loadEditItemDetails(itemId);
    });

    window.viewSelectedItem = function (event, itemId) {
        event.preventDefault();
        createItemSection.style.display = 'none';
        editItemSection.style.display = 'none';
        loadViewItemDetails(itemId);
    };

    window.deleteSelectedItem = function (event, itemId) {
        event.preventDefault();
        deleteItem(itemId);
    };

    window.editSelectedItem = function (event, itemId) {
        event.preventDefault();
        editForm.reset();

        // show only create item section hiding others
        editItemSection.style.display = 'block';
        editItemSection.scrollIntoView({ behavior: 'smooth' });
        createItemSection.style.display = 'none';
        viewItemSection.style.display = 'none';
        loadEditItemDetails(itemId);
    };
});
