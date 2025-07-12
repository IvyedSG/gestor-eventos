// Manejo de acciones sobre pagos existentes

import { showAlert, insertAlert, getAntiForgeryToken, setButtonLoading, restoreButton } from './utils.js';

export function initializePaymentActions() {
    try {
        setupEditButtons();
        setupDeleteButtons();
        setupDeleteConfirmation();
        console.log('Payment actions initialized successfully');
    } catch (error) {
        console.error('Error initializing payment actions:', error);
    }
}

function setupEditButtons() {
    const editButtons = document.querySelectorAll('.edit-payment');
    console.log(`Found ${editButtons.length} edit buttons`);
    
    editButtons.forEach(button => {
        button.addEventListener('click', function() {
            const paymentId = this.dataset.id;
            const paymentType = this.dataset.type;
            const paymentAmount = this.dataset.amount;
            const reservationName = this.dataset.reservation;
            const paymentDate = this.dataset.date;
            
            loadPaymentForEdit(paymentId, paymentType, paymentAmount, reservationName, paymentDate);
        });
    });
}

function setupDeleteButtons() {
    const deleteButtons = document.querySelectorAll('.delete-payment');
    console.log(`Found ${deleteButtons.length} delete buttons`);
    
    deleteButtons.forEach(button => {
        button.addEventListener('click', function() {
            const paymentId = this.dataset.id;
            const paymentType = this.dataset.type;
            const paymentAmount = this.dataset.amount;
            const paymentDate = this.dataset.date;
            
            loadPaymentForDelete(paymentId, paymentType, paymentAmount, paymentDate);
        });
    });
}

function setupDeleteConfirmation() {
    const confirmDeleteBtn = document.getElementById('confirmDeletePaymentBtn');
    if (confirmDeleteBtn) {
        confirmDeleteBtn.addEventListener('click', handleDeletePayment);
    }
}

function loadPaymentForEdit(paymentId, paymentType, paymentAmount, reservationName, paymentDate) {
    console.log('Editando pago:', {
        id: paymentId,
        type: paymentType,
        amount: paymentAmount,
        reservation: reservationName,
        date: paymentDate
    });
    
    // Rellenar formulario de edición
    document.getElementById('editPaymentId').value = paymentId;
    document.getElementById('editPaymentReservation').value = reservationName || 'Sin reserva';
    document.getElementById('editPaymentAmount').value = paymentAmount;
    document.getElementById('editPaymentDate').value = paymentDate;
    
    // Configurar tipo de pago
    const typeSelect = document.getElementById('editPaymentType');
    typeSelect.value = paymentType.toLowerCase();
    
    // Limpiar errores previos
    const errorDiv = document.getElementById('editPaymentError');
    if (errorDiv) {
        errorDiv.classList.add('d-none');
    }
    
    // Mostrar modal
    const modalElement = document.getElementById('editPaymentModal');
    if (modalElement) {
        const bsModal = new bootstrap.Modal(modalElement);
        bsModal.show();
    }
}

function loadPaymentForDelete(paymentId, paymentType, paymentAmount, paymentDate) {
    console.log('Eliminando pago:', {
        id: paymentId,
        type: paymentType,
        amount: paymentAmount,
        date: paymentDate
    });
    
    // Configurar modal de eliminación
    document.getElementById('deletePaymentId').value = paymentId;
    document.getElementById('deletePaymentType').textContent = paymentType;
    document.getElementById('deletePaymentAmount').textContent = paymentAmount;
    document.getElementById('deletePaymentDate').textContent = paymentDate;
    
    // Limpiar errores previos
    const errorDiv = document.getElementById('deletePaymentError');
    if (errorDiv) {
        errorDiv.classList.add('d-none');
    }
    
    // Mostrar modal
    const modalElement = document.getElementById('deletePaymentModal');
    if (modalElement) {
        const bsModal = new bootstrap.Modal(modalElement);
        bsModal.show();
    }
}

async function handleDeletePayment() {
    const button = document.getElementById('confirmDeletePaymentBtn');
    const originalHtml = setButtonLoading(button, 'Procesando...');
    
    try {
        const paymentId = document.getElementById('deletePaymentId').value;
        const errorDiv = document.getElementById('deletePaymentError');
        const errorMessage = document.getElementById('deletePaymentErrorMessage');
        
        // Ocultar errores previos
        errorDiv.classList.add('d-none');
        
        if (!paymentId) {
            errorDiv.classList.remove('d-none');
            errorMessage.textContent = 'ID de pago no válido.';
            return;
        }
        
        const token = getAntiForgeryToken();
        
        const response = await fetch(`?handler=DeletePayment&id=${encodeURIComponent(paymentId)}`, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token,
                'X-Requested-With': 'XMLHttpRequest'
            }
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
            const modal = bootstrap.Modal.getInstance(document.getElementById('deletePaymentModal'));
            modal.hide();
            
            // Mostrar mensaje de éxito y recargar
            setTimeout(() => {
                window.location.href = '/Pagos?success=true';
            }, 500);
        } else {
            throw new Error(result.message || 'No se pudo eliminar el pago');
        }
    } catch (error) {
        console.error('Error:', error);
        
        const errorDiv = document.getElementById('deletePaymentError');
        const errorMessage = document.getElementById('deletePaymentErrorMessage');
        
        if (errorDiv && errorMessage) {
            errorDiv.classList.remove('d-none');
            errorMessage.textContent = error.message || 'Error al eliminar el pago';
        }
    } finally {
        restoreButton(button, originalHtml);
    }
}