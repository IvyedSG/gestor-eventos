export function showAlert(type, message) {
    // Remover alertas anteriores para evitar acumulación
    const existingAlerts = document.querySelectorAll('.alert.position-fixed');
    existingAlerts.forEach(alert => alert.remove());
    
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show position-fixed top-0 start-50 translate-middle-x mt-4`;
    alertDiv.style.zIndex = '9999';
    alertDiv.style.maxWidth = '80%';
    alertDiv.style.minWidth = '300px';
    
    let icon = 'bi-exclamation-triangle';
    switch(type) {
        case 'success':
            icon = 'bi-check-circle';
            break;
        case 'warning':
            icon = 'bi-exclamation-triangle-fill';
            break;
        case 'danger':
            icon = 'bi-x-octagon';
            break;
        case 'info':
            icon = 'bi-info-circle';
            break;
    }
    
    alertDiv.innerHTML = `
        <div class="d-flex align-items-center">
            <i class="bi ${icon} me-2 fs-5"></i>
            <div class="flex-grow-1">${message}</div>
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    
    document.body.appendChild(alertDiv);
    
    const autoRemoveTime = type === 'warning' || type === 'danger' ? 6000 : 4000;
    
    setTimeout(() => {
        if (alertDiv.parentNode) {
            alertDiv.classList.remove('show');
            setTimeout(() => {
                if (alertDiv.parentNode) {
                    alertDiv.remove();
                }
            }, 150);
        }
    }, autoRemoveTime);
}

export function formatDateForApi(dateStr) {
    if (!dateStr) return null;
    
    try {
        const date = new Date(dateStr);
        if (isNaN(date.getTime())) return null;
        
        return date.toISOString().split('T')[0];
    } catch (e) {
        console.error('Error formatting date:', e);
        return null;
    }
}

export function formatCurrency(value) {
    if (value === null || value === undefined) return '-';
    try {
        return `S/${parseFloat(value).toFixed(2)}`;
    } catch (error) {
        console.error('Error formatting currency:', error);
        return '-';
    }
}

export function formatDate(dateStr) {
    if (!dateStr) return '-';
    try {
        const date = new Date(dateStr);
        if (isNaN(date.getTime())) return '-';
        return date.toLocaleString('es-ES', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    } catch (error) {
        console.error('Error formatting date:', error);
        return '-';
    }
}

export function safeSetText(id, value, defaultValue = '-') {
    const element = document.getElementById(id);
    if (element) {
        element.textContent = value !== null && value !== undefined ? value : defaultValue;
    } else {
        console.warn(`Element with id "${id}" not found in DOM`);
    }
}

export function safeSetHtml(id, htmlContent, defaultHtml = '-') {
    const element = document.getElementById(id);
    if (element) {
        element.innerHTML = htmlContent || defaultHtml;
    } else {
        console.warn(`Element with id "${id}" not found in DOM`);
    }
}


if (typeof window !== 'undefined') {
    window.showAlert = showAlert;
}