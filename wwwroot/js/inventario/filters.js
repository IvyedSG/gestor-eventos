// Manejo de filtros de búsqueda y limpieza

export function initializeFilters() {
    setupClearFilters();
    console.log('Filters initialized successfully');
}

function setupClearFilters() {
    const clearFiltersBtn = document.getElementById('clearFilters');
    if (clearFiltersBtn) {
        clearFiltersBtn.addEventListener('click', function() {
            const filterForm = document.getElementById('filterForm');
            if (filterForm) {
                filterForm.reset();
                filterForm.submit();
            }
        });
    }
}