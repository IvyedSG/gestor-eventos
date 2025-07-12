// Archivo principal que inicializa todo
import { initializeClientForm } from './client-form.js';
import { initializeClientActions } from './client-actions.js';
import { initializeClientModals } from './client-modals.js';
import { initializeFilters } from './filters.js';
import { showAlert } from './utils.js';

document.addEventListener('DOMContentLoaded', function() {
    try {
        console.log('DOM Content Loaded - Initializing Clients Module');
        
        // Inicializar diferentes módulos
        initializeFilters();
        initializeClientForm();
        initializeClientActions();
        initializeClientModals();
        
        console.log('Clients module initialized successfully');
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
        'newClientModal',
        'editClientModal',
        'viewClientModal',
        'deleteClientModal'
    ];
    
    const missingElements = criticalElements.filter(id => !document.getElementById(id));
    
    if (missingElements.length > 0) {
        console.warn('Missing critical modal elements:', missingElements);
    }
});