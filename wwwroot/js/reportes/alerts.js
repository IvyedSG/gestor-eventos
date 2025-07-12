// Manejo centralizado de alertas para reportes

import { showAlert } from './utils.js';

export function initializeAlerts() {
    console.log('Alerts system initialized for reports');
}

export function showSuccessAlert(message) {
    showAlert('success', message);
}

export function showErrorAlert(message) {
    showAlert('error', message);
}

export function showWarningAlert(message) {
    showAlert('warning', message);
}

export function showInfoAlert(message) {
    showAlert('info', message);
}

export function showNotification(message, type = 'info') {
    // Crear elemento de notificación
    const notification = document.createElement('div');
    notification.className = `alert alert-${type === 'error' ? 'danger' : 'success'} alert-dismissible fade show position-fixed`;
    notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    notification.innerHTML = `
        <i class="bi bi-${type === 'error' ? 'exclamation-circle' : 'check-circle'} me-2"></i>
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    document.body.appendChild(notification);

    // Auto-eliminar después de 5 segundos
    setTimeout(() => {
        if (notification && notification.parentNode) {
            notification.remove();
        }
    }, 5000);
}