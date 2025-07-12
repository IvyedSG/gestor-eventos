// Manejo de acciones sobre clientes existentes

import { showAlert, insertAlert, getAntiForgeryToken, setButtonLoading, restoreButton } from './utils.js';

export function initializeClientActions() {
    try {
        setupViewButtons();
        setupEditButtons();
        setupDeleteButtons();
        setupDeleteModal();
        console.log('Client actions initialized successfully');
    } catch (error) {
        console.error('Error initializing client actions:', error);
    }
}

function setupViewButtons() {
    const viewButtons = document.querySelectorAll('.view-client-btn');
    console.log(`Found ${viewButtons.length} view buttons`);
    
    viewButtons.forEach(button => {
        button.addEventListener('click', function() {
            const clientId = this.dataset.clientId;
            loadClientForView(clientId);
        });
    });
}

function setupEditButtons() {
    const editButtons = document.querySelectorAll('.edit-client-btn');
    console.log(`Found ${editButtons.length} edit buttons`);
    
    editButtons.forEach(button => {
        button.addEventListener('click', function() {
            const clientId = this.dataset.clientId;
            loadClientForEdit(clientId);
        });
    });
}

function setupDeleteButtons() {
    const deleteButtons = document.querySelectorAll('.delete-client-btn');
    console.log(`Found ${deleteButtons.length} delete buttons`);
    
    deleteButtons.forEach(button => {
        button.addEventListener('click', function() {
            const clientId = this.dataset.clientId;
            const clientRow = document.querySelector(`[data-client-id="${clientId}"]`);
            
            if (!clientRow) return;
            
            const row = clientRow.closest('tr');
            const clientName = row.querySelector('td:nth-child(1)').textContent.trim();
            
            // Configurar modal de eliminación
            document.getElementById('deleteClientId').value = clientId;
            document.getElementById('deleteClientName').textContent = clientName;
            
            // Mostrar modal
            const deleteModal = new bootstrap.Modal(document.getElementById('deleteClientModal'));
            deleteModal.show();
        });
    });
}

function setupDeleteModal() {
    const confirmDeleteBtn = document.getElementById('confirmDeleteClient');
    if (confirmDeleteBtn) {
        confirmDeleteBtn.addEventListener('click', function() {
            const clientId = document.getElementById('deleteClientId').value;
            const clientName = document.getElementById('deleteClientName').textContent;
            
            handleDeleteClient(clientId, clientName);
        });
    }
}

function loadClientForView(clientId) {
    const clientRow = document.querySelector(`[data-client-id="${clientId}"]`);
    
    if (!clientRow) return;
    
    const row = clientRow.closest('tr');
    
    const name = row.querySelector('td:nth-child(1)').textContent.trim();
    const email = row.querySelector('td:nth-child(2)').textContent.trim();
    const phone = row.querySelector('td:nth-child(3)').textContent.trim();
    const address = row.querySelector('td:nth-child(4)').textContent.trim();
    const typeElement = row.querySelector('td:nth-child(5) span');
    const type = typeElement ? typeElement.textContent.trim() : 'No especificado';
    const ruc = row.querySelector('td:nth-child(6)').textContent.trim();
    const razonSocial = row.querySelector('td:nth-child(7)').textContent.trim();
    const eventCount = row.querySelector('td:nth-child(8)').textContent.trim();
    const lastReservation = row.querySelector('td:nth-child(9)').textContent.trim();
    
    // Rellenar modal de vista
    document.getElementById('viewClientName').textContent = name;
    document.getElementById('viewClientEmail').textContent = email;
    document.getElementById('viewClientPhone').textContent = phone || "No disponible";
    document.getElementById('viewClientAddress').textContent = address || "No disponible";
    document.getElementById('viewClientType').textContent = type;
    document.getElementById('viewClientRuc').textContent = ruc && ruc !== "-" ? ruc : "No disponible";
    document.getElementById('viewClientRazonSocial').textContent = razonSocial && razonSocial !== "-" ? razonSocial : "No disponible";
    document.getElementById('viewTotalEvents').textContent = eventCount;
    document.getElementById('viewLastReservation').textContent = lastReservation || "No disponible";
}

function loadClientForEdit(clientId) {
    const clientRow = document.querySelector(`[data-client-id="${clientId}"]`);
    
    if (!clientRow) return;
    
    const row = clientRow.closest('tr');
    
    const name = row.querySelector('td:nth-child(1)').textContent.trim();
    const email = row.querySelector('td:nth-child(2)').textContent.trim();
    const phone = row.querySelector('td:nth-child(3)').textContent.trim();
    const address = row.querySelector('td:nth-child(4)').textContent.trim();
    const type = row.querySelector('td:nth-child(5) span').textContent.trim();
    const ruc = row.querySelector('td:nth-child(6)').textContent.trim();
    const razonSocial = row.querySelector('td:nth-child(7)').textContent.trim();
    
    // Rellenar formulario de edición
    document.getElementById('editClientId').value = clientId;
    document.getElementById('editClientName').value = name;
    document.getElementById('editClientEmail').value = email;
    document.getElementById('editClientPhone').value = phone;
    document.getElementById('editClientAddress').value = address;
    document.getElementById('editClientRuc').value = ruc !== "-" ? ruc : "";
    document.getElementById('editClientRazonSocial').value = razonSocial !== "-" ? razonSocial : "";
    
    // Configurar tipo de cliente
    const editCompanyFields = document.querySelectorAll('.edit-company-field');
    const editNameLabel = document.getElementById('editNameLabel');
    
    if (type === 'Individual') {
        document.getElementById('editIndividualClient').checked = true;
        if (editNameLabel) editNameLabel.textContent = 'Nombre completo';
        editCompanyFields.forEach(field => field.style.display = 'none');
    } else {
        document.getElementById('editCompanyClient').checked = true;
        if (editNameLabel) editNameLabel.textContent = 'Nombre de la empresa';
        editCompanyFields.forEach(field => field.style.display = 'block');
    }
}

async function handleDeleteClient(clientId, clientName) {
    const button = document.getElementById('confirmDeleteClient');
    const originalHtml = setButtonLoading(button, 'Eliminando...');
    
    try {
        const token = getAntiForgeryToken();
        
        const response = await fetch('/Clientes?handler=DeleteCliente', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({ id: clientId })
        });
        
        if (!response.ok) {
            throw new Error(`Error al eliminar cliente: ${response.statusText}`);
        }
        
        const result = await response.json();
        
        if (result.success) {
            // Cerrar modal
            const deleteModal = bootstrap.Modal.getInstance(document.getElementById('deleteClientModal'));
            deleteModal.hide();
            
            // Mostrar mensaje de éxito
            insertAlert(`Cliente "${clientName}" eliminado exitosamente`, 'success');
            
            // Remover fila de la tabla
            const clientRow = document.querySelector(`tr[data-client-id="${clientId}"]`);
            if (clientRow) {
                clientRow.remove();
            }
            
            // Verificar si quedan clientes
            const remainingRows = document.querySelectorAll('tbody tr.client-row');
            if (remainingRows.length === 0) {
                setTimeout(() => {
                    window.location.reload();
                }, 1500);
            }
        } else {
            throw new Error(result.message || 'No se pudo eliminar el cliente');
        }
    } catch (error) {
        console.error('Error:', error);
        showAlert('danger', error.message || 'Ha ocurrido un error al procesar la solicitud');
    } finally {
        restoreButton(button, originalHtml);
    }
}