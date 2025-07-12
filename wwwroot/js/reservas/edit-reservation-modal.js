// Manejo del modal de edición de reservas

import { showAlert } from './utils.js';

export function initializeEditReservationModal() {
    console.log('Inicializando modal de edición de reservas');
    
    const updateReservationBtn = document.getElementById('updateReservationBtn');
    if (updateReservationBtn) {
        updateReservationBtn.addEventListener('click', handleUpdateReservation);
    }
    
    setupEditServiceSelection();
    setupEditPriceManagement();
    
    // *** NUEVA VALIDACIÓN: Validación en tiempo real para fecha del evento en edición ***
    setupDateValidation();
}

// *** NUEVA FUNCIÓN: Configurar validación de fecha ***
function setupDateValidation() {
    const editEventDate = document.getElementById('editEventDate');
    if (editEventDate) {
        editEventDate.addEventListener('change', function() {
            const selectedDate = new Date(this.value);
            const today = new Date();
            
            // Resetear las horas para comparar solo las fechas
            today.setHours(0, 0, 0, 0);
            selectedDate.setHours(0, 0, 0, 0);
            
            if (selectedDate < today) {
                this.classList.add('is-invalid');
                
                // Crear o actualizar mensaje de error
                let errorDiv = this.parentNode.querySelector('.invalid-feedback');
                if (!errorDiv) {
                    errorDiv = document.createElement('div');
                    errorDiv.className = 'invalid-feedback';
                    this.parentNode.appendChild(errorDiv);
                }
                errorDiv.textContent = 'La fecha del evento no puede ser anterior a la fecha actual';
            } else {
                this.classList.remove('is-invalid');
            }
        });
    }
}

function setupEditServiceSelection() {
    const editServiceSelect = document.getElementById('editServiceSelect');
    if (editServiceSelect) {
        editServiceSelect.addEventListener('change', function() {
            const selectedOption = this.options[this.selectedIndex];
            
            if (selectedOption && !selectedOption.disabled && selectedOption.value) {
                const serviceName = selectedOption.getAttribute('data-nombre');
                const servicePrice = parseFloat(selectedOption.getAttribute('data-precio'));
                
                // Actualizar display del servicio seleccionado
                const selectedServiceDisplay = document.getElementById('editSelectedServiceDisplay');
                const selectedServiceName = document.getElementById('editSelectedServiceName');
                const selectedServicePrice = document.getElementById('editSelectedServicePrice');
                
                if (selectedServiceDisplay && selectedServiceName && selectedServicePrice) {
                    selectedServiceName.textContent = serviceName;
                    selectedServicePrice.textContent = `S/${servicePrice.toFixed(2)}`;
                    selectedServiceDisplay.classList.remove('d-none');
                }
                
                // Actualizar precio total automáticamente
                updateEditTotalPrice(servicePrice);
                
                // Ocultar mensaje de error si estaba visible
                const servicesHelp = document.getElementById('editServicesHelp');
                if (servicesHelp) {
                    servicesHelp.classList.add('d-none');
                }
            }
        });
    }
}

function setupEditPriceManagement() {
    const editTotalPriceInput = document.getElementById('editTotalPrice');
    if (editTotalPriceInput) {
        editTotalPriceInput.addEventListener('input', function() {
            this.classList.add('border-warning');
        });
    }

    const editResetTotalPriceBtn = document.getElementById('editResetTotalPrice');
    if (editResetTotalPriceBtn) {
        editResetTotalPriceBtn.addEventListener('click', function() {
            const editServiceSelect = document.getElementById('editServiceSelect');
            if (editServiceSelect && editServiceSelect.selectedIndex > 0) {
                const selectedOption = editServiceSelect.options[editServiceSelect.selectedIndex];
                const servicePrice = parseFloat(selectedOption.getAttribute('data-precio'));
                updateEditTotalPrice(servicePrice);
            }
        });
    }
}

function updateEditTotalPrice(servicePrice) {
    const editTotalPriceInput = document.getElementById('editTotalPrice');
    if (editTotalPriceInput) {
        editTotalPriceInput.value = servicePrice.toFixed(2);
        editTotalPriceInput.classList.remove('border-warning');
    }
}

// *** FUNCIÓN PRINCIPAL DE ACTUALIZACIÓN ***
async function handleUpdateReservation() {
    const button = document.getElementById('updateReservationBtn');
    const originalHtml = button.innerHTML;
    
    try {
        console.log('Starting reservation update process...');
        
        if (!validateEditForm()) {
            console.log('Validation failed');
            return;
        }
        
        // Mostrar loading
        button.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Actualizando...';
        button.disabled = true;
        
        const reservationData = collectEditFormData();
        const reservationId = document.getElementById('editReservationId').value;
        
        console.log('Actualizando reserva:', reservationId, reservationData);
        
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        const token = tokenElement ? tokenElement.value : '';
        
        const response = await fetch(`/Reservas?handler=UpdateReservation&id=${reservationId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(reservationData)
        });
        
        // DEBUGGING COMPLETO DE LA RESPUESTA
        console.log('=== UPDATE RESPONSE DEBUGGING ===');
        console.log('Response status:', response.status);
        console.log('Response ok:', response.ok);
        
        const responseText = await response.text();
        console.log('Update response text completo:', responseText);
        
        let result = null;
        try {
            if (responseText && responseText.trim()) {
                result = JSON.parse(responseText);
                console.log('Update parsed result:', result);
            } else {
                result = { message: 'Respuesta vacía del servidor' };
            }
        } catch (e) {
            console.error('Error parsing update JSON:', e);
            result = { message: responseText || 'Error al procesar la respuesta' };
        }
        
        
        if (response.ok && result && result.success !== false) {
            showAlert('success', 'Reserva actualizada exitosamente');
            
            // Cerrar modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('editReservationModal'));
            if (modal) {
                modal.hide();
            }
            
            setTimeout(() => {
                window.location.reload();
            }, 1500);
        } else {
            // Buscar información de stock en toda la respuesta
            const fullResponseString = JSON.stringify(result) + ' ' + responseText;
            const errorMessage = parseCompleteErrorMessage(fullResponseString, result?.message);
            throw new Error(errorMessage);
        }
        
    } catch (error) {
        console.error('Error al actualizar la reserva:', error);
        showAlert('danger', error.message || 'Error al actualizar la reserva');
    } finally {
        // Restaurar botón
        button.innerHTML = originalHtml;
        button.disabled = false;
    }
}

// VERSIÓN ULTRA SIMPLE para edición
function parseCompleteErrorMessage(fullResponse, primaryMessage) {
    console.log('=== PARSING COMPLETE UPDATE ERROR ===');
    console.log('Primary message:', primaryMessage);
    console.log('Full response:', fullResponse);
    
    // Convertir a string para análisis
    const messageStr = String(primaryMessage || '').toLowerCase();
    const fullResponseStr = String(fullResponse || '').toLowerCase();
    
    // Buscar errores de fecha
    if (messageStr.includes('fecha anterior') || messageStr.includes('fecha pasada') || 
        messageStr.includes('día actual') || messageStr.includes('date') && messageStr.includes('past')) {
        console.log('✓ Error de fecha detectado en actualización');
        return primaryMessage; // Devolver el mensaje tal como viene del backend
    }
    
    // Buscar en el mensaje principal
    if (messageStr.includes('stock') || messageStr.includes('insuficiente') || messageStr.includes('inventario')) {
        console.log('✓ Mensaje principal contiene información de stock en actualización');
        return primaryMessage; // Devolver el mensaje tal como viene del backend
    }
    
    // Buscar en toda la respuesta
    if (fullResponseStr.includes('stock') || fullResponseStr.includes('insuficiente') || fullResponseStr.includes('inventario')) {
        console.log('✓ Respuesta completa contiene información de stock en actualización');
        
        // Intentar extraer el mensaje de stock de la respuesta completa
        const stockMatch = fullResponseStr.match(/stock insuficiente[^"]*([^"]*)/i);
        if (stockMatch) {
            return stockMatch[0];
        }
        
        return 'No hay stock suficiente para realizar esta actualización';
    }
    
    // Si no hay información de stock, devolver mensaje genérico
    console.log('✗ No se encontraron indicios de error de stock o fecha en actualización');
    return 'Error al actualizar la reserva. Por favor, intente nuevamente.';
}

function collectEditFormData() {
    const editEventName = document.getElementById('editEventName');
    const editEventDate = document.getElementById('editEventDate');
    const editEventDescription = document.getElementById('editEventDescription');
    const editEventType = document.getElementById('editEventType');
    const editEventStatus = document.getElementById('editEventStatus');
    const editTotalPrice = document.getElementById('editTotalPrice');
    const editAdvancePrice = document.getElementById('editAdvancePrice');
    const editServiceSelect = document.getElementById('editServiceSelect');
    
    return {
        nombreEvento: editEventName ? editEventName.value : "",
        fechaEjecucion: editEventDate ? editEventDate.value : "",
        descripcion: editEventDescription ? editEventDescription.value : "",
        tipoEventoNombre: editEventType ? editEventType.value : "",
        estado: editEventStatus ? editEventStatus.value : "Pendiente",
        precioTotal: editTotalPrice ? parseFloat(editTotalPrice.value) : 0,
        precioAdelanto: editAdvancePrice ? parseFloat(editAdvancePrice.value) : 0,
        servicioId: editServiceSelect ? editServiceSelect.value : null
    };
}

function validateEditForm() {
    let isValid = true;
    
    // Validar campos requeridos
    const requiredFields = [
        'editEventName',
        'editEventDate',
        'editEventType',
        'editTotalPrice'
    ];
    
    requiredFields.forEach(fieldId => {
        const field = document.getElementById(fieldId);
        if (field) {
            if (!field.value.trim()) {
                field.classList.add('is-invalid');
                isValid = false;
            } else {
                field.classList.remove('is-invalid');
            }
        }
    });
    
    // *** NUEVA VALIDACIÓN: Verificar que la fecha del evento no sea en el pasado ***
    const editEventDate = document.getElementById('editEventDate');
    if (editEventDate && editEventDate.value) {
        const selectedDate = new Date(editEventDate.value);
        const today = new Date();
        
        // Resetear las horas para comparar solo las fechas
        today.setHours(0, 0, 0, 0);
        selectedDate.setHours(0, 0, 0, 0);
        
        if (selectedDate < today) {
            editEventDate.classList.add('is-invalid');
            showAlert('warning', 'La fecha del evento no puede ser anterior a la fecha actual');
            isValid = false;
        } else {
            editEventDate.classList.remove('is-invalid');
        }
    }
    
    // Validar servicio seleccionado
    const editServiceSelect = document.getElementById('editServiceSelect');
    const servicesHelp = document.getElementById('editServicesHelp');
    
    if (editServiceSelect) {
        if (editServiceSelect.selectedIndex === 0 || !editServiceSelect.value) {
            editServiceSelect.classList.add('is-invalid');
            if (servicesHelp) {
                servicesHelp.classList.remove('d-none');
            }
            isValid = false;
        } else {
            editServiceSelect.classList.remove('is-invalid');
            if (servicesHelp) {
                servicesHelp.classList.add('d-none');
            }
        }
    }
    
    return isValid;
}

// Función para cargar datos en el modal de edición
export function loadReservationForEdit(reservationId) {
    console.log('Cargando reserva para editar:', reservationId);
    
    const editReservationLoading = document.getElementById('editReservationLoading');
    const editReservationError = document.getElementById('editReservationError');
    const editReservationForm = document.getElementById('editReservationForm');
    
    // Mostrar loading
    if (editReservationLoading) editReservationLoading.style.display = 'block';
    if (editReservationError) editReservationError.style.display = 'none';
    if (editReservationForm) editReservationForm.style.display = 'none';
    
    // Realizar petición para obtener datos
    fetch(`/Reservas?handler=ReservationDetails&id=${reservationId}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                populateEditForm(data.data);
                
                // Mostrar formulario
                if (editReservationLoading) editReservationLoading.style.display = 'none';
                if (editReservationForm) editReservationForm.style.display = 'block';
            } else {
                throw new Error(data.message || 'Error al cargar los datos de la reserva');
            }
        })
        .catch(error => {
            console.error('Error al cargar reserva:', error);
            
            // Mostrar error
            if (editReservationLoading) editReservationLoading.style.display = 'none';
            if (editReservationError) {
                editReservationError.style.display = 'block';
                const errorMessage = editReservationError.querySelector('#editReservationErrorMessage');
                if (errorMessage) {
                    errorMessage.textContent = error.message || 'Error al cargar los datos de la reserva';
                }
            }
        });
}

function populateEditForm(reservationData) {
    console.log('Poblando formulario de edición con:', reservationData);
    
    // Llenar campos básicos
    const fieldMappings = {
        'editReservationId': 'id',
        'editEventName': 'nombreEvento',
        'editEventDate': 'fechaEjecucion',
        'editEventDescription': 'descripcion',
        'editEventType': 'tipoEventoNombre',
        'editEventStatus': 'estado',
        'editTotalPrice': 'precioTotal',
        'editAdvancePrice': 'precioAdelanto'
    };
    
    Object.entries(fieldMappings).forEach(([fieldId, dataKey]) => {
        const field = document.getElementById(fieldId);
        if (field && reservationData[dataKey] !== undefined) {
            if (fieldId === 'editEventDate') {
                // Formatear fecha para input type="date"
                const date = new Date(reservationData[dataKey]);
                field.value = date.toISOString().split('T')[0];
            } else {
                field.value = reservationData[dataKey];
            }
        }
    });
    
    // Llenar servicio seleccionado
    const editServiceSelect = document.getElementById('editServiceSelect');
    if (editServiceSelect && reservationData.servicioId) {
        editServiceSelect.value = reservationData.servicioId;
        
        // Trigger change event para actualizar displays
        const changeEvent = new Event('change');
        editServiceSelect.dispatchEvent(changeEvent);
    }
}

// *** FUNCIÓN PARA ABRIR EL MODAL ***
export function openEditReservationModal(reservationId) {
    console.log('Opening edit modal for reservation:', reservationId);
    
    try {
        // Verificar que el modal existe
        const modalElement = document.getElementById('editReservationModal');
        if (!modalElement) {
            console.error('Edit modal element not found!');
            return;
        }
        
        // Verificar que Bootstrap está disponible
        if (typeof bootstrap === 'undefined') {
            console.error('Bootstrap no está disponible');
            return;
        }
        
        // Crear y mostrar el modal
        let modal = bootstrap.Modal.getInstance(modalElement);
        if (!modal) {
            modal = new bootstrap.Modal(modalElement);
        }
        
        // Mostrar el modal primero
        modal.show();
        
        // Cargar los datos de la reserva
        loadReservationForEdit(reservationId);
        
    } catch (error) {
        console.error('Error opening edit modal:', error);
        alert('Error al abrir el modal de edición');
    }
}

// *** EXPORTAR LA FUNCIÓN DE ACTUALIZACIÓN ***
export function updateReservation() {
    console.log('updateReservation function called');
    return handleUpdateReservation();
}

// *** TAMBIÉN EXPORTAR COMO handleUpdateReservation ***
export { handleUpdateReservation };