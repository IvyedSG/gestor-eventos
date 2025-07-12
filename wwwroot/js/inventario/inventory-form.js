// Manejo de formularios de inventario (crear/editar)

import { showAlert, getAntiForgeryToken, setButtonLoading, restoreButton, validateRequiredFields } from './utils.js';

export function initializeInventoryForm() {
    setupNewItemForm();
    setupEditItemForm();
    console.log('Inventory forms initialized successfully');
}

function setupNewItemForm() {
    const saveNewItemBtn = document.getElementById('saveNewItem');
    if (saveNewItemBtn) {
        saveNewItemBtn.addEventListener('click', handleCreateItem);
    }
}

function setupEditItemForm() {
    const saveEditItemBtn = document.getElementById('saveEditItem');
    if (saveEditItemBtn) {
        saveEditItemBtn.addEventListener('click', handleUpdateItem);
    }
}

async function handleCreateItem() {
    const button = document.getElementById('saveNewItem');
    const form = document.getElementById('newItemForm');
    
    if (!form.checkValidity()) {
        form.reportValidity();
        return;
    }
    
    const originalHtml = setButtonLoading(button, 'Guardando...');
    
    try {
        // Validar campos requeridos
        const fieldsToValidate = ['itemName', 'itemStock', 'itemPrice'];
        const isValid = validateRequiredFields(fieldsToValidate);
        
        if (!isValid) {
            showAlert('warning', 'Por favor completa todos los campos requeridos correctamente');
            return;
        }
        
        // Preparar datos
        const itemData = {
            nombre: document.getElementById('itemName').value,
            descripcion: document.getElementById('itemDescription').value || "",
            stock: parseInt(document.getElementById('itemStock').value) || 0,
            preciobase: document.getElementById('itemPrice').value || "0"
        };
        
        console.log('Enviando datos:', itemData);
        
        const token = getAntiForgeryToken();
        
        const response = await fetch('?handler=SaveItem', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(itemData)
        });
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        const data = await response.json();
        
        if (data.success) {
            showAlert('success', 'Ítem creado correctamente');
            
            // Cerrar modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('newItemModal'));
            if (modal) {
                modal.hide();
            }
            
            // Recargar página después de un breve delay
            setTimeout(() => {
                window.location.reload();
            }, 1000);
        } else {
            throw new Error(data.message || 'Error al crear el ítem');
        }
        
    } catch (error) {
        console.error('Error:', error);
        showAlert('danger', 'Error al procesar la solicitud: ' + error.message);
    } finally {
        restoreButton(button, originalHtml);
    }
}

async function handleUpdateItem() {
    const button = document.getElementById('saveEditItem');
    const form = document.getElementById('editItemForm');
    
    if (!form.checkValidity()) {
        form.reportValidity();
        return;
    }
    
    const originalHtml = setButtonLoading(button, 'Guardando...');
    
    try {
        const itemId = document.getElementById('editItemId').value;
        
        // Validar campos requeridos
        const fieldsToValidate = ['editItemName', 'editCurrentStock', 'editItemPrice'];
        const isValid = validateRequiredFields(fieldsToValidate);
        
        if (!isValid) {
            showAlert('warning', 'Por favor completa todos los campos requeridos correctamente');
            return;
        }
        
        // Preparar datos
        const itemData = {
            nombre: document.getElementById('editItemName').value,
            descripcion: document.getElementById('editItemNotes').value || "",
            stock: parseInt(document.getElementById('editCurrentStock').value) || 0,
            preciobase: document.getElementById('editItemPrice').value || "0"
        };
        
        console.log('Actualizando ítem:', itemId, itemData);
        
        const token = getAntiForgeryToken();
        
        const response = await fetch(`?handler=UpdateItem&id=${itemId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(itemData)
        });
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        const data = await response.json();
        
        if (data.success) {
            showAlert('success', 'Ítem actualizado correctamente');
            
            // Cerrar modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('editItemModal'));
            if (modal) {
                modal.hide();
            }
            
            // Recargar página después de un breve delay
            setTimeout(() => {
                window.location.reload();
            }, 1000);
        } else {
            throw new Error(data.message || 'Error al actualizar el ítem');
        }
        
    } catch (error) {
        console.error('Error:', error);
        showAlert('danger', 'Error al procesar la solicitud: ' + error.message);
    } finally {
        restoreButton(button, originalHtml);
    }
}