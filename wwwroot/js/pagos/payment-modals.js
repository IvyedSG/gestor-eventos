// Inicialización de modales y funciones globales necesarias

export function initializePaymentModals() {
    // Configurar limpieza de formularios al abrir modales
    setupModalCleanup();
    
    console.log('Payment modals initialized successfully');
}

function setupModalCleanup() {
    // Limpiar formulario al abrir modal de nuevo pago
    const newPaymentModal = document.getElementById('newPaymentModal');
    if (newPaymentModal) {
        newPaymentModal.addEventListener('hidden.bs.modal', function() {
            // Limpiar formulario de crear pago
            const form = document.getElementById('createPaymentForm');
            if (form) {
                form.reset();
                
                // Remover clases de validación
                const inputs = form.querySelectorAll('.form-control, .form-select');
                inputs.forEach(input => {
                    input.classList.remove('is-invalid');
                });
                
                // Ocultar errores
                const errorDiv = document.getElementById('paymentError');
                if (errorDiv) {
                    errorDiv.classList.add('d-none');
                }
            }
        });
    }
    
    // Limpiar formulario al cerrar modal de edición
    const editPaymentModal = document.getElementById('editPaymentModal');
    if (editPaymentModal) {
        editPaymentModal.addEventListener('hidden.bs.modal', function() {
            // Resetear formulario
            const form = document.getElementById('editPaymentForm');
            if (form) {
                form.reset();
            }
            
            // Limpiar clases de validación
            const inputs = form.querySelectorAll('.form-control, .form-select');
            inputs.forEach(input => {
                input.classList.remove('is-invalid', 'is-valid');
            });
            
            // Ocultar errores
            const errorDiv = document.getElementById('editPaymentError');
            if (errorDiv) {
                errorDiv.classList.add('d-none');
            }
        });
    }
}