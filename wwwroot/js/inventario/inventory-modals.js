// Inicialización de modales y funciones globales necesarias

export function initializeInventoryModals() {
    setupModalCleanup();
    setupFormValidation();
    console.log('Inventory modals initialized successfully');
}

function setupModalCleanup() {
    // Limpiar formulario al abrir modal de nuevo ítem
    const newItemModal = document.getElementById('newItemModal');
    if (newItemModal) {
        newItemModal.addEventListener('show.bs.modal', function() {
            const form = document.getElementById('newItemForm');
            if (form) {
                form.reset();
                clearFormValidation(form);
            }
        });
    }
    
    // Limpiar formulario al cerrar modal de edición
    const editItemModal = document.getElementById('editItemModal');
    if (editItemModal) {
        editItemModal.addEventListener('hidden.bs.modal', function() {
            const form = document.getElementById('editItemForm');
            if (form) {
                form.reset();
                clearFormValidation(form);
            }
        });
    }
}

function setupFormValidation() {
    // Configurar validación en tiempo real para campos de nuevo ítem
    const newItemFields = ['itemName', 'itemStock', 'itemPrice'];
    newItemFields.forEach(fieldId => {
        const field = document.getElementById(fieldId);
        if (field) {
            field.addEventListener('input', function() {
                this.classList.remove('is-invalid');
                
                // Validación específica por tipo de campo
                if (this.type === 'number' && this.value !== '') {
                    const value = parseFloat(this.value);
                    if (isNaN(value) || value < 0) {
                        this.classList.add('is-invalid');
                    }
                }
            });
        }
    });
    
    // Configurar validación en tiempo real para campos de edición
    const editItemFields = ['editItemName', 'editCurrentStock', 'editItemPrice'];
    editItemFields.forEach(fieldId => {
        const field = document.getElementById(fieldId);
        if (field) {
            field.addEventListener('input', function() {
                this.classList.remove('is-invalid');
                
                // Validación específica por tipo de campo
                if (this.type === 'number' && this.value !== '') {
                    const value = parseFloat(this.value);
                    if (isNaN(value) || value < 0) {
                        this.classList.add('is-invalid');
                    }
                }
            });
        }
    });
}

function clearFormValidation(form) {
    if (!form) return;
    
    // Limpiar clases de validación
    const inputs = form.querySelectorAll('.form-control, .form-select');
    inputs.forEach(input => {
        input.classList.remove('is-invalid', 'is-valid');
    });
}