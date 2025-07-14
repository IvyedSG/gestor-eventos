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
    if (errorDiv) {
        errorDiv.classList.add('d-none');
    }
    
    // Validar formulario
    let isValid = true;
    
    if (!reservaSelect || !reservaSelect.value) {
        if (reservaSelect) reservaSelect.classList.add('is-invalid');
        isValid = false;
    } else {
        reservaSelect.classList.remove('is-invalid');
    }
    
    if (!tipoSelect || !tipoSelect.value) {
        if (tipoSelect) tipoSelect.classList.add('is-invalid');
        isValid = false;
    } else {
        tipoSelect.classList.remove('is-invalid');
    }
    
    if (!montoInput || !montoInput.value || parseFloat(montoInput.value) <= 0) {
        if (montoInput) montoInput.classList.add('is-invalid');
        isValid = false;
    } else {
        montoInput.classList.remove('is-invalid');
    }
    
    if (!isValid) {
        if (errorDiv && errorMessage) {
            errorDiv.classList.remove('d-none');
            errorMessage.textContent = 'Por favor, completa todos los campos correctamente.';
        }
        return null;
    }
    
    // Extraer el nombre de la reserva del select
    const selectedOption = reservaSelect.options[reservaSelect.selectedIndex];
    let nombreReserva = '';
    
    if (selectedOption) {
        nombreReserva = selectedOption.getAttribute('data-nombre');
        
        // Fallback: extraer del texto si data-nombre no existe
        if (!nombreReserva) {
            const optionText = selectedOption.text;
            const match = optionText.match(/^(.*?)\s*\([^)]+\)$/);
            if (match) {
                nombreReserva = match[1].trim();
            } else {
                nombreReserva = optionText;
            }
        }
    }
    
    // Validar que se extrajo el nombre de la reserva
    if (!nombreReserva || nombreReserva.trim() === '') {
        if (errorDiv && errorMessage) {
            errorDiv.classList.remove('d-none');
            errorMessage.textContent = 'Error al obtener el nombre de la reserva seleccionada.';
        }
        return null;
    }
    
    return {
        idReserva: reservaSelect.value,
        nombreTipoPago: tipoSelect.value,
        nombreReserva: nombreReserva,
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
    if (errorDiv) {
        errorDiv.classList.add('d-none');
    }
    
    // Validar formulario
    let isValid = true;
    
    if (!tipoSelect || !tipoSelect.value) {
        if (tipoSelect) tipoSelect.classList.add('is-invalid');
        isValid = false;
    } else {
        tipoSelect.classList.remove('is-invalid');
    }
    
    if (!montoInput || !montoInput.value || parseFloat(montoInput.value) <= 0) {
        if (montoInput) montoInput.classList.add('is-invalid');
        isValid = false;
    } else {
        montoInput.classList.remove('is-invalid');
    }
    
    if (!isValid) {
        if (errorDiv && errorMessage) {
            errorDiv.classList.remove('d-none');
            errorMessage.textContent = 'Por favor, completa todos los campos correctamente.';
        }
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