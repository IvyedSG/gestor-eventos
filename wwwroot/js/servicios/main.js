// Archivo principal que inicializa todo
import { initializeServiceForm } from './service-form.js';
import { initializeServiceActions } from './service-actions.js';
import { initializeServiceModals } from './service-modals.js';
import { initializeAlerts } from './alerts.js';
import { showAlert } from './utils.js';

// Variables globales necesarias para mantener compatibilidad
window.selectedItems = [];
window.serviceToEdit = null;
window.currentItems = [];
window.itemsToRemove = [];
window.newItems = [];

document.addEventListener('DOMContentLoaded', function() {
    try {
        console.log('DOM Content Loaded - Initializing Services Module');
        
        // Inicializar diferentes módulos
        initializeAlerts();
        initializeServiceForm();
        initializeServiceActions();
        initializeServiceModals();
        
        // Mostrar mensaje de éxito si existe
        const urlParams = new URLSearchParams(window.location.search);
        const successMessage = urlParams.get('success');
        if (successMessage) {
            showAlert('success', decodeURIComponent(successMessage));
        }
        
        console.log('Services module initialized successfully');
    } catch (error) {
        console.error("Error in DOMContentLoaded:", error);
        showAlert('error', 'Error al inicializar la aplicación');
        
        // Fallback: si hay errores en los módulos, mostrar mensaje pero no bloquear
        console.log('Attempting to continue with basic functionality...');
    }
});

// Asegurar que las funciones críticas estén disponibles globalmente
window.addEventListener('load', function() {
    // Verificar que los elementos críticos existan
    const criticalElements = [
        'newServiceModal',
        'editServiceModal',
        'deleteServiceModal'
    ];
    
    const missingElements = criticalElements.filter(id => !document.getElementById(id));
    
    if (missingElements.length > 0) {
        console.warn('Missing critical modal elements:', missingElements);
    }
});