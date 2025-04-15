document.addEventListener('DOMContentLoaded', () => {
    const createItemSection = document.getElementById('create-item-section');
    const viewItemSection = document.getElementById('view-item-section');
    const viewItemTable = document.getElementById('view-item-table');
    const viewVariationsTable = document.getElementById('view-variations-table');

    // Hide everything before showing View on page load
    createItemSection.style.display = 'none';
    viewItemSection.style.display = 'none';

    // Load stock table on page load
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

    function loadItemDetails(id) {
        fetch(`/api/Items/${id}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error("Failed to load item details.");
                }
                return response.json();
            })
            .then(item => {
                // Load view-details table
                document.getElementById('item-id').textContent = item.id;
                document.getElementById('item-reference').textContent = item.reference;
                document.getElementById('item-name').textContent = item.name;
                document.getElementById('item-price').textContent = `£${item.price.toFixed(2)}`;

                // Load variations table
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

    window.showViewItem = function (event, id) {
        event.preventDefault();
        createItemSection.style.display = 'none';
        loadItemDetails(id);
    };
});
