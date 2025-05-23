document.addEventListener('DOMContentLoaded', () => {
    const viewItemSection = document.getElementById('view-item-section');
    const createItemSection = document.getElementById('create-item-section');
    const editItemSection = document.getElementById('edit-item-section'); 
    const viewVariationsTable = document.getElementById('view-variations-table');
    const createItemBtn = document.getElementById('create-item-btn');
    const createAddVariationBtn = document.getElementById('create-add-variation-btn');
    const createRemoveVariationBtn = document.getElementById('create-remove-variation-btn');
    const createVariationRows = document.getElementById('create-variation-rows');
    const createForm = document.getElementById('create-form');
    const popupOkBtn = document.getElementById('popup-ok-btn');
    const editItemBtn = document.getElementById('edit-item-btn');
    const editForm = document.getElementById('edit-form');
    const editAddVariationBtn = document.getElementById('edit-add-row-btn');
    const editRemoveVariationBtn = document.getElementById('edit-remove-row-btn');
    const editVariationRows = document.getElementById('edit-variation-rows');

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
        while (createVariationRows.firstChild) {
            createVariationRows.removeChild(createVariationRows.firstChild);
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

        // Add new variation row
        const newRow = document.createElement('div');
        newRow.classList.add('variation-row');
        newRow.innerHTML = `
            <label>Size:</label>
            <input type="text" name="create-size[]" required>
            <span class="error-message size-error"></span>
            <label>Quantity:</label>
            <input type="number" name="create-quantity[]" min="0" required>
            <span class="error-message quantity-error"></span>
          `;
        createVariationRows.appendChild(newRow);
    });

    // Create '- Variation' button click
    createRemoveVariationBtn.addEventListener('click', (e) => {
        e.preventDefault();

        const rows = createVariationRows.getElementsByClassName('variation-row');

        if (rows.length > 0) {
            createVariationRows.removeChild(rows[rows.length - 1]);
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
    function createItem() {
        // Gather form data
        const reference = document.getElementById('create-reference').value;
        const name = document.getElementById('create-name').value;
        const price = parseFloat(document.getElementById('create-price').value);

        // Gather variations
        const sizeInputs = document.querySelectorAll('input[name="create-size[]"]');
        const quantityInputs = document.querySelectorAll('input[name="create-quantity[]"]');
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
    }

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

    // Edit '+ Variation' button click
    editAddVariationBtn.addEventListener('click', () => {
        const newRow = document.createElement('div');
        newRow.classList.add('variation-row');
        newRow.innerHTML = `
             <label>Variation ID:</label>
             <input type="text" name="edit-variation-id" disabled>
             <label>Size:</label>
             <input type="text" name="edit-size[]" required>
             <span class="error-message size-error"></span>
             <label>Quantity:</label>
             <input type="number" name="edit-quantity[]" min="0" required>
             <span class="error-message quantity-error"></span>
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
    function editItem() {
        // Gather form data
        const id = document.getElementById('edit-id').value;
        const reference = document.getElementById('edit-reference').value;
        const name = document.getElementById('edit-name').value;
        const price = parseFloat(document.getElementById('edit-price').value);

        // Gather variations
        const varIdInputs = document.querySelectorAll('input[name="edit-variation-id[]"]');
        const sizeInputs = document.querySelectorAll('input[name="edit-size[]"]');
        const quantityInputs = document.querySelectorAll('input[name="edit-quantity[]"]');
        const variations = [];

        for (let i = 0; i < sizeInputs.length; i++) {
            const varId = varIdInputs[i];
            const size = sizeInputs[i].value;
            const quantity = parseInt(quantityInputs[i].value);

            if (!size || isNaN(quantity)) continue;

            const variation = {
                id: null,   // variation id has to be null for new variations added
                itemId: id,
                size,
                quantity
            };

            // Set variation id if available
            if (varId && varId.value) {
                variation.id = varId.value;
            }

            variations.push(variation);
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
    }

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
                    <label>Variation ID:</label>
                    <input type="text" name="edit-variation-id[]" value="${variation.id}" disabled>
                    <label>Size:</label>
                    <input type="text" name="edit-size[]" value="${variation.size}" required>
                    <span class="error-message size-error"></span>
                    <label>Quantity:</label>
                    <input type="number" name="edit-quantity[]" value="${variation.quantity}" min="0" required>
                    <span class="error-message quantity-error"></span>
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

    // Form validations
    editForm.addEventListener('submit', (e) => {
        e.preventDefault();

        let isValid = true;

        // Clear previous error messages
        document.querySelectorAll('.error-message').forEach(span => span.textContent = '');

        // Validate Reference
        const referenceInput = document.getElementById('edit-reference');
        if (referenceInput.value.trim() === '') {
            document.getElementById('error-edit-reference').textContent = 'Reference is required.';
            isValid = false;
        }

        // Validate Name
        const nameInput = document.getElementById('edit-name');
        if (nameInput.value.trim() === '') {
            document.getElementById('error-edit-name').textContent = 'Name is required.';
            isValid = false;
        }

        // Validate Price
        const priceInput = document.getElementById('edit-price');
        if (priceInput.value.trim() === '') {
            document.getElementById('error-edit-price').textContent = 'Price is required.';
            isValid = false;
        } else if (parseFloat(priceInput.value) < 0) {
            document.getElementById('error-edit-price').textContent = 'Price cannot be negative.';
            isValid = false;
        }

        // Validate Variations
        const sizeInputs = document.querySelectorAll('input[name="edit-size[]"]');
        const quantityInputs = document.querySelectorAll('input[name="edit-quantity[]"]');

        sizeInputs.forEach((input, index) => {
            const errorSpan = input.nextElementSibling;
            if (input.value.trim() === '') {
                errorSpan.textContent = 'Size is required.';
                isValid = false;
            }
        });

        quantityInputs.forEach((input, index) => {
            const errorSpan = input.nextElementSibling;
            if (input.value.trim() === '') {
                errorSpan.textContent = 'Quantity is required.';
                isValid = false;
            } else if (parseInt(input.value) < 0) {
                errorSpan.textContent = 'Quantity cannot be negative.';
                isValid = false;
            }
        });

        if (isValid) {
            editItem();
        }
    });

    createForm.addEventListener('submit', (e) => {
        e.preventDefault();

        let isValid = true;

        // Clear previous error messages
        document.querySelectorAll('.error-message').forEach(span => span.textContent = '');

        // Validate Reference
        const referenceInput = document.getElementById('create-reference');
        if (referenceInput.value.trim() === '') {
            document.getElementById('error-create-reference').textContent = 'Reference is required.';
            isValid = false;
        }

        // Validate Name
        const nameInput = document.getElementById('create-name');
        if (nameInput.value.trim() === '') {
            document.getElementById('error-create-name').textContent = 'Name is required.';
            isValid = false;
        }

        // Validate Price
        const priceInput = document.getElementById('create-price');
        if (priceInput.value.trim() === '') {
            document.getElementById('error-create-price').textContent = 'Price is required.';
            isValid = false;
        } else if (parseFloat(priceInput.value) < 0) {
            document.getElementById('error-create-price').textContent = 'Price cannot be negative.';
            isValid = false;
        }

        // Validate Variations
        const sizeInputs = document.querySelectorAll('input[name="create-size[]"]');
        const quantityInputs = document.querySelectorAll('input[name="create-quantity[]"]');

        sizeInputs.forEach((input, index) => {
            const errorSpan = input.nextElementSibling;
            if (input.value.trim() === '') {
                errorSpan.textContent = 'Size is required.';
                isValid = false;
            }
        });

        quantityInputs.forEach((input, index) => {
            const errorSpan = input.nextElementSibling;
            if (input.value.trim() === '') {
                errorSpan.textContent = 'Quantity is required.';
                isValid = false;
            } else if (parseInt(input.value) < 0) {
                errorSpan.textContent = 'Quantity cannot be negative.';
                isValid = false;
            }
        });

        if (isValid) {
            createItem();
        }
    });
});
