// Archivo principal que inicializa todo
import { initializeReservationForm } from './reservation-form.js';
import { initializeReservationActions } from './reservation-actions.js';
import { initializeReservationModals } from './reservation-modals.js';
import { initializeEditReservationModal } from './edit-reservation-modal.js';
import { initializeFilters } from './filters.js';
import { showAlert } from './utils.js';

document.addEventListener('DOMContentLoaded', function() {
    try {
        console.log('DOM Content Loaded - Initializing Reservations Module');
        
        // Inicializar diferentes módulos
        initializeFilters();
        initializeReservationForm();
        initializeReservationActions();
        initializeReservationModals();
        initializeEditReservationModal(); // *** AGREGAR ESTA LÍNEA ***
        
        console.log('Reservations module initialized successfully');
    } catch (error) {
        console.error("Error in DOMContentLoaded:", error);
        showAlert('danger', 'Error al inicializar la aplicación');
        
        // Fallback: si hay errores en los módulos, mostrar mensaje pero no bloquear
        console.log('Attempting to continue with basic functionality...');
    }
});

// Asegurar que las funciones críticas estén disponibles globalmente
window.addEventListener('load', function() {
    // Verificar que los elementos críticos existan
    const criticalElements = [
        'newReservationModal',
        'viewReservationModal', 
        'editReservationModal',
        'deleteReservationModal'
    ];
    
    const missingElements = criticalElements.filter(id => !document.getElementById(id));
    
    if (missingElements.length > 0) {
        console.warn('Missing critical modal elements:', missingElements);
    }
    
    const updateButton = document.getElementById('updateReservationBtn');
    if (updateButton) {
        console.log('Update button found and should be connected');
    } else {
        console.warn('Update button not found in DOM');
    }
});