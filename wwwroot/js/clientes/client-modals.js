// Inicialización de modales y funciones globales necesarias

export function initializeClientModals() {
    // Configurar limpieza de formularios al abrir modales
    setupModalCleanup();
    
    console.log('Client modals initialized successfully');
}

function setupModalCleanup() {
    // Limpiar formulario al abrir modal de nuevo cliente
    const newClientModal = document.getElementById('newClientModal');
    if (newClientModal) {
        newClientModal.addEventListener('show.bs.modal', function() {
            // Resetear formulario
            const form = document.getElementById('newClientForm');
            if (form) {
                form.reset();
            }
            
            // Resetear tipo de cliente a Individual
            const individualRadio = document.getElementById('individualClient');
            if (individualRadio) {
                individualRadio.checked = true;
            }
            
            // Ocultar campos de empresa
            const companyFields = document.querySelectorAll('.company-field');
            companyFields.forEach(field => field.style.display = 'none');
            
            // Actualizar label
            const nameLabel = document.getElementById('nameLabel');
            if (nameLabel) {
                nameLabel.textContent = 'Nombre completo';
            }
        });
    }
    
    // Limpiar formulario al cerrar modal de edición
    const editClientModal = document.getElementById('editClientModal');
    if (editClientModal) {
        editClientModal.addEventListener('hidden.bs.modal', function() {
            // Resetear formulario
            const form = document.getElementById('editClientForm');
            if (form) {
                form.reset();
            }
            
            // Ocultar campos de empresa
            const editCompanyFields = document.querySelectorAll('.edit-company-field');
            editCompanyFields.forEach(field => field.style.display = 'none');
        });
    }
}