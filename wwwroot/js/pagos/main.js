// Archivo principal que inicializa todo
import { initializePaymentForm } from './payment-form.js';
import { initializePaymentActions } from './payment-actions.js';
import { initializePaymentModals } from './payment-modals.js';
import { initializeFilters } from './filters.js';
import { initializeBadges } from './badges.js';
import { showAlert } from './utils.js';

document.addEventListener('DOMContentLoaded', function() {
    try {
        console.log('DOM Content Loaded - Initializing Payments Module');
        
        // Inicializar diferentes módulos
        initializeFilters();
        initializeBadges();
        initializePaymentForm();
        initializePaymentActions();
        initializePaymentModals();
        
        console.log('Payments module initialized successfully');
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
        'newPaymentModal',
        'editPaymentModal',
        'deletePaymentModal'
    ];
    
    const missingElements = criticalElements.filter(id => !document.getElementById(id));
    
    if (missingElements.length > 0) {
        console.warn('Missing critical modal elements:', missingElements);
    }
});