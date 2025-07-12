// Archivo principal que inicializa todo
import { initializeInventoryForm } from './inventory-form.js';
import { initializeInventoryActions } from './inventory-actions.js';
import { initializeInventoryModals } from './inventory-modals.js';
import { initializeFilters } from './filters.js';
import { initializeAlerts } from './alerts.js';
import { showAlert } from './utils.js';

document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM Content Loaded - Initializing Inventory Module');
    
    // Inicializar módulos uno por uno con manejo de errores individual
    try {
        initializeAlerts();
        console.log('✓ Alerts initialized');
    } catch (error) {
        console.error('Error initializing alerts:', error);
    }
    
    try {
        initializeFilters();
        console.log('✓ Filters initialized');
    } catch (error) {
        console.error('Error initializing filters:', error);
    }
    
    try {
        initializeInventoryModals();
        console.log('✓ Modals initialized');
    } catch (error) {
        console.error('Error initializing modals:', error);
    }
    
    try {
        initializeInventoryForm();
        console.log('✓ Forms initialized');
    } catch (error) {
        console.error('Error initializing forms:', error);
    }
    
    try {
        initializeInventoryActions();
        console.log('✓ Actions initialized');
    } catch (error) {
        console.error('Error initializing actions:', error);
    }
    
    console.log('Inventory module initialization completed');
});

// Verificar elementos críticos después de que todo esté cargado
window.addEventListener('load', function() {
    const criticalElements = [
        'newItemModal',
        'editItemModal', 
        'deleteConfirmModal',
        'itemDetailsModal'
    ];
    
    const missingElements = criticalElements.filter(id => !document.getElementById(id));
    
    if (missingElements.length > 0) {
        console.warn('Missing critical modal elements:', missingElements);
        showAlert('warning', 'Algunos elementos de la interfaz no se cargaron correctamente');
    } else {
        console.log('All critical modal elements found');
    }
});