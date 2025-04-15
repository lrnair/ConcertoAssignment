document.addEventListener('DOMContentLoaded', () => {
    const viewItemSection = document.getElementById('view-item-section');
    const createItemSection = document.getElementById('create-item-section');
    const editItemSection = document.getElementById('edit-item-section'); 
    const viewVariationsTable = document.getElementById('view-variations-table');
    const createItemBtn = document.getElementById('create-item-btn');
    const addVariationBtn = document.getElementById('add-variation-btn');
    const removeVariationBtn = document.getElementById('remove-variation-btn');
    const variationSection = document.getElementById('variation-section');
    const variationRows = document.getElementById('variation-rows');
    const createForm = document.getElementById('create-form');
    const createItemSubmitBtn = document.getElementById('create-submit-btn');

    // Hide all sections on page load
    viewItemSection.style.display = 'none';
    createItemSection.style.display = 'none';
    editItemSection.style.display = 'none';

    // Load stock table on page load
    loadStockTable()

    // View Item
    function loadItemDetails(id) {
        fetch(`/api/Items/${id}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error("Failed to load item details.");
                }
                return response.json();
            })
            .then(item => {
                // Bind properties in view-item-table
                document.getElementById('item-id').textContent = item.id;
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
    function loadStockTable() {
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
                        <a href="#" onclick="showViewItem(event, '${item.id}')">View</a> |
                        <a href="#edit" onclick="showEditSection(event)">Edit</a> |
                        <a href="#">Delete</a>
                    </td>
                `;

                    tbody.appendChild(tr);
                });

                // Automatically load the first item's details
                if (data.length > 0) {
                    loadItemDetails(data[0].id);
                }
            })
            .catch(error => {
                console.error("Error fetching items:", error);
            });
    }

    window.showViewItem = function (event, id) {
        event.preventDefault();
        createItemSection.style.display = 'none';
        loadItemDetails(id);
    };

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

    // '+ Variation' button click
    addVariationBtn.addEventListener('click', (e) => {
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

    // '- Variation' button click
    removeVariationBtn.addEventListener('click', (e) => {
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

    // Create Item
    createItemSubmitBtn.addEventListener('click', (e) => {
        e.preventDefault();

        // Gather form data
        const reference = document.getElementById('reference').value;
        const name = document.getElementById('name').value;
        const price = parseFloat(document.getElementById('price').value);

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
                const newItemId = data.id;

                // Hide the create form
                createItemSection.style.display = 'none';

                // Clear the form fields
                document.getElementById('create-form').reset();
                document.getElementById('variation-section').style.display = 'none';
                document.getElementById('variation-rows').innerHTML = '';

                // Reload the stock table and display the new item's details
                loadStockTable(newItemId);
            })
            .catch(error => {
                console.error('Error creating item:', error);
            });
    });
});
