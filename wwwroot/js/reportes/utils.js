// Funciones utilitarias para reportes

export function showAlert(type, message) {
    // Crear elemento de alerta
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type === 'error' ? 'danger' : 'success'} alert-dismissible fade show position-fixed`;
    alertDiv.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    alertDiv.innerHTML = `
        <i class="bi bi-${type === 'error' ? 'exclamation-circle' : 'check-circle'} me-2"></i>
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    document.body.appendChild(alertDiv);

    // Auto-eliminar después de 5 segundos
    setTimeout(() => {
        if (alertDiv && alertDiv.parentNode) {
            alertDiv.remove();
        }
    }, 5000);
}

export function getAntiForgeryToken() {
    const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
    if (!tokenElement) {
        throw new Error('Token de verificación no encontrado');
    }
    return tokenElement.value;
}

export function fetchWithToken(url, options = {}) {
    const token = getAntiForgeryToken();
    const headers = {
        'RequestVerificationToken': token,
        ...options.headers
    };
    return fetch(url, { ...options, headers });
}

export function formatCurrency(value) {
    if (value === null || value === undefined) return 'S/0.00';
    try {
        return `S/${parseFloat(value).toLocaleString('es-PE', {minimumFractionDigits: 2})}`;
    } catch (error) {
        console.error('Error formatting currency:', error);
        return 'S/0.00';
    }
}

export function formatDate(dateStr) {
    if (!dateStr) return '-';
    try {
        const date = new Date(dateStr);
        if (isNaN(date.getTime())) return '-';
        return date.toLocaleDateString('es-ES', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
    } catch (error) {
        console.error('Error formatting date:', error);
        return '-';
    }
}

export function setButtonLoading(button, loadingText = 'Cargando...') {
    if (!button) return null;
    
    const originalHtml = button.innerHTML;
    button.disabled = true;
    button.innerHTML = `<i class="bi bi-hourglass-split me-1"></i>${loadingText}`;
    
    return originalHtml;
}

export function restoreButton(button, originalHtml) {
    if (!button) return;
    
    button.disabled = false;
    button.innerHTML = originalHtml;
}

export function showNoDataMessage(canvasId, message = 'No hay datos disponibles') {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;
    
    const ctx = canvas.getContext('2d');
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    
    // Configurar el texto
    ctx.font = '16px Arial';
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillStyle = '#6B7280';
    
    // Calcular posición central
    const centerX = canvas.width / 2;
    const centerY = canvas.height / 2;
    
    // Dibujar icono simple (círculo con línea)
    ctx.strokeStyle = '#D1D5DB';
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.arc(centerX, centerY - 40, 20, 0, 2 * Math.PI);
    ctx.moveTo(centerX - 10, centerY - 40);
    ctx.lineTo(centerX + 10, centerY - 40);
    ctx.stroke();
    
    // Texto principal
    ctx.fillStyle = '#374151';
    ctx.font = 'bold 16px Arial';
    ctx.fillText('Sin datos para mostrar', centerX, centerY - 10);
    
    // Texto secundario
    ctx.fillStyle = '#6B7280';
    ctx.font = '14px Arial';
    ctx.fillText(message, centerX, centerY + 15);
}

export function hideChartLoading(chartId) {
    console.log(`Ocultando loading para: ${chartId}`);
    const loadingElement = document.getElementById(chartId + 'Loading');
    if (loadingElement) {
        loadingElement.style.display = 'none';
        loadingElement.classList.add('hidden');
        console.log(`Loading ocultado para: ${chartId}`);
        
        // Verificar que el canvas sea visible
        const canvas = document.getElementById(chartId);
        if (canvas) {
            console.log(`Canvas ${chartId} - Visible: ${canvas.offsetWidth > 0 && canvas.offsetHeight > 0}`);
        }
    } else {
        console.warn(`No se encontró elemento loading: ${chartId}Loading`);
    }
}

export function showAllChartLoadings() {
    const loadingIds = [
        'revenueChartLoading',
        'reservationStatusChartLoading',
        'popularServicesChartLoading',
        'clientGrowthChartLoading',
        'topItemsChartLoading',
        'eventTypeProfitChartLoading'
    ];
    
    loadingIds.forEach(id => {
        const element = document.getElementById(id);
        if (element) {
            element.classList.remove('hidden');
            element.style.display = 'flex';
            console.log(`Mostrando loading: ${id}`);
        }
    });
}

// Hacer funciones disponibles globalmente para compatibilidad
if (typeof window !== 'undefined') {
    window.showAlert = showAlert;
    window.formatCurrency = formatCurrency;
    window.formatDate = formatDate;
}