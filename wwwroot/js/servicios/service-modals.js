// Inicialización de modales y funciones globales necesarias

export function initializeServiceModals() {
    setupModalCleanup();
    setupTableUpdaters();
    setupItemManagement();
    console.log('Service modals initialized successfully');
}

function setupModalCleanup() {
    // Limpiar modal de nuevo servicio al cerrarlo
    const newServiceModal = document.getElementById('newServiceModal');
    if (newServiceModal) {
        newServiceModal.addEventListener('hidden.bs.modal', function() {
            const form = document.getElementById('newServiceForm');
            if (form) {
                form.reset();
            }
            
            // Resetear variables globales
            window.selectedItems = [];
            window.updateItemsTable();
            
            // Limpiar validaciones
            const fieldsToClean = ['serviceName', 'serviceDescription', 'servicePrice'];
            fieldsToClean.forEach(fieldId => {
                const element = document.getElementById(fieldId);
                if (element) {
                    element.classList.remove('is-invalid');
                }
            });
            
            const itemsTable = document.getElementById('itemsTable');
            const itemsHelp = document.getElementById('itemsHelp');
            
            if (itemsTable) {
                itemsTable.classList.remove('border', 'border-danger');
            }
            if (itemsHelp) {
                itemsHelp.classList.add('d-none');
            }
        });
    }
    
    // Limpiar modal de edición al cerrarlo
    const editServiceModal = document.getElementById('editServiceModal');
    if (editServiceModal) {
        editServiceModal.addEventListener('hidden.bs.modal', function() {
            const form = document.getElementById('editServiceForm');
            if (form) {
                form.reset();
            }
            
            // Limpiar tablas
            const currentItemsBody = document.getElementById('currentItemsTableBody');
            const newItemsBody = document.getElementById('newItemsTableBody');
            const newItemsCountBadge = document.getElementById('newItemsCountBadge');
            
            if (currentItemsBody) currentItemsBody.innerHTML = '';
            if (newItemsBody) newItemsBody.innerHTML = '';
            if (newItemsCountBadge) newItemsCountBadge.textContent = '0 ítems nuevos';
            
            // Resetear variables globales
            window.serviceToEdit = null;
            window.currentItems = [];
            window.itemsToRemove = [];
            window.newItems = [];
        });
    }
}

function setupTableUpdaters() {
    // Función para actualizar la tabla de ítems seleccionados (modal nuevo)
    window.updateItemsTable = function() {
        const itemsTableBody = document.getElementById('itemsTableBody');
        const noItemsRow = document.getElementById('noItemsRow');
        const itemsCountBadge = document.getElementById('itemsCountBadge');
        
        if (!itemsTableBody) return;
        
        if (window.selectedItems.length > 0) {
            itemsTableBody.innerHTML = '';
            if (noItemsRow) noItemsRow.style.display = 'none';
            
            window.selectedItems.forEach(function(item, index) {
                const badgeClass = getBadgeClass(item.estado);
                
                const row = document.createElement('tr');
                row.dataset.itemId = item.id;
                row.innerHTML = `
                    <td>${item.nombre}</td>
                    <td><span class="badge ${badgeClass}">${item.estado}</span></td>
                    <td>${item.stock}</td>
                    <td>
                        <input type="number" class="form-control form-control-sm item-quantity" 
                               min="1" max="${item.stock}" value="${item.cantidad}" style="width: 70px">
                    </td>
                    <td>
                        <div class="btn-group btn-group-sm">
                            <button type="button" class="btn btn-outline-secondary edit-state" data-index="${index}">
                                <i class="bi bi-pencil-square"></i>
                            </button>
                            <button type="button" class="btn btn-outline-danger remove-item" data-index="${index}">
                                <i class="bi bi-trash"></i>
                            </button>
                        </div>
                    </td>
                `;
                
                itemsTableBody.appendChild(row);
                
                // Configurar eventos para esta fila
                setupRowEvents(row, item, index);
            });
            
            if (itemsCountBadge) {
                itemsCountBadge.textContent = `${window.selectedItems.length} ítems`;
            }
        } else {
            itemsTableBody.innerHTML = '';
            if (noItemsRow) noItemsRow.style.display = 'table-row';
            if (itemsCountBadge) itemsCountBadge.textContent = '0 ítems';
        }
    };
    
    // Función para actualizar estado del botón de guardar
    window.updateSaveButtonState = function() {
        const saveButton = document.getElementById('saveServiceBtn');
        const itemsTable = document.getElementById('itemsTable');
        const itemsHelp = document.getElementById('itemsHelp');
        
        if (window.selectedItems.length === 0) {
            if (itemsTable) itemsTable.classList.add('border', 'border-danger');
            if (itemsHelp) itemsHelp.classList.remove('d-none');
            if (saveButton) saveButton.disabled = true;
        } else {
            if (itemsTable) itemsTable.classList.remove('border', 'border-danger');
            if (itemsHelp) itemsHelp.classList.add('d-none');
            if (saveButton) saveButton.disabled = false;
        }
    };
    
    // Función para actualizar tabla de ítems actuales (modal edición)
    window.updateCurrentItemsTable = function() {
        const tbody = document.getElementById('currentItemsTableBody');
        
        if (!tbody) return;
        
        tbody.innerHTML = '';
        
        if (window.currentItems.length === 0) {
            tbody.innerHTML = `
                <tr id="noCurrentItemsRow">
                    <td colspan="4" class="text-center text-muted">
                        <i class="bi bi-info-circle me-2"></i>
                        Este servicio no tiene ítems asignados
                    </td>
                </tr>
            `;
            return;
        }
        
        window.currentItems.forEach((item, index) => {
            const badgeClass = getBadgeClass(item.estado);
            
            const row = document.createElement('tr');
            row.dataset.itemId = item.id;
            row.innerHTML = `
                <td>${item.nombreItem}</td>
                <td><span class="badge ${badgeClass}">${item.estado}</span></td>
                <td>${item.cantidad}</td>
                <td>
                    <button type="button" class="btn btn-sm btn-outline-danger remove-current-item" 
                            data-index="${index}" data-id="${item.id}">
                        <i class="bi bi-trash"></i>
                    </button>
                </td>
            `;
            
            tbody.appendChild(row);
        });
        
        // Configurar eventos para botones de eliminación
        setupCurrentItemsEvents();
    };
    
    // Función para actualizar tabla de nuevos ítems (modal edición)
    window.updateNewItemsTable = function() {
        const tbody = document.getElementById('newItemsTableBody');
        const itemsCountBadge = document.getElementById('newItemsCountBadge');
        
        if (!tbody) return;
        
        tbody.innerHTML = '';
        
        if (window.newItems.length === 0) {
            tbody.innerHTML = `
                <tr id="noNewItemsRow">
                    <td colspan="5" class="text-center text-muted">
                        <i class="bi bi-info-circle me-2"></i>
                        No hay ítems nuevos agregados
                    </td>
                </tr>
            `;
            if (itemsCountBadge) itemsCountBadge.textContent = '0 ítems nuevos';
            return;
        }
        
        window.newItems.forEach((item, index) => {
            const badgeClass = getBadgeClass(item.estado);
            
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${item.nombreItem}</td>
                <td><span class="badge ${badgeClass}">${item.estado}</span></td>
                <td>${item.stockDisponible}</td>
                <td>
                    <input type="number" class="form-control form-control-sm new-item-quantity" 
                           min="1" max="${item.stockDisponible}" value="${item.cantidad}" style="width: 70px"
                           data-index="${index}">
                </td>
                <td>
                    <button type="button" class="btn btn-sm btn-outline-danger remove-new-item" data-index="${index}">
                        <i class="bi bi-trash"></i>
                    </button>
                </td>
            `;
            
            tbody.appendChild(row);
        });
        
        if (itemsCountBadge) {
            itemsCountBadge.textContent = `${window.newItems.length} ítems nuevos`;
        }
        
        // Configurar eventos para nuevos ítems
        setupNewItemsEvents();
    };
    
    // Función para cargar ítems de inventario disponibles
    window.loadAvailableInventoryItems = function() {
        try {
            const itemSelect = document.getElementById('editItemSelect');
            if (!itemSelect) return;
            
            itemSelect.innerHTML = '<option value="" selected>-- Selecciona un ítem --</option>';
            
            // Usar datos del inventario desde el modelo de la página
            const inventoryItems = window.inventarioItems || [];
            
            inventoryItems.forEach(item => {
                const alreadyInService = window.currentItems.some(ci => ci.inventarioId === item.id);
                
                if (item.stockDisponible > 0 && !alreadyInService) {
                    const option = document.createElement('option');
                    option.value = item.id;
                    option.dataset.nombre = item.nombre;
                    option.dataset.precio = item.precioBase;
                    option.dataset.stock = item.stockDisponible;
                    option.textContent = `${item.nombre} (${item.stockDisponible} disponibles)`;
                    
                    itemSelect.appendChild(option);
                }
            });
        } catch (error) {
            console.error('Error loading inventory items:', error);
            showAlert('error', 'No se pudieron cargar los ítems de inventario disponibles');
        }
    };
}

function setupItemManagement() {
    // Configurar eventos globales para el manejo de ítems
    document.addEventListener('click', function(e) {
        if (e.target.classList.contains('remove-item')) {
            const index = parseInt(e.target.dataset.index);
            window.selectedItems.splice(index, 1);
            window.updateItemsTable();
            window.updateSaveButtonState();
            import('./utils.js').then(module => {
                module.showAlert('info', 'Ítem eliminado del servicio');
            });
        }
    });
}

function setupRowEvents(row, item, index) {
    // Configurar cambio de cantidad
    const quantityInput = row.querySelector('.item-quantity');
    if (quantityInput) {
        quantityInput.addEventListener('change', function() {
            const newQuantity = parseInt(this.value);
            if (newQuantity > 0 && newQuantity <= item.stock) {
                window.selectedItems[index].cantidad = newQuantity;
            } else {
                this.value = item.cantidad;
                import('./utils.js').then(module => {
                    module.showAlert('warning', `La cantidad debe estar entre 1 y ${item.stock}`);
                });
            }
        });
    }
    
    // Configurar edición de estado
    const editStateBtn = row.querySelector('.edit-state');
    if (editStateBtn) {
        editStateBtn.addEventListener('click', function() {
            const currentState = window.selectedItems[index].estado;
            const cell = row.querySelector('td:nth-child(2)');
            
            const stateSelect = document.createElement('select');
            stateSelect.className = 'form-select form-select-sm state-select';
            stateSelect.innerHTML = `
                <option value="Nuevo" ${currentState === 'Nuevo' ? 'selected' : ''}>Nuevo</option>
                <option value="Dañado" ${currentState === 'Dañado' ? 'selected' : ''}>Dañado</option>
                <option value="Roto" ${currentState === 'Roto' ? 'selected' : ''}>Roto</option>
            `;
            
            const originalContent = cell.innerHTML;
            cell.innerHTML = '';
            cell.appendChild(stateSelect);
            
            stateSelect.addEventListener('change', function() {
                window.selectedItems[index].estado = this.value;
                window.updateItemsTable();
            });
            
            stateSelect.addEventListener('blur', function() {
                cell.innerHTML = originalContent;
            });
            
            stateSelect.focus();
        });
    }
}

function setupCurrentItemsEvents() {
    const removeButtons = document.querySelectorAll('.remove-current-item');
    removeButtons.forEach(button => {
        button.addEventListener('click', function() {
            const index = parseInt(this.dataset.index);
            const itemId = this.dataset.id;
            
            window.itemsToRemove.push(itemId);
            window.currentItems.splice(index, 1);
            window.updateCurrentItemsTable();
            
            import('./utils.js').then(module => {
                module.showAlert('info', 'Ítem eliminado del servicio');
            });
        });
    });
}

function setupNewItemsEvents() {
    // Configurar eventos para cambios de cantidad
    const quantityInputs = document.querySelectorAll('.new-item-quantity');
    quantityInputs.forEach(input => {
        input.addEventListener('change', function() {
            const index = parseInt(this.dataset.index);
            const newQuantity = parseInt(this.value);
            const maxStock = window.newItems[index].stockDisponible;
            
            if (newQuantity >= 1 && newQuantity <= maxStock) {
                window.newItems[index].cantidad = newQuantity;
            } else {
                this.value = window.newItems[index].cantidad;
                import('./utils.js').then(module => {
                    module.showAlert('warning', `La cantidad debe estar entre 1 y ${maxStock}`);
                });
            }
        });
    });
    
    // Configurar eventos para eliminación
    const removeButtons = document.querySelectorAll('.remove-new-item');
    removeButtons.forEach(button => {
        button.addEventListener('click', function() {
            const index = parseInt(this.dataset.index);
            window.newItems.splice(index, 1);
            window.updateNewItemsTable();
            
            import('./utils.js').then(module => {
                module.showAlert('info', 'Ítem eliminado');
            });
        });
    });
}

function getBadgeClass(estado) {
    switch(estado) {
        case 'Nuevo':
            return 'bg-success';
        case 'Dañado':
            return 'bg-warning text-dark';
        case 'Roto':
            return 'bg-danger';
        default:
            return 'bg-secondary';
    }
}