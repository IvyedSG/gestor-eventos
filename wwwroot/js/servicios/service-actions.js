// Manejo de acciones sobre servicios existentes

import { showAlert, getAntiForgeryToken, setButtonLoading, restoreButton } from './utils.js';

export function initializeServiceActions() {
    try {
        setupEditButtons();
        setupDeleteButtons();
        setupDeleteConfirmation();
        console.log('Service actions initialized successfully');
    } catch (error) {
        console.error('Error initializing service actions:', error);
    }
}

function setupEditButtons() {
    const editButtons = document.querySelectorAll('.edit-service');
    console.log(`Found ${editButtons.length} edit buttons`);
    
    editButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            
            const serviceId = this.dataset.serviceId;
            console.log("Editando servicio ID:", serviceId);
            
            try {
                loadServiceForEdit(serviceId);
            } catch (error) {
                console.error('Error loading service for edit:', error);
                showAlert('error', 'Error al cargar los detalles del servicio');
            }
        });
    });
}

function setupDeleteButtons() {
    const deleteButtons = document.querySelectorAll('.btn-delete-service');
    console.log(`Found ${deleteButtons.length} delete buttons`);
    
    deleteButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            
            const serviceId = this.dataset.serviceId;
            const serviceName = this.dataset.serviceName;
            
            loadServiceForDelete(serviceId, serviceName, this);
        });
    });
}

function setupDeleteConfirmation() {
    const confirmDeleteBtn = document.getElementById('confirmDeleteServiceBtn');
    if (confirmDeleteBtn) {
        confirmDeleteBtn.addEventListener('click', handleDeleteService);
    }
}

function loadServiceForEdit(serviceId) {
    // Usar serviciosData global que se carga en la página
    const servicio = window.serviciosData.find(s => s.id === serviceId);
    
    if (!servicio) {
        throw new Error(`Servicio con ID ${serviceId} no encontrado`);
    }
    
    console.log("Detalles del servicio:", servicio);
    
    // Almacenar datos del servicio
    window.serviceToEdit = servicio;
    
    // Resetear colecciones de items
    window.currentItems = [...servicio.items];
    window.itemsToRemove = [];
    window.newItems = [];
    
    // Poblar campos del formulario
    document.getElementById('editServiceId').value = servicio.id;
    document.getElementById('editServiceName').value = servicio.nombreServicio;
    document.getElementById('editServiceDescription').value = servicio.descripcion;
    document.getElementById('editServicePrice').value = servicio.precioBase;
    
    // Actualizar tabla de items actuales
    window.updateCurrentItemsTable();
    
    // Cargar items de inventario disponibles
    window.loadAvailableInventoryItems();
    
    // Mostrar modal
    const editModal = new bootstrap.Modal(document.getElementById('editServiceModal'));
    editModal.show();
}

function loadServiceForDelete(serviceId, serviceName, buttonElement) {
    // Configurar el modal con la información del servicio
    const messageElement = document.getElementById('deleteServiceMessage');
    const idElement = document.getElementById('deleteServiceId');
    
    if (messageElement) {
        messageElement.textContent = `¿Estás seguro de que deseas eliminar el servicio "${serviceName}"? Esta acción no se puede deshacer.`;
    }
    
    if (idElement) {
        idElement.value = serviceId;
    }
    
    // Guardar referencia al botón correctamente
    const modal = document.getElementById('deleteServiceModal');
    if (modal) {
        // Almacenar el serviceId y serviceName en el modal
        modal.dataset.serviceId = serviceId;
        modal.dataset.serviceName = serviceName;
        // Almacenar la referencia del botón de manera que funcione
        modal.buttonElement = buttonElement;
    }
    
    // Mostrar modal de confirmación
    const deleteModal = new bootstrap.Modal(document.getElementById('deleteServiceModal'));
    deleteModal.show();
}

async function handleDeleteService() {
    const button = document.getElementById('confirmDeleteServiceBtn');
    const originalHtml = setButtonLoading(button, 'Eliminando...');
    
    try {
        const serviceId = document.getElementById('deleteServiceId').value;
        const modal = document.getElementById('deleteServiceModal');
        const buttonElement = modal.buttonElement; // Ahora es una referencia válida
        const serviceName = modal.dataset.serviceName;
        
        if (!serviceId) {
            throw new Error('ID de servicio no válido');
        }
        
        console.log('Eliminando servicio ID:', serviceId);
        
        const token = getAntiForgeryToken();
        
        const response = await fetch(`/api/servicios/${serviceId}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            }
        });
        
        if (!response.ok) {
            throw new Error(`Error ${response.status}: ${response.statusText}`);
        }
        
        const result = await response.json();
        
        // Cerrar modal
        const modalInstance = bootstrap.Modal.getInstance(document.getElementById('deleteServiceModal'));
        if (modalInstance) {
            modalInstance.hide();
        }
        
        showAlert('success', result.message || `Servicio "${serviceName}" eliminado correctamente`);
        
        // Eliminar la tarjeta del servicio con animación
        if (buttonElement && typeof buttonElement.closest === 'function') {
            const serviceCard = buttonElement.closest('.col-md-6');
            if (serviceCard) {
                serviceCard.style.transition = 'opacity 0.5s ease';
                serviceCard.style.opacity = '0';
                
                setTimeout(() => {
                    serviceCard.remove();
                    
                    // Si no quedan servicios, mostrar mensaje
                    const remainingCards = document.querySelectorAll('.card.shadow-sm');
                    if (remainingCards.length === 0) {
                        const container = document.querySelector('.container.py-4');
                        if (container) {
                            container.insertAdjacentHTML('beforeend', `
                                <div class="alert alert-info" role="alert">
                                    <i class="bi bi-info-circle-fill me-2"></i> No se encontraron servicios registrados.
                                </div>
                            `);
                        }
                    }
                }, 500);
            }
        } else {
            // Fallback: recargar la página si no podemos eliminar visualmente
            setTimeout(() => {
                window.location.reload();
            }, 1500);
        }
        
    } catch (error) {
        console.error('Error al eliminar el servicio:', error);
        showAlert('error', error.message || 'Ocurrió un error al eliminar el servicio');
    } finally {
        restoreButton(button, originalHtml);
    }
}