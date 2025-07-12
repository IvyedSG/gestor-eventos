// Funciones utilitarias reutilizables

export function showAlert(type, message) {
    // Remover alertas anteriores para evitar acumulación
    const existingAlerts = document.querySelectorAll('.alert.position-fixed');
    existingAlerts.forEach(alert => alert.remove());
    
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show position-fixed top-0 start-50 translate-middle-x mt-4`;
    alertDiv.style.zIndex = '9999';
    alertDiv.style.maxWidth = '80%';
    alertDiv.style.minWidth = '300px';
    
    // Iconos según el tipo de alerta
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
    
    // Auto-remover después de 6 segundos para alertas de warning/error
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

export function insertAlert(message, type = 'success') {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
    alertDiv.setAttribute('role', 'alert');
    alertDiv.innerHTML = `
        <i class="bi bi-check-circle-fill me-2"></i>
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    
    const firstCard = document.querySelector('.card.shadow-sm');
    if (firstCard && firstCard.parentNode) {
        firstCard.parentNode.insertBefore(alertDiv, firstCard.nextSibling);
    }
    
    // Auto-remover después de 5 segundos
    setTimeout(() => {
        if (alertDiv.parentNode) {
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

export function setButtonLoading(button, loadingText = 'Procesando...') {
    if (!button) return null;
    
    const originalHtml = button.innerHTML;
    button.disabled = true;
    button.innerHTML = `<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> ${loadingText}`;
    
    return originalHtml;
}

export function restoreButton(button, originalHtml) {
    if (!button) return;
    
    button.disabled = false;
    button.innerHTML = originalHtml;
}

// Hacer showAlert disponible globalmente para compatibilidad
if (typeof window !== 'undefined') {
    window.showAlert = showAlert;
}