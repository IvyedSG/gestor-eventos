// Inicialización de modales y funciones globales necesarias

import { setupDeleteModal } from './delete-reservation-modal.js';

export function initializeReservationModals() {
    // Configurar modal de eliminación
    setupDeleteModal();
    
    // Configurar función global para cerrar modal de reserva
    setupReservationModalClosing();
    
    // Configurar botón de generar recibo
    setupReceiptGeneration();
}

function setupReservationModalClosing() {
    // Función global para cerrar modal de reserva
    window.closeReservationModal = function() {
        const modalElement = document.getElementById('viewReservationModal');
        if (modalElement && typeof bootstrap !== 'undefined') {
            const modal = bootstrap.Modal.getInstance(modalElement);
            if (modal) {
                modal.hide();
            }
        }
    };
}

function setupReceiptGeneration() {
    // Configurar botón de generar recibo si existe
    const generateReceiptBtn = document.getElementById('generateReceiptBtn');
    if (generateReceiptBtn) {
        generateReceiptBtn.addEventListener('click', function() {
            // Verificar si existen los datos de la reserva
            if (window.currentReservationData) {
                // Aquí puedes implementar la lógica de generación de recibo
                // o llamar a una función externa si ya existe
                console.log('Generating receipt for reservation:', window.currentReservationData.id);
                
                // Si tienes una función de generación de recibos, descoméntala:
                // if (typeof generateReceipt === 'function') {
                //     generateReceipt(window.currentReservationData, window.reservationPayments);
                // }
            } else {
                console.warn('No reservation data available for receipt generation');
            }
        });
    }
}