// Manejo centralizado de alertas

import { showAlert } from './utils.js';

export function initializeAlerts() {
    console.log('Alerts system initialized for inventory');
}

export function showSuccessAlert(message) {
    showAlert('success', message);
}

export function showErrorAlert(message) {
    showAlert('danger', message);
}

export function showWarningAlert(message) {
    showAlert('warning', message);
}

export function showInfoAlert(message) {
    showAlert('info', message);
}