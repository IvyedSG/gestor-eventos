// Manejo de formularios de pago (crear/editar)

import { showAlert, insertAlert, getAntiForgeryToken, setButtonLoading, restoreButton } from './utils.js';

export function initializePaymentForm() {
    setupCreatePaymentForm();
    setupUpdatePaymentForm();
    console.log('Payment forms initialized successfully');
}

function setupCreatePaymentForm() {
    const createPaymentBtn = document.getElementById('createPaymentBtn');
    
    if (createPaymentBtn) {
        createPaymentBtn.addEventListener('click', handleCreatePayment);
    }
}

function setupUpdatePaymentForm() {
    const updatePaymentBtn = document.getElementById('updatePaymentBtn');
    
    if (updatePaymentBtn) {
        updatePaymentBtn.addEventListener('click', handleUpdatePayment);
    }
}

async function handleCreatePayment() {
    const button = document.getElementById('createPaymentBtn');
    const originalHtml = setButtonLoading(button, 'Procesando...');
    
    try {
        const paymentData = collectCreatePaymentData();
        if (!paymentData) return;
        
        const token = getAntiForgeryToken();
        
        const response = await fetch('?handler=CreatePayment', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token,
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify(paymentData)
        });
        
        const responseText = await response.text();
        let result;
        try {
            result = JSON.parse(responseText);
        } catch (e) {
            console.error('Error parsing JSON response:', e, responseText);
            throw new Error('Error en la respuesta del servidor');
        }
        
        if (response.ok && result.success) {
            // Cerrar modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('newPaymentModal'));
            modal.hide();
            
            // Mostrar mensaje de éxito y recargar
            setTimeout(() => {
                window.location.href = '/Pagos?success=true';
            }, 500);
        } else {
            throw new Error(result.message || 'No se pudo crear el pago');
        }
    } catch (error) {
        console.error('Error:', error);
        
        const errorDiv = document.getElementById('paymentError');
        const errorMessage = document.getElementById('paymentErrorMessage');
        
        if (errorDiv && errorMessage) {
            errorDiv.classList.remove('d-none');
            errorMessage.textContent = error.message || 'Error al crear el pago';
        }
    } finally {
        restoreButton(button, originalHtml);
    }
}

async function handleUpdatePayment() {
    const button = document.getElementById('updatePaymentBtn');
    const originalHtml = setButtonLoading(button, 'Procesando...');
    
    try {
        const { paymentId, paymentData } = collectUpdatePaymentData();
        if (!paymentData) return;
        
        const token = getAntiForgeryToken();
        
        const response = await fetch(`?handler=UpdatePayment&id=${encodeURIComponent(paymentId)}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token,
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify(paymentData)
        });
        
        const responseText = await response.text();
        let result;
        try {
            result = JSON.parse(responseText);
        } catch (e) {
            console.error('Error parsing JSON response:', e, responseText);
            throw new Error('Error en la respuesta del servidor');
        }
        
        if (response.ok && result.success) {
            // Cerrar modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('editPaymentModal'));
            modal.hide();
            
            // Mostrar mensaje de éxito y recargar
            setTimeout(() => {
                window.location.href = '/Pagos?success=true';
            }, 500);
        } else {
            throw new Error(result.message || 'No se pudo actualizar el pago');
        }
    } catch (error) {
        console.error('Error:', error);
        
        const errorDiv = document.getElementById('editPaymentError');
        const errorMessage = document.getElementById('editPaymentErrorMessage');
        
        if (errorDiv && errorMessage) {
            errorDiv.classList.remove('d-none');
            errorMessage.textContent = error.message || 'Error al actualizar el pago';
        }
    } finally {
        restoreButton(button, originalHtml);
    }
}

function collectCreatePaymentData() {
    const reservaSelect = document.getElementById('paymentReservation');
    const tipoSelect = document.getElementById('paymentType');
    const montoInput = document.getElementById('paymentAmount');
    const errorDiv = document.getElementById('paymentError');
    const errorMessage = document.getElementById('paymentErrorMessage');
    
    // Ocultar errores previos
    errorDiv.classList.add('d-none');
    
    // Validar formulario
    let isValid = true;
    
    if (!reservaSelect.value) {
        reservaSelect.classList.add('is-invalid');
        isValid = false;
    } else {
        reservaSelect.classList.remove('is-invalid');
    }
    
    if (!tipoSelect.value) {
        tipoSelect.classList.add('is-invalid');
        isValid = false;
    } else {
        tipoSelect.classList.remove('is-invalid');
    }
    
    if (!montoInput.value || parseFloat(montoInput.value) <= 0) {
        montoInput.classList.add('is-invalid');
        isValid = false;
    } else {
        montoInput.classList.remove('is-invalid');
    }
    
    if (!isValid) {
        errorDiv.classList.remove('d-none');
        errorMessage.textContent = 'Por favor, completa todos los campos correctamente.';
        return null;
    }
    
    return {
        idReserva: reservaSelect.value,
        nombreTipoPago: tipoSelect.value,
        monto: montoInput.value
    };
}

function collectUpdatePaymentData() {
    const paymentId = document.getElementById('editPaymentId').value;
    const tipoSelect = document.getElementById('editPaymentType');
    const montoInput = document.getElementById('editPaymentAmount');
    const errorDiv = document.getElementById('editPaymentError');
    const errorMessage = document.getElementById('editPaymentErrorMessage');
    
    // Ocultar errores previos
    errorDiv.classList.add('d-none');
    
    // Validar formulario
    let isValid = true;
    
    if (!tipoSelect.value) {
        tipoSelect.classList.add('is-invalid');
        isValid = false;
    } else {
        tipoSelect.classList.remove('is-invalid');
    }
    
    if (!montoInput.value || parseFloat(montoInput.value) <= 0) {
        montoInput.classList.add('is-invalid');
        isValid = false;
    } else {
        montoInput.classList.remove('is-invalid');
    }
    
    if (!isValid) {
        errorDiv.classList.remove('d-none');
        errorMessage.textContent = 'Por favor, completa todos los campos correctamente.';
        return { paymentId: null, paymentData: null };
    }
    
    return {
        paymentId,
        paymentData: {
            nombreTipoPago: tipoSelect.value,
            monto: montoInput.value
        }
    };
}