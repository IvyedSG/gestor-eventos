// Manejo de formularios de servicio (crear/editar)

import { showAlert, getAntiForgeryToken, setButtonLoading, restoreButton, validateRequiredFields } from './utils.js';

export function initializeServiceForm() {
    setupNewServiceForm();
    setupEditServiceForm();
    setupItemManagement();
    console.log('Service forms initialized successfully');
}

function setupNewServiceForm() {
    const saveButton = document.getElementById('saveServiceBtn');
    if (saveButton) {
        saveButton.addEventListener('click', handleCreateService);
    }
    
    const addItemButton = document.getElementById('addSelectedItemBtn');
    if (addItemButton) {
        addItemButton.addEventListener('click', handleAddItem);
    }
}

function setupEditServiceForm() {
    const saveEditButton = document.getElementById('saveEditServiceBtn');
    if (saveEditButton) {
        saveEditButton.addEventListener('click', handleUpdateService);
    }
    
    const addEditItemButton = document.getElementById('addEditItemBtn');
    if (addEditItemButton) {
        addEditItemButton.addEventListener('click', handleAddEditItem);
    }
}

function setupItemManagement() {
    // Configurar validación en tiempo real
    const serviceNameInput = document.getElementById('serviceName');
    const serviceDescInput = document.getElementById('serviceDescription');
    const servicePriceInput = document.getElementById('servicePrice');
    
    [serviceNameInput, serviceDescInput, servicePriceInput].forEach(input => {
        if (input) {
            input.addEventListener('input', function() {
                this.classList.remove('is-invalid');
            });
        }
    });
}

async function handleCreateService() {
    const button = document.getElementById('saveServiceBtn');
    const originalHtml = setButtonLoading(button, 'Guardando...');
    
    try {
        // Validar campos requeridos
        const fieldsToValidate = [
            { id: 'serviceName', type: 'text' },
            { id: 'serviceDescription', type: 'text' },
            { id: 'servicePrice', type: 'number' }
        ];
        
        const isValid = validateRequiredFields(fieldsToValidate);
        
        if (!isValid || window.selectedItems.length === 0) {
            if (window.selectedItems.length === 0) {
                const itemsTable = document.getElementById('itemsTable');
                const itemsHelp = document.getElementById('itemsHelp');
                
                if (itemsTable) itemsTable.classList.add('border', 'border-danger');
                if (itemsHelp) itemsHelp.classList.remove('d-none');
            }
            
            showAlert('warning', 'Por favor completa todos los campos requeridos');
            return;
        }
        
        // Preparar datos
        const serviceData = {
            nombreServicio: document.getElementById('serviceName').value,
            descripcion: document.getElementById('serviceDescription').value,
            precioBase: parseFloat(document.getElementById('servicePrice').value),
            items: window.selectedItems.map(item => ({
                inventarioId: item.id,
                cantidad: item.cantidad,
                estado: item.estado,
                precioActual: String(item.precio)
            }))
        };
        
        console.log('Enviando datos:', serviceData);
        
        const token = getAntiForgeryToken();
        
        const response = await fetch('/Servicios', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(serviceData)
        });
        
        if (!response.ok) {
            const errorText = await response.text();
            console.error('Response error:', response.status, errorText);
            throw new Error(`Error ${response.status}: ${response.statusText}`);
        }
        
        const result = await response.json();
        
        // Cerrar modal
        const modal = bootstrap.Modal.getInstance(document.getElementById('newServiceModal'));
        if (modal) {
            modal.hide();
        }
        
        showAlert('success', `Servicio "${serviceData.nombreServicio}" creado exitosamente`);
        
        // Recargar página
        setTimeout(() => {
            window.location.reload();
        }, 1500);
        
    } catch (error) {
        console.error('Error al guardar el servicio:', error);
        showAlert('error', error.message || 'Ocurrió un error al guardar el servicio');
    } finally {
        restoreButton(button, originalHtml);
    }
}

async function handleUpdateService() {
    const button = document.getElementById('saveEditServiceBtn');
    const originalHtml = setButtonLoading(button, 'Guardando...');
    
    try {
        const serviceId = document.getElementById('editServiceId').value;
        
        // Validar campos requeridos
        const fieldsToValidate = [
            { id: 'editServiceName', type: 'text' },
            { id: 'editServiceDescription', type: 'text' },
            { id: 'editServicePrice', type: 'number' }
        ];
        
        const isValid = validateRequiredFields(fieldsToValidate);
        
        if (!isValid) {
            showAlert('warning', 'Por favor completa todos los campos requeridos');
            return;
        }
        
        // Preparar datos
        const serviceData = {
            nombreServicio: document.getElementById('editServiceName').value,
            descripcion: document.getElementById('editServiceDescription').value,
            precioBase: parseFloat(document.getElementById('editServicePrice').value),
            itemsToAdd: window.newItems.map(item => ({
                inventarioId: item.inventarioId,
                cantidad: item.cantidad,
                estado: item.estado,
                precioActual: String(item.precioActual)
            })),
            itemsToRemove: window.itemsToRemove
        };
        
        console.log('Enviando datos de edición:', serviceData);
        
        const token = getAntiForgeryToken();
        
        const response = await fetch(`/api/servicios/${serviceId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(serviceData)
        });
        
        if (!response.ok) {
            throw new Error(`Error ${response.status}: ${response.statusText}`);
        }
        
        const result = await response.json();
        
        // Cerrar modal
        const modal = bootstrap.Modal.getInstance(document.getElementById('editServiceModal'));
        if (modal) {
            modal.hide();
        }
        
        showAlert('success', `Servicio "${serviceData.nombreServicio}" actualizado exitosamente`);
        
        // Recargar página
        setTimeout(() => {
            window.location.reload();
        }, 1500);
        
    } catch (error) {
        console.error('Error al actualizar el servicio:', error);
        showAlert('error', error.message || 'Ocurrió un error al actualizar el servicio');
    } finally {
        restoreButton(button, originalHtml);
    }
}

function handleAddItem() {
    const itemSelect = document.getElementById('itemSelect');
    const selectedOption = itemSelect.options[itemSelect.selectedIndex];
    const itemId = itemSelect.value;
    const itemQuantity = parseInt(document.getElementById('itemQuantity').value);
    const itemState = document.getElementById('itemState').value;
    
    if (!itemId) {
        showAlert('warning', 'Por favor selecciona un ítem');
        return;
    }
    
    if (isNaN(itemQuantity) || itemQuantity < 1) {
        showAlert('warning', 'La cantidad debe ser al menos 1');
        return;
    }
    
    const itemStock = parseInt(selectedOption.dataset.stock);
    if (itemQuantity > itemStock) {
        showAlert('warning', `Solo hay ${itemStock} unidades disponibles`);
        return;
    }
    
    const existingItem = window.selectedItems.find(item => item.id === itemId);
    if (existingItem) {
        showAlert('warning', 'Este ítem ya ha sido agregado');
        return;
    }
    
    const item = {
        id: itemId,
        nombre: selectedOption.dataset.nombre,
        estado: itemState,
        stock: itemStock,
        cantidad: itemQuantity,
        precio: selectedOption.dataset.precio
    };
    
    window.selectedItems.push(item);
    window.updateItemsTable();
    window.updateSaveButtonState();
    
    itemSelect.value = '';
    document.getElementById('itemQuantity').value = 1;
    document.getElementById('itemState').value = 'Nuevo';
    
    showAlert('success', 'Ítem agregado al servicio');
}

function handleAddEditItem() {
    const itemSelect = document.getElementById('editItemSelect');
    const selectedOption = itemSelect.options[itemSelect.selectedIndex];
    const itemId = itemSelect.value;
    const itemQuantity = parseInt(document.getElementById('editItemQuantity').value);
    const itemState = document.getElementById('editItemState').value;
    
    if (!itemId) {
        showAlert('warning', 'Por favor selecciona un ítem');
        return;
    }
    
    if (isNaN(itemQuantity) || itemQuantity < 1) {
        showAlert('warning', 'La cantidad debe ser al menos 1');
        return;
    }
    
    const itemStock = parseInt(selectedOption.dataset.stock);
    if (itemQuantity > itemStock) {
        showAlert('warning', `Solo hay ${itemStock} unidades disponibles`);
        return;
    }
    
    if (window.newItems.some(item => item.inventarioId === itemId)) {
        showAlert('warning', 'Este ítem ya ha sido agregado');
        return;
    }
    
    const item = {
        inventarioId: itemId,
        nombreItem: selectedOption.dataset.nombre,
        estado: itemState,
        stockDisponible: itemStock,
        cantidad: itemQuantity,
        precioActual: selectedOption.dataset.precio
    };
    
    window.newItems.push(item);
    window.updateNewItemsTable();
    
    itemSelect.value = '';
    document.getElementById('editItemQuantity').value = 1;
    document.getElementById('editItemState').value = 'Nuevo';
    
    showAlert('success', 'Ítem agregado al servicio');
}