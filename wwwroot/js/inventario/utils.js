// Funciones utilitarias reutilizables

export function showAlert(type, message) {
    const alertContainer = document.getElementById('alertContainer');
    if (!alertContainer) {
        console.warn('Alert container not found');
        return;
    }
    
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    alertContainer.appendChild(alertDiv);
    
    // Auto-cerrar después de 5 segundos
    setTimeout(() => {
        if (alertDiv.parentNode) {
            const alert = bootstrap.Alert.getOrCreateInstance(alertDiv);
            alert.close();
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

export function validateRequiredFields(fieldIds) {
    let isValid = true;
    
    fieldIds.forEach(fieldId => {
        const element = document.getElementById(fieldId);
        if (!element) return;
        
        const value = element.value.trim();
        
        if (!value || (element.type === 'number' && (isNaN(value) || parseFloat(value) < 0))) {
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

export function formatCurrency(amount) {
    return `S/ ${parseFloat(amount).toFixed(2)}`;
}

export function getItemDataFromRow(row) {
    if (!row) throw new Error('No se pudo encontrar la fila del ítem');
    
    return {
        nombre: row.querySelector('[data-nombre]')?.getAttribute('data-nombre') || '',
        descripcion: row.querySelector('[data-descripcion]')?.getAttribute('data-descripcion') || '',
        stock: parseInt(row.querySelector('[data-stock]')?.getAttribute('data-stock') || '0'),
        stockDisponible: parseInt(row.querySelector('[data-stock-disponible]')?.getAttribute('data-stock-disponible') || '0'),
        itemsEnUso: parseInt(row.querySelector('[data-items-en-uso]')?.getAttribute('data-items-en-uso') || '0'),
        precio: row.querySelector('[data-precio]')?.getAttribute('data-precio') || '0'
    };
}

// Hacer showAlert disponible globalmente para compatibilidad
if (typeof window !== 'undefined') {
    window.showAlert = showAlert;
}