// Manejo de acciones sobre reservas existentes

export function initializeReservationActions() {
    try {
        setupEditButtons();
        setupViewButtons();
        setupDeleteButtons();
        console.log('Reservation actions initialized successfully');
    } catch (error) {
        console.error('Error initializing reservation actions:', error);
    }
}

function setupEditButtons() {
    const editButtons = document.querySelectorAll('.edit-reservation');
    console.log(`Found ${editButtons.length} edit buttons`);
    
    editButtons.forEach(button => {
        const clone = button.cloneNode(true);
        button.parentNode.replaceChild(clone, button);
        
        clone.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            const id = this.getAttribute('data-id');
            console.log('Edit button clicked for reservation:', id);
            
            if (!id) {
                console.error('No reservation ID found in edit button');
                alert('Error: No se encontró el ID de la reserva');
                return;
            }
            
            // Verificar que el modal existe antes de intentar abrirlo
            const modalElement = document.getElementById('editReservationModal');
            if (!modalElement) {
                console.error('Edit modal not found in DOM');
                alert('Error: Modal de edición no encontrado');
                return;
            }
            
            console.log('Modal element found, importing edit modal module...');
            
            // Importar dinámicamente el modal de edición
            import('./edit-reservation-modal.js').then(module => {
                console.log('Edit modal module loaded successfully');
                console.log('Available functions:', Object.keys(module));
                
                if (typeof module.openEditReservationModal === 'function') {
                    console.log('Calling openEditReservationModal with ID:', id);
                    module.openEditReservationModal(id);
                } else {
                    console.error('openEditReservationModal function not found in module');
                    alert('Error: Función de edición no encontrada');
                }
            }).catch(error => {
                console.error('Error loading edit modal module:', error);
                alert('Error al cargar el modal de edición: ' + error.message);
            });
        });
    });
}

function setupViewButtons() {
    const viewButtons = document.querySelectorAll('.view-reservation');
    console.log(`Found ${viewButtons.length} view buttons`);
    
    viewButtons.forEach(button => {
        const clone = button.cloneNode(true);
        button.parentNode.replaceChild(clone, button);
        
        clone.addEventListener('click', function(e) {
            e.preventDefault();
            const id = this.getAttribute('data-id');
            console.log('View button clicked for reservation:', id);
            
            if (!id) {
                console.error('No reservation ID found in view button');
                return;
            }
            
            // Importar dinámicamente el modal de visualización
            import('./view-reservation-modal.js').then(module => {
                module.openReservationDetailsModal(id);
            }).catch(error => {
                console.error('Error loading view modal module:', error);
                alert('Error al cargar el modal de visualización');
            });
        });
    });
}

function setupDeleteButtons() {
    const deleteButtons = document.querySelectorAll('.delete-reservation');
    console.log(`Found ${deleteButtons.length} delete buttons`);
    
    deleteButtons.forEach(button => {
        const clone = button.cloneNode(true);
        button.parentNode.replaceChild(clone, button);
        
        clone.addEventListener('click', function(e) {
            e.preventDefault();
            const id = this.getAttribute('data-id');
            const name = this.getAttribute('data-name');
            const fecha = this.getAttribute('data-fecha');
            
            if (!id) {
                console.error('No reservation ID found in delete button');
                return;
            }
            
            // Importar dinámicamente el modal de eliminación
            import('./delete-reservation-modal.js').then(module => {
                module.openDeleteReservationModal(id, name, fecha);
            }).catch(error => {
                console.error('Error loading delete modal module:', error);
                alert('Error al cargar el modal de eliminación');
            });
        });
    });
}
