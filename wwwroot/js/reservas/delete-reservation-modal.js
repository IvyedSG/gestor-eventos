// Manejo del modal de eliminación de reservas

import { showAlert } from './utils.js';

export function openDeleteReservationModal(id, name, fecha) {
    console.log('Opening delete confirmation for reservation:', id);
    
    try {
        // Rellenar modal con los datos
        const idField = document.getElementById('deleteReservationId');
        const detailsField = document.getElementById('deleteReservationDetails');
        
        if (idField) idField.value = id;
        if (detailsField) detailsField.textContent = `${name} (${id}) - ${fecha}`;
        
        // Mostrar el modal usando setTimeout para evitar retornar cualquier valor
        setTimeout(() => {
            const modalElement = document.getElementById('deleteReservationModal');
            if (typeof bootstrap !== 'undefined' && modalElement) {
                // Limpiar cualquier modal anterior
                const oldBackdrop = document.querySelector('.modal-backdrop');
                if (oldBackdrop) {
                    oldBackdrop.remove();
                }
                
                // Restablecer clases del body
                document.body.classList.remove('modal-open');
                document.body.style.paddingRight = '';
                
                // Crear y mostrar el modal
                const modal = new bootstrap.Modal(modalElement);
                modal.show();
            }
        }, 0);
    } catch (error) {
        console.error('Error opening delete modal:', error);
    }
}

export async function deleteReservation() {
    try {
        const id = document.getElementById('deleteReservationId').value;
        
        if (!id) {
            console.error('No reservation ID found');
            return;
        }
        
        // Mostrar loading state en el botón
        const button = document.getElementById('confirmDeleteBtn');
        const originalHtml = button.innerHTML;
        button.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Eliminando...';
        button.disabled = true;
        
        // Obtener token CSRF
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        const token = tokenElement ? tokenElement.value : '';
        
        // Enviar solicitud
        const response = await fetch('/Reservas?handler=DeleteReservation', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({ id: id })
        });
        
        console.log('Response status:', response.status);
        
        // Leer el texto de la respuesta primero
        const responseText = await response.text();
        console.log('Response text:', responseText);
        
        // Solo intentar parsear como JSON si hay contenido
        let result = null;
        if (responseText && responseText.trim()) {
            try {
                result = JSON.parse(responseText);
                console.log('Parsed result:', result);
            } catch (e) {
                console.warn('Response is not valid JSON:', e);
            }
        }
        
        // Consideramos exitoso si la respuesta tiene código 200-299 O tenemos un resultado con success=true
        const isSuccess = response.ok || (result && result.success === true);
        
        if (isSuccess) {
            console.log('Delete operation successful');
            
            // Cerrar modal
            const modalElement = document.getElementById('deleteReservationModal');
            const modal = bootstrap.Modal.getInstance(modalElement);
            if (modal) {
                modal.hide();
                
                // Limpiar backdrop
                setTimeout(() => {
                    const backdrop = document.querySelector('.modal-backdrop');
                    if (backdrop) backdrop.remove();
                    document.body.classList.remove('modal-open');
                    document.body.style.paddingRight = '';
                }, 300);
            }
            
            // Mostrar mensaje de éxito
            showAlert('success', 'Reserva eliminada exitosamente');
            
            // Recargar página después de un breve delay
            setTimeout(() => {
                window.location.href = '/Reservas?success=true&action=delete';
            }, 1500);
            
            return;
        }
        
        // Si llegamos aquí, hubo un problema
        const errorMessage = (result && result.message) 
            ? result.message 
            : `Error ${response.status}: ${response.statusText || 'Error desconocido'}`;
            
        throw new Error(errorMessage);
        
    } catch (error) {
        console.error('Error deleting reservation:', error);
        
        // Mostrar mensaje de error
        showAlert('danger', 'Error al eliminar la reserva: ' + (error.message || 'Error desconocido'));
    } finally {
        // Restaurar estado del botón
        const button = document.getElementById('confirmDeleteBtn');
        if (button) {
            button.innerHTML = '<i class="bi bi-trash me-2"></i>Eliminar';
            button.disabled = false;
        }
    }
}

export function setupDeleteModal() {
    const deleteModal = document.getElementById('deleteReservationModal');
    if (!deleteModal) {
        console.warn('Delete modal not found');
        return;
    }
    
    const cancelButton = deleteModal.querySelector('button.btn-secondary');
    const closeButton = deleteModal.querySelector('button.btn-close');
    
    // Función para asegurar que el backdrop se elimine correctamente
    const cleanupModal = () => {
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) {
            backdrop.remove();
        }
        document.body.classList.remove('modal-open');
        document.body.style.paddingRight = '';
    };
    
    // Manejar clic en el botón Cancelar
    if (cancelButton) {
        cancelButton.addEventListener('click', function(e) {
            e.preventDefault();
            
            const modal = bootstrap.Modal.getInstance(deleteModal);
            if (modal) {
                modal.hide();
                setTimeout(cleanupModal, 300);
            }
            
            return false;
        });
    }
    
    // También manejar el botón de cierre (X) con la misma lógica
    if (closeButton) {
        closeButton.addEventListener('click', function(e) {
            e.preventDefault();
            const modal = bootstrap.Modal.getInstance(deleteModal);
            if (modal) {
                modal.hide();
                setTimeout(cleanupModal, 300);
            }
            return false;
        });
    }
    
    // Manejar evento hidden.bs.modal
    deleteModal.addEventListener('hidden.bs.modal', function(e) {
        cleanupModal();
    });
}

// Hacer disponibles globalmente para compatibilidad con el HTML existente
if (typeof window !== 'undefined') {
    window.deleteReservation = deleteReservation;
    window.openDeleteReservationModal = openDeleteReservationModal;
    window.setupDeleteModal = setupDeleteModal;
}