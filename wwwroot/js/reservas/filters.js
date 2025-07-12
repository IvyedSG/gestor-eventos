// Manejo de filtros de búsqueda y limpieza

export function initializeFilters() {
    const clearFiltersBtn = document.getElementById('clearFilters');
    if (clearFiltersBtn) {
        clearFiltersBtn.addEventListener('click', function() {
            // Redirigir a la página sin filtros
            window.location.href = '/Reservas?CurrentPage=1&PageSize=10';
        });
    }
}