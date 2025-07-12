// Manejo de filtros de fecha y período

export function initializeFilters() {
    setupDateFilters();
    setupPeriodFilter();
    setupFilterButtons();
    console.log('Filters initialized successfully');
}

function setupDateFilters() {
    // Inicializar fechas por defecto (último año)
    const today = new Date();
    const lastYear = new Date(today.getFullYear() - 1, today.getMonth(), today.getDate());
    
    const dateFromInput = document.getElementById('dateFrom');
    const dateToInput = document.getElementById('dateTo');
    
    if (dateFromInput && dateToInput) {
        dateFromInput.value = lastYear.toISOString().split('T')[0];
        dateToInput.value = today.toISOString().split('T')[0];
        
        console.log('Fechas inicializadas:', {
            desde: dateFromInput.value,
            hasta: dateToInput.value
        });
    }
}

function setupPeriodFilter() {
    const periodSelect = document.getElementById('reportPeriod');
    if (periodSelect) {
        periodSelect.addEventListener('change', function() {
            const days = parseInt(this.value);
            if (days) {
                const endDate = new Date();
                const startDate = new Date();
                startDate.setDate(endDate.getDate() - days);
                
                const dateFromInput = document.getElementById('dateFrom');
                const dateToInput = document.getElementById('dateTo');
                
                if (dateFromInput && dateToInput) {
                    dateFromInput.value = startDate.toISOString().split('T')[0];
                    dateToInput.value = endDate.toISOString().split('T')[0];
                }
            }
        });
    }
}

function setupFilterButtons() {
    const applyFiltersBtn = document.getElementById('applyFilters');
    if (applyFiltersBtn) {
        applyFiltersBtn.addEventListener('click', function() {
            console.log('Aplicando filtros...');
            if (window.loadAllReports) {
                window.loadAllReports();
            }
        });
    }

    const refreshBtn = document.getElementById('refreshReports');
    if (refreshBtn) {
        refreshBtn.addEventListener('click', function() {
            console.log('Refrescando reportes...');
            if (window.loadAllReports) {
                window.loadAllReports();
            }
        });
    }
}

export function getFilterDates() {
    const fechaInicio = document.getElementById('dateFrom')?.value;
    const fechaFin = document.getElementById('dateTo')?.value;
    
    // Formatear fechas para la API (agregar tiempo)
    const fechaInicioFormatted = fechaInicio ? `${fechaInicio}T00:00:00` : '';
    const fechaFinFormatted = fechaFin ? `${fechaFin}T23:59:59` : '';
    
    return {
        fechaInicio: fechaInicioFormatted,
        fechaFin: fechaFinFormatted,
        fechaInicioRaw: fechaInicio,
        fechaFinRaw: fechaFin
    };
}