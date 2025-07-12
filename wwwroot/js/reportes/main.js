// Archivo principal que inicializa todo el sistema de reportes
import { initializeReportsData } from './reports-data.js';
import { initializeCharts } from './charts.js';
import { initializeFilters } from './filters.js';
import { initializeExport } from './export.js';
import { initializeAlerts } from './alerts.js';
import { showAlert } from './utils.js';

document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM Content Loaded - Initializing Reports Module');
    
    try {
        // Inicializar módulos en orden
        initializeAlerts();
        console.log('✓ Alerts initialized');
        
        initializeFilters();
        console.log('✓ Filters initialized');
        
        initializeCharts();
        console.log('✓ Charts initialized');
        
        initializeExport();
        console.log('✓ Export initialized');
        
        initializeReportsData();
        console.log('✓ Reports data initialized');
        
        console.log('Reports module initialized successfully');
        
        // Cargar reportes iniciales
        setTimeout(() => {
            console.log('Loading initial reports...');
            window.loadAllReports();
        }, 100);
        
    } catch (error) {
        console.error("Error in DOMContentLoaded:", error);
        showAlert('error', 'Error al inicializar el sistema de reportes');
    }
});

// Verificar elementos críticos después de que todo esté cargado
window.addEventListener('load', function() {
    const criticalElements = [
        'revenueChart',
        'reservationStatusChart',
        'popularServicesChart',
        'clientGrowthChart',
        'topItemsChart',
        'eventTypeProfitChart'
    ];
    
    const missingElements = criticalElements.filter(id => !document.getElementById(id));
    
    if (missingElements.length > 0) {
        console.warn('Missing critical chart elements:', missingElements);
    } else {
        console.log('All critical chart elements found');
    }
});