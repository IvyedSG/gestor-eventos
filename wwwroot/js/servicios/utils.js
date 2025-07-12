// Funciones utilitarias reutilizables

export function showAlert(type, message) {
    // Remover alertas anteriores para evitar acumulación
    const existingAlerts = document.querySelectorAll('.alert.alert-dismissible');
    existingAlerts.forEach(alert => {
        if (alert.parentNode) {
            alert.remove();
        }
    });
    
    // Determinar la clase de estilo según el tipo
    let alertClass = 'alert-info';
    let icon = 'bi-info-circle-fill';
    
    if (type === 'success') {
        alertClass = 'alert-success';
        icon = 'bi-check-circle-fill';
    } else if (type === 'error') {
        alertClass = 'alert-danger';
        icon = 'bi-exclamation-triangle-fill';
    } else if (type === 'warning') {
        alertClass = 'alert-warning';
        icon = 'bi-exclamation-circle-fill';
    }
    
    // Crear el elemento de alerta
    const alertHtml = `
        <div class="alert ${alertClass} alert-dismissible fade show" role="alert">
            <i class="bi ${icon} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    
    // Insertar la alerta al principio del contenedor
    const alertContainer = document.createElement('div');
    alertContainer.innerHTML = alertHtml;
    const container = document.querySelector('.container.py-4');
    container.insertBefore(alertContainer.firstElementChild, container.firstElementChild.nextSibling);
    
    // Auto-cerrar después de 5 segundos
    setTimeout(() => {
        const insertedAlert = document.querySelector(`.${alertClass}.alert-dismissible`);
        if (insertedAlert) {
            const bsAlert = new bootstrap.Alert(insertedAlert);
            bsAlert.close();
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

export function validateRequiredFields(fields) {
    let isValid = true;
    
    fields.forEach(field => {
        const element = document.getElementById(field.id);
        if (!element) return;
        
        const value = element.value.trim();
        
        if (!value || (field.type === 'number' && (isNaN(value) || parseFloat(value) <= 0))) {
            element.classList.add('is-invalid');
            isValid = false;
        } else {
            element.classList.remove('is-invalid');
        }
    });
    
    return isValid;
}

export function clearValidationErrors(fieldIds) {
    fieldIds.forEach(fieldId => {
        const element = document.getElementById(fieldId);
        if (element) {
            element.classList.remove('is-invalid');
        }
    });
}

// Hacer showAlert disponible globalmente para compatibilidad
if (typeof window !== 'undefined') {
    window.showAlert = showAlert;
}