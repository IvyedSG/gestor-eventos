// Manejo de acciones sobre ítems de inventario existentes

import { showAlert, getAntiForgeryToken, setButtonLoading, restoreButton, getItemDataFromRow } from './utils.js';

export function initializeInventoryActions() {
    console.log('Initializing inventory actions...');
    
    try {
        setupEditButtons();
        setupDeleteButtons();
        setupViewDetailsButtons();
        setupDeleteConfirmation();
        console.log('Inventory actions initialized successfully');
    } catch (error) {
        console.error('Error initializing inventory actions:', error);
        throw error; // Re-lanzar el error para que se maneje en main.js
    }
}

function setupEditButtons() {
    const editButtons = document.querySelectorAll('.edit-item-btn');
    console.log(`Found ${editButtons.length} edit buttons`);
    
    if (editButtons.length === 0) {
        console.warn('No edit buttons found - this might indicate a timing issue');
    }
    
    editButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            console.log('Edit button clicked, item ID:', this.dataset.id);
            
            const itemId = this.dataset.id;
            const originalHtml = setButtonLoading(this, '');
            
            try {
                const row = this.closest('tr');
                if (!row) {
                    throw new Error('No se pudo encontrar la fila del ítem');
                }
                
                const itemData = getItemDataFromRow(row);
                console.log('Item data extracted:', itemData);
                
                loadItemForEdit(itemId, itemData);
                
            } catch (error) {
                console.error('Error:', error);
                showAlert('danger', 'Error al cargar los datos del ítem: ' + error.message);
            } finally {
                restoreButton(this, originalHtml);
            }
        });
    });
}

function setupDeleteButtons() {
    const deleteButtons = document.querySelectorAll('.delete-item-btn');
    console.log(`Found ${deleteButtons.length} delete buttons`);
    
    if (deleteButtons.length === 0) {
        console.warn('No delete buttons found - this might indicate a timing issue');
    }
    
    deleteButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            console.log('Delete button clicked, item ID:', this.dataset.id);
            
            const itemId = this.dataset.id;
            const itemName = this.dataset.name;
            
            loadItemForDelete(itemId, itemName);
        });
    });
}

function setupViewDetailsButtons() {
    const viewDetailsButtons = document.querySelectorAll('.view-details-btn');
    console.log(`Found ${viewDetailsButtons.length} view details buttons`);
    
    if (viewDetailsButtons.length === 0) {
        console.warn('No view details buttons found - this might indicate a timing issue');
    }
    
    viewDetailsButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            console.log('View details button clicked, item ID:', this.dataset.id);
            
            const itemId = this.dataset.id;
            const originalHtml = setButtonLoading(this, '');
            
            try {
                const row = this.closest('tr');
                if (!row) {
                    throw new Error('No se pudo encontrar la fila del ítem');
                }
                
                const itemData = getItemDataFromRow(row);
                console.log('Item data for details:', itemData);
                
                loadItemDetails(itemData);
                
            } catch (error) {
                console.error('Error:', error);
                showAlert('danger', 'Error al cargar los detalles del ítem: ' + error.message);
            } finally {
                restoreButton(this, originalHtml);
            }
        });
    });
}

function setupDeleteConfirmation() {
    const confirmDeleteBtn = document.getElementById('confirmDelete');
    if (confirmDeleteBtn) {
        confirmDeleteBtn.addEventListener('click', handleDeleteItem);
        console.log('Delete confirmation button configured');
    } else {
        console.warn('Confirm delete button not found');
    }
}

function loadItemForEdit(itemId, itemData) {
    console.log('Loading item for edit:', itemId, itemData);
    
    try {
        // Verificar que el modal existe
        const editModal = document.getElementById('editItemModal');
        if (!editModal) {
            throw new Error('Modal de edición no encontrado');
        }
        
        // Poblar formulario de edición
        const editItemId = document.getElementById('editItemId');
        const editItemName = document.getElementById('editItemName');
        const editCurrentStock = document.getElementById('editCurrentStock');
        const editItemPrice = document.getElementById('editItemPrice');
        const editItemNotes = document.getElementById('editItemNotes');
        
        if (!editItemId || !editItemName || !editCurrentStock || !editItemPrice || !editItemNotes) {
            throw new Error('Campos del formulario de edición no encontrados');
        }
        
        editItemId.value = itemId;
        editItemName.value = itemData.nombre;
        editCurrentStock.value = itemData.stock;
        editItemPrice.value = itemData.precio;
        editItemNotes.value = itemData.descripcion;
        
        console.log('Edit form populated, showing modal');
        
        // Mostrar modal
        const modal = new bootstrap.Modal(editModal);
        modal.show();
        
    } catch (error) {
        console.error('Error loading item for edit:', error);
        showAlert('danger', 'Error al cargar el formulario de edición: ' + error.message);
    }
}

function loadItemForDelete(itemId, itemName) {
    console.log('Loading item for delete:', itemId, itemName);
    
    try {
        // Verificar que el modal existe
        const deleteModal = document.getElementById('deleteConfirmModal');
        if (!deleteModal) {
            throw new Error('Modal de eliminación no encontrado');
        }
        
        // Configurar modal de eliminación
        const deleteItemName = document.getElementById('deleteItemName');
        const confirmDelete = document.getElementById('confirmDelete');
        
        if (!deleteItemName || !confirmDelete) {
            throw new Error('Elementos del modal de eliminación no encontrados');
        }
        
        deleteItemName.textContent = itemName;
        confirmDelete.dataset.id = itemId;
        
        console.log('Delete modal configured, showing modal');
        
        // Mostrar modal
        const modal = new bootstrap.Modal(deleteModal);
        modal.show();
        
    } catch (error) {
        console.error('Error loading item for delete:', error);
        showAlert('danger', 'Error al cargar el modal de eliminación: ' + error.message);
    }
}

function loadItemDetails(itemData) {
    console.log('Loading item details:', itemData);
    
    try {
        // Verificar que el modal existe
        const detailsModal = document.getElementById('itemDetailsModal');
        if (!detailsModal) {
            throw new Error('Modal de detalles no encontrado');
        }
        
        // Poblar modal de detalles
        const detailItemName = document.getElementById('detailItemName');
        const detailItemStock = document.getElementById('detailItemStock');
        const detailItemStockDisponible = document.getElementById('detailItemStockDisponible');
        const detailItemsEnUso = document.getElementById('detailItemsEnUso');
        const detailItemDescription = document.getElementById('detailItemDescription');
        
        if (!detailItemName || !detailItemStock || !detailItemStockDisponible || 
            !detailItemsEnUso || !detailItemDescription) {
            throw new Error('Elementos del modal de detalles no encontrados');
        }
        
        detailItemName.textContent = itemData.nombre;
        detailItemStock.textContent = `${itemData.stock} unidades`;
        detailItemStockDisponible.textContent = `${itemData.stockDisponible} unidades`;
        detailItemsEnUso.textContent = `${itemData.itemsEnUso} unidades`;
        detailItemDescription.textContent = itemData.descripcion || 'Sin descripción disponible.';
        
        console.log('Details modal populated, showing modal');
        
        // Mostrar modal
        const modal = new bootstrap.Modal(detailsModal);
        modal.show();
        
    } catch (error) {
        console.error('Error loading item details:', error);
        showAlert('danger', 'Error al cargar los detalles del ítem: ' + error.message);
    }
}

async function handleDeleteItem() {
    const button = document.getElementById('confirmDelete');
    const itemId = button.dataset.id;
    
    const originalHtml = setButtonLoading(button, 'Eliminando...');
    
    try {
        if (!itemId) {
            throw new Error('ID de ítem no válido');
        }
        
        console.log('Deleting item ID:', itemId);
        
        const token = getAntiForgeryToken();
        
        const response = await fetch(`?handler=DeleteItem&id=${itemId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            }
        });
        
        const data = await response.json();
        
        // Cerrar modal
        const modal = bootstrap.Modal.getInstance(document.getElementById('deleteConfirmModal'));
        if (modal) {
            modal.hide();
        }
        
        if (data.success) {
            showAlert('success', 'Ítem eliminado correctamente');
            
            // Recargar página después de un breve delay
            setTimeout(() => {
                window.location.reload();
            }, 1000);
        } else {
            throw new Error(data.message || 'Error al eliminar el ítem');
        }
        
    } catch (error) {
        console.error('Error deleting item:', error);
        showAlert('danger', 'Error al procesar la solicitud de eliminación: ' + error.message);
    } finally {
        restoreButton(button, originalHtml);
    }
}