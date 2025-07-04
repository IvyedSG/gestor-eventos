// wwwroot/js/reservas.js
document.addEventListener('DOMContentLoaded', function() {
    try {
        console.log('DOM Content Loaded');
        
        // Limpiar filtros
        const clearFiltersBtn = document.getElementById('clearFilters');
        if (clearFiltersBtn) {
            clearFiltersBtn.addEventListener('click', function() {
                const searchTermInput = document.querySelector('[name="SearchTerm"]');
                const statusFilterSelect = document.querySelector('[name="StatusFilter"]');
                const dateFilterInput = document.querySelector('[name="DateFilter"]');
                
                if (searchTermInput) searchTermInput.value = '';
                if (statusFilterSelect) statusFilterSelect.value = '';
                if (dateFilterInput) dateFilterInput.value = '';
                
                const filterForm = document.getElementById('filterForm');
                if (filterForm) filterForm.submit();
            });
        }
        
        // GESTIÓN DE NUEVA RESERVA
        setupReservationForm();
        
        // ACCIONES PARA RESERVAS EXISTENTES
        console.log('Setting up reservation actions...');
        setupReservationActions();
        console.log('Setup complete');
    } catch (error) {
        console.error("Error in DOMContentLoaded:", error);
    }
});

// Define selectedServices at the global scope
let selectedServices = [];
let totalAmount = 0;

// Add this variable to track if price was manually edited
let priceManuallyEdited = false;

function setupReservationForm() {
    // Manejo de opciones de cliente (existente o nuevo)
    const existingClientRadio = document.getElementById('existingClient');
    const newClientRadio = document.getElementById('newClient');
    const existingClientForm = document.getElementById('existingClientForm');
    const newClientForm = document.getElementById('newClientForm');
    
    if (existingClientRadio && newClientRadio && existingClientForm && newClientForm) {
        existingClientRadio.addEventListener('change', function() {
            existingClientForm.style.display = 'block';
            newClientForm.style.display = 'none';
        });
        
        newClientRadio.addEventListener('change', function() {
            existingClientForm.style.display = 'none';
            newClientForm.style.display = 'block';
        });
    }
    
    // Mostrar detalles del cliente seleccionado
    const clientSelect = document.getElementById('clientSelect');
    const clientDetails = document.getElementById('clientDetails');
    
    if (clientSelect && clientDetails) {
        clientSelect.addEventListener('change', function() {
            const selectedOption = this.options[this.selectedIndex];
            
            if (selectedOption && !selectedOption.disabled) {
                const clientDetailName = document.getElementById('clientDetailName');
                const clientDetailEmail = document.getElementById('clientDetailEmail');
                const clientDetailPhone = document.getElementById('clientDetailPhone');
                
                if (clientDetailName) clientDetailName.textContent = selectedOption.getAttribute('data-nombre') || '';
                if (clientDetailEmail) clientDetailEmail.textContent = selectedOption.getAttribute('data-email') || '';
                if (clientDetailPhone) clientDetailPhone.textContent = selectedOption.getAttribute('data-telefono') || '';
                
                clientDetails.style.display = 'block';
            } else {
                clientDetails.style.display = 'none';
            }
        });
    }
    
    // Manejo de selección de servicio (solo uno permitido)
    const serviceSelect = document.getElementById('serviceSelect');
    const selectedServiceDisplay = document.getElementById('selectedServiceDisplay');
    const selectedServiceName = document.getElementById('selectedServiceName');
    const selectedServicePrice = document.getElementById('selectedServicePrice');
    
    if (serviceSelect) {
        serviceSelect.addEventListener('change', function() {
            const selectedOption = this.options[this.selectedIndex];
            
            if (selectedOption && !selectedOption.disabled && selectedOption.value) {
                const serviceId = selectedOption.value;
                const serviceName = selectedOption.getAttribute('data-nombre');
                const servicePrice = parseFloat(selectedOption.getAttribute('data-precio'));
                
                // Limpiar servicios previos y agregar solo el nuevo
                selectedServices = [{
                    id: serviceId,
                    name: serviceName,
                    price: servicePrice
                }];
                
                // Mostrar el servicio seleccionado
                if (selectedServiceDisplay && selectedServiceName && selectedServicePrice) {
                    selectedServiceName.textContent = serviceName;
                    selectedServicePrice.textContent = `S/${servicePrice.toFixed(2)}`;
                    selectedServiceDisplay.classList.remove('d-none');
                }
                
                // Actualizar precio total
                updateTotalPrice();
                
                // Ocultar el select después de seleccionar
                this.style.display = 'none';
                
                // Ocultar mensaje de error si estaba visible
                const servicesHelp = document.getElementById('servicesHelp');
                if (servicesHelp) {
                    servicesHelp.classList.add('d-none');
                }
            }
        });
    }
    
    // Add an event listener to detect manual edits to the total price
    const totalPriceInput = document.getElementById('totalPrice');
    if (totalPriceInput) {
        totalPriceInput.addEventListener('input', function() {
            priceManuallyEdited = true;
            
            // Add a visual indicator that the price was manually edited
            this.classList.add('border-warning');
            
            const priceInfoText = document.querySelector('.form-text');
            if (priceInfoText) {
                priceInfoText.innerHTML = `
                    <i class="bi bi-exclamation-triangle-fill text-warning me-1"></i>
                    Precio modificado manualmente (S/${this.value} vs S/${servicesTotalAmount.toFixed(2)} calculado)
                `;
            }
        });
    }

    // Add this to your JavaScript
    const resetTotalPriceBtn = document.getElementById('resetTotalPrice');
    if (resetTotalPriceBtn) {
        resetTotalPriceBtn.addEventListener('click', function() {
            priceManuallyEdited = false;
            
            // Calculate services total
            let servicesTotalAmount = 0;
            selectedServices.forEach(service => {
                servicesTotalAmount += service.price;
            });
            
            // Update the total price input
            if (totalPriceInput) {
                totalPriceInput.value = servicesTotalAmount.toFixed(2);
                totalPriceInput.classList.remove('border-warning');
                
                const priceInfoText = document.querySelector('.form-text');
                if (priceInfoText) {
                    priceInfoText.innerHTML = `
                        <i class="bi bi-info-circle me-1"></i>
                        Este valor se actualiza automáticamente según los servicios seleccionados, pero también puede modificarse manualmente.
                    `;
                }
            }
        });
    }

    // Función para actualizar el precio total basado en el servicio seleccionado
    function updateTotalPrice() {
        let servicesTotalAmount = 0;
        
        // Calcular total de servicios
        if (selectedServices.length > 0) {
            servicesTotalAmount = selectedServices[0].price;
        }
        
        // Update the total price input only if it wasn't manually edited
        const totalPriceInput = document.getElementById('totalPrice');
        if (!priceManuallyEdited && totalPriceInput) {
            totalPriceInput.value = servicesTotalAmount.toFixed(2);
            totalAmount = servicesTotalAmount;
        } else if (totalPriceInput) {
            // If manually edited, keep the user's value but update the internal variable
            totalAmount = parseFloat(totalPriceInput.value) || 0;
        }
        
        // Hide/show error message
        const servicesHelp = document.getElementById('servicesHelp');
        if (servicesHelp) {
            if (selectedServices.length > 0) {
                servicesHelp.classList.add('d-none');
            } else {
                servicesHelp.classList.remove('d-none');
            }
        }
    }

    // Mantener compatibilidad con código existente
    function updateServicesTable() {
        updateTotalPrice();
    }
    
    // Add a reset function for when the modal is opened/closed
    function resetReservationForm() {
        // Reset the manual edit flag when the form is reset
        priceManuallyEdited = false;
        
        // Reset other form values as needed
        selectedServices = [];
        totalAmount = 0;
        
        // Update the UI
        const totalPriceInput = document.getElementById('totalPrice');
        if (totalPriceInput) {
            totalPriceInput.value = "0.00";
        }
        
        // Reset service selection
        const serviceSelect = document.getElementById('serviceSelect');
        const selectedServiceDisplay = document.getElementById('selectedServiceDisplay');
        
        if (serviceSelect) {
            serviceSelect.selectedIndex = 0;
            serviceSelect.style.display = 'block';
        }
        
        if (selectedServiceDisplay) {
            selectedServiceDisplay.classList.add('d-none');
        }
        
        // Hide service error message
        const servicesHelp = document.getElementById('servicesHelp');
        if (servicesHelp) {
            servicesHelp.classList.add('d-none');
        }
    }
    
    // Call resetReservationForm when modal is shown
    document.addEventListener('DOMContentLoaded', function() {
        const newReservationModal = document.getElementById('newReservationModal');
        if (newReservationModal) {
            newReservationModal.addEventListener('show.bs.modal', function() {
                resetReservationForm();
            });
        }
    });

    // Crear Reserva
    const createReservationBtn = document.getElementById('createReservationBtn');
    
    if (createReservationBtn) {
        createReservationBtn.addEventListener('click', async function() {
            try {
                // Validar formulario
                if (!validateReservationForm()) {
                    return;
                }
                
                // Mostrar indicador de carga
                const button = this;
                const originalHtml = button.innerHTML;
                button.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Creando...';
                button.disabled = true;
                
                // Preparar los datos para la API
                const newEventName = document.getElementById('newEventName');
                const newEventDate = document.getElementById('newEventDate');
                const newEventDescription = document.getElementById('newEventDescription');
                const newEventStatus = document.getElementById('newEventStatus');
                const totalPrice = document.getElementById('totalPrice');
                const clientSelect = document.getElementById('clientSelect');
                const newEventType = document.getElementById('newEventType');
                const advancePrice = document.getElementById('advancePrice');
                const existingClient = document.getElementById('existingClient');
                
                // Handle new client fields
                const newClientName = document.getElementById('newClientName');
                const newClientEmail = document.getElementById('newClientEmail');
                const newClientPhone = document.getElementById('newClientPhone');
                
                const reservationData = {
                    nombreEvento: newEventName ? newEventName.value : "",
                    fechaEjecucion: newEventDate ? newEventDate.value : "",
                    descripcion: newEventDescription ? newEventDescription.value : "",
                    estado: newEventStatus ? newEventStatus.value : "Pendiente",
                    precioTotal: totalPrice ? parseFloat(totalPrice.value) : 0,
                    tipoEventoNombre: newEventType ? newEventType.value : "",
                    servicioId: selectedServices.length > 0 ? selectedServices[0].id : null,
                    precioAdelanto: advancePrice ? parseFloat(advancePrice.value) : 0,
                    
                    // Client information based on radio button selection
                    clienteId: (existingClient && existingClient.checked) ? clientSelect.value : null,
                    nombreCliente: (!existingClient || !existingClient.checked) ? 
                        (newClientName ? newClientName.value : "") : null,
                    correoElectronico: (!existingClient || !existingClient.checked) ? 
                        (newClientEmail ? newClientEmail.value : "") : null,
                    telefono: (!existingClient || !existingClient.checked) ? 
                        (newClientPhone ? newClientPhone.value : "") : null
                };
                
                console.log('Enviando datos para crear reserva:', reservationData);
                
                // Make sure to get the anti-forgery token safely
                const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
                const token = tokenElement ? tokenElement.value : '';
                
                // Use your Razor Page handler instead of the API directly
                const response = await fetch('/Reservas', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': token
                    },
                    body: JSON.stringify(reservationData)
                });
                
                console.log('Response status:', response.status);
                let result = null;
                
                try {
                    // Attempt to parse response as JSON
                    const text = await response.text();
                    console.log('Raw response:', text);
                    result = text ? JSON.parse(text) : {};
                } catch (e) {
                    console.error('Error parsing response:', e);
                }
                
                if (response.ok) {
                    // Cerrar modal
                    if (typeof bootstrap !== 'undefined') {
                        const modal = bootstrap.Modal.getInstance(document.getElementById('newReservationModal'));
                        if (modal) modal.hide();
                    }
                    
                    // Mostrar mensaje de éxito
                    showAlert('success', 'Reserva creada exitosamente');
                    
                    // Recargar la página después de un breve retraso
                    setTimeout(() => {
                        window.location.href = '/Reservas?success=true';
                    }, 1500);
                } else {
                    throw new Error(result?.message || `Error ${response.status}: ${response.statusText}`);
                }
            } catch (error) {
                console.error('Error al crear la reserva:', error);
                showAlert('error', error.message || 'Ocurrió un error al crear la reserva');
            } finally {
                // Restaurar botón
                button.innerHTML = originalHtml;
                button.disabled = false;
            }
        });
    }
}

// Función para remover el servicio seleccionado
window.removeSelectedService = function() {
    selectedServices = [];
    
    const selectedServiceDisplay = document.getElementById('selectedServiceDisplay');
    const serviceSelect = document.getElementById('serviceSelect');
    
    if (selectedServiceDisplay) {
        selectedServiceDisplay.classList.add('d-none');
    }
    
    if (serviceSelect) {
        serviceSelect.style.display = 'block';
        serviceSelect.selectedIndex = 0;
    }
    
    updateTotalPrice();
};

function validateReservationForm() {
    let isValid = true;
    
    // Validar cliente
    const existingClient = document.getElementById('existingClient');
    const clientSelect = document.getElementById('clientSelect');
    
    if (existingClient && existingClient.checked && clientSelect) {
        if (clientSelect.selectedIndex === 0) {
            clientSelect.classList.add('is-invalid');
            isValid = false;
        } else {
            clientSelect.classList.remove('is-invalid');
        }
    } else if (document.getElementById('newClient') && document.getElementById('newClient').checked) {
        // Validar nuevo cliente
        const nameInput = document.getElementById('newClientName');
        const emailInput = document.getElementById('newClientEmail');
        const phoneInput = document.getElementById('newClientPhone');
        
        if (nameInput) {
            if (!nameInput.value.trim()) {
                nameInput.classList.add('is-invalid');
                isValid = false;
            } else {
                nameInput.classList.remove('is-invalid');
            }
        }
        
        if (emailInput) {
            if (!emailInput.value.trim() || !emailInput.value.includes('@')) {
                emailInput.classList.add('is-invalid');
                isValid = false;
            } else {
                emailInput.classList.remove('is-invalid');
            }
        }
        
        if (phoneInput) {
            if (!phoneInput.value.trim()) {
                phoneInput.classList.add('is-invalid');
                isValid = false;
            } else {
                phoneInput.classList.remove('is-invalid');
            }
        }
    }
    
    // Validar datos del evento
    const eventNameInput = document.getElementById('newEventName');
    const eventTypeSelect = document.getElementById('newEventType');
    const eventDateInput = document.getElementById('newEventDate');
    
    if (eventNameInput) {
        if (!eventNameInput.value.trim()) {
            eventNameInput.classList.add('is-invalid');
            isValid = false;
        } else {
            eventNameInput.classList.remove('is-invalid');
        }
    }
    
    if (eventTypeSelect) {
        if (eventTypeSelect.selectedIndex === 0) {
            eventTypeSelect.classList.add('is-invalid');
            isValid = false;
        } else {
            eventTypeSelect.classList.remove('is-invalid');
        }
    }
    
    if (eventDateInput) {
        if (!eventDateInput.value) {
            eventDateInput.classList.add('is-invalid');
            isValid = false;
        } else {
            eventDateInput.classList.remove('is-invalid');
        }
    }
    
    // Validar servicios
    if (selectedServices.length === 0) {
        const servicesHelp = document.getElementById('servicesHelp');
        if (servicesHelp) servicesHelp.classList.remove('d-none');
        isValid = false;
    } else {
        const servicesHelp = document.getElementById('servicesHelp');
        if (servicesHelp) servicesHelp.classList.add('d-none');
    }
    
    return isValid;
}

function setupReservationActions() {
    console.log('Setting up reservation actions');
    
    // Eliminar listeners duplicados de los botones de edición
    document.querySelectorAll('.edit-reservation').forEach(button => {
        // Clonar el botón para eliminar todos los listeners anteriores
        const clone = button.cloneNode(true);
        button.parentNode.replaceChild(clone, button);
        
        // Agregar nuevo listener
        clone.addEventListener('click', function(e) {
            e.preventDefault();
            const id = this.getAttribute('data-id');
            console.log('Edit button clicked for reservation:', id);
            openEditReservationModal(id); // Llamada directa a la función
        });
    });
    
    // Lo mismo para los botones de ver detalles
    document.querySelectorAll('.view-reservation').forEach(button => {
        const clone = button.cloneNode(true);
        button.parentNode.replaceChild(clone, button);
        
        clone.addEventListener('click', function(e) {
            e.preventDefault();
            const id = this.getAttribute('data-id');
            console.log('View button clicked for reservation:', id);
            openReservationDetailsModal(id);
        });
    });
    
    // Configurar el botón de actualizar reserva
    const updateReservationBtn = document.getElementById('updateReservationBtn');
    if (updateReservationBtn) {
        // Eliminar listeners anteriores
        const updateClone = updateReservationBtn.cloneNode(true);
        updateReservationBtn.parentNode.replaceChild(updateClone, updateReservationBtn);
        
        // Agregar nuevo listener
        updateClone.addEventListener('click', function(e) {
            e.preventDefault();
            console.log('Update reservation button clicked');
            if (typeof window.updateReservation === 'function') {
                window.updateReservation();
            } else {
                console.error('updateReservation function not found');
            }
        });
    }
    
    // Resto de tu código para otros botones...
}

// Replace the openReservationDetailsModal function with a stub version:

async function openReservationDetailsModal(reservationId) {
    console.log('Opening modal for reservation:', reservationId);
    
    // Verificar si el elemento modal existe
    const modalElement = document.getElementById('viewReservationModal');
    if (!modalElement) {
        console.error('Modal element not found!');
        return;
    }
    
    try {
        // Verificar si bootstrap está disponible y utilizarlo correctamente
        if (typeof bootstrap === 'undefined') {
            console.error('Bootstrap no está disponible. Verifica que el script de Bootstrap esté cargado.');
            return;
        }
        
        // Intentar obtener instancia existente primero
        let modal = bootstrap.Modal.getInstance(modalElement);
        if (!modal) {
            // Si no existe, crear nueva instancia
            modal = new bootstrap.Modal(modalElement);
        }
        
        // Mostrar el modal
        modal.show();
        
        // Resto de la función...
        
        // Show loading state
        const loadingElement = document.getElementById('reservationLoading');
        const detailsElement = document.getElementById('reservationDetails');
        const errorElement = document.getElementById('reservationError');
        
        if (loadingElement) loadingElement.style.display = 'block';
        if (detailsElement) detailsElement.style.display = 'none';
        if (errorElement) errorElement.style.display = 'none';
        
        // Obtenemos el token antifalsificación si existe
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        const token = tokenElement ? tokenElement.value : '';
        
        console.log('Fetching reservation details for ID:', reservationId);
        
        // Fetch reservation details
        const response = await fetch(`/Reservas?handler=ReservationDetails&id=${reservationId}`, {
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': token
            }
        });
        
        const responseText = await response.text();
        console.log('Response received:', responseText.substring(0, 100) + '...');
        
        // Try to parse the response as JSON
        let result;
        try {
            result = JSON.parse(responseText);
        } catch (e) {
            console.error('Failed to parse response as JSON:', e);
            throw new Error('La respuesta del servidor no es un JSON válido');
        }
        
        if (!response.ok) {
            throw new Error(result?.message || `Error ${response.status}: ${response.statusText}`);
        }
        
        if (!result.success) {
            throw new Error(result.message || 'Error al cargar los detalles de la reserva.');
        }
        
        console.log('Reservation data received successfully');
        
        // Populate the modal with reservation data
        fillReservationModal(result.data);
        
        // After successfully getting reservation details and before showing them:
        console.log('Fetching payment history for reservation:', reservationId);
        
        // Fetch payment history
        const paymentsResponse = await fetch(`/Reservas?handler=ReservationPayments&id=${reservationId}`, {
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': token
            }
        });
        
        // Process payment data
        if (paymentsResponse.ok) {
            const paymentsText = await paymentsResponse.text();
            let paymentsResult;
            
            try {
                paymentsResult = JSON.parse(paymentsText);
                if (paymentsResult.success) {
                    // Fill the payments table
                    fillPaymentsTable(paymentsResult.data);
                }
            } catch (error) {
                console.error('Error parsing payments data:', error);
            }
        } else {
            console.warn('Failed to fetch payment history:', paymentsResponse.status);
        }
        
        // Hide loading, show details
        if (loadingElement) loadingElement.style.display = 'none';
        if (detailsElement) detailsElement.style.display = 'block';
        
    } catch (error) {
        console.error('Error loading reservation details:', error);
        
        // Show error message
        const loadingElement = document.getElementById('reservationLoading');
        const errorElement = document.getElementById('reservationError');
        const errorMessageElement = document.getElementById('reservationErrorMessage');
        
        if (loadingElement) loadingElement.style.display = 'none';
        if (errorElement) errorElement.style.display = 'block';
        if (errorMessageElement) errorMessageElement.textContent = error.message || 'Error al cargar los detalles.';
    }
}

// Modificar la función fillReservationModal para almacenar los datos
function fillReservationModal(reservation) {
    console.log('Filling modal with reservation data:', reservation);
    
    try {
        // Almacenar los datos de la reserva para uso en la generación de boleta
        window.currentReservationData = reservation;
        
        // Safely set text content to DOM elements with error handling
        const safeSetText = (id, value, defaultValue = '-') => {
            const element = document.getElementById(id);
            if (element) {
                element.textContent = value !== null && value !== undefined ? value : defaultValue;
            } else {
                console.warn(`Element with id "${id}" not found in DOM`);
            }
        };
        
        const safeSetHtml = (id, htmlContent, defaultHtml = '-') => {
            const element = document.getElementById(id);
            if (element) {
                element.innerHTML = htmlContent || defaultHtml;
            } else {
                console.warn(`Element with id "${id}" not found in DOM`);
            }
        };
        
        // Reservation general information
        safeSetText('reservationId', reservation.id);
        safeSetText('reservationName', reservation.nombreEvento);
        safeSetText('reservationType', reservation.tipoEventoNombre);
        
        // Format and display the status with appropriate styling
        const status = (reservation.estado || 'Pendiente').toUpperCase();
        let statusText = '';
        
        if (status === 'CONFIRMADO' || status === 'CONFIRMED') {
            statusText = '<span class="badge bg-success">Confirmado</span>';
        } else if (status === 'PENDIENTE' || status === 'PENDING') {
            statusText = '<span class="badge bg-warning text-dark">Pendiente</span>';
        } else if (status === 'CANCELADO' || status === 'CANCELED' || status === 'CANCELLED') {
            statusText = '<span class="badge bg-danger">Cancelado</span>';
        } else if (status === 'FINALIZADO' || status === 'COMPLETED' || status === 'FINISHED') {
            statusText = '<span class="badge bg-info">Finalizado</span>';
        } else {
            statusText = `<span class="badge bg-secondary">${reservation.estado || '-'}</span>`;
        }
        
        safeSetHtml('reservationStatus', statusText);
        
        // Format dates using a helper function
        const formatDate = (dateStr) => {
            if (!dateStr) return '-';
            try {
                const date = new Date(dateStr);
                if (isNaN(date.getTime())) return '-'; // Invalid date
                return date.toLocaleString('es-ES', {
                    day: '2-digit',
                    month: '2-digit',
                    year: 'numeric',
                    hour: '2-digit',
                    minute: '2-digit'
                });
            } catch (error) {
                console.error('Error formatting date:', error);
                return '-';
            }
        };
        
        safeSetText('reservationEventDate', formatDate(reservation.fechaEjecucion));
        safeSetText('reservationRegistrationDate', formatDate(reservation.fechaRegistro));
        
        // Client information
        safeSetText('clientName', reservation.nombreCliente);
        safeSetText('clientEmail', reservation.correoCliente);
        safeSetText('clientPhone', reservation.telefonoCliente);
        safeSetText('clientId', reservation.clienteId);
        
        // Service details
        safeSetText('serviceName', reservation.nombreServicio);
        safeSetText('serviceId', reservation.servicioId);
        
        // Payment information
        const formatCurrency = (value) => {
            if (value === null || value === undefined) return '-';
            try {
                return `S/${parseFloat(value).toFixed(2)}`;
            } catch (error) {
                console.error('Error formatting currency:', error);
                return '-';
            }
        };
        
        safeSetText('reservationTotalPrice', formatCurrency(reservation.precioTotal));
        safeSetText('reservationAdvancePrice', formatCurrency(reservation.precioAdelanto));
        safeSetText('reservationTotalPaid', formatCurrency(reservation.totalPagado));
        safeSetText('reservationLastPayment', reservation.ultimoPago ? formatDate(reservation.ultimoPago) : '-');
        
        // Description
        safeSetText('reservationDescription', reservation.descripcion || 'Sin descripción');
        
        // Configure print button
        const printButton = document.getElementById('printReservationBtn');
        if (printButton) {
            printButton.onclick = function() {
                printReservationDetails(reservation);
            };
        }
        
        console.log('Modal filled successfully with reservation data');
    } catch (error) {
        console.error('Error filling modal with data:', error);
    }
}

// Modificar la función fillPaymentsTable para guardar los datos de pagos
function fillPaymentsTable(payments) {
    console.log('Filling payments table with data:', payments);
    
    // Guardar los datos de pagos para la boleta
    window.reservationPayments = payments;
    
    const tableBody = document.getElementById('paymentTableBody');
    const noPaymentsMessage = document.getElementById('noPaymentsMessage');
    const paymentsTable = document.getElementById('paymentsTable');
    const paymentsTotalElement = document.getElementById('paymentsTotal');
    
    if (!tableBody || !noPaymentsMessage || !paymentsTable) {
        console.warn('Payment table elements not found in DOM');
        return;
    }
    
    // Clear previous content
    tableBody.innerHTML = '';
    
    // Handle no payments case
    if (!payments || payments.length === 0) {
        paymentsTable.style.display = 'none';
        noPaymentsMessage.style.display = 'block';
        return;
    }
    
    // We have payments
    paymentsTable.style.display = 'table';
    noPaymentsMessage.style.display = 'none';
    
    // Calculate total
    let totalAmount = 0;
    
    // Add rows for each payment
    payments.forEach(payment => {
        const amount = parseFloat(payment.monto) || 0;
        totalAmount += amount;
        
        // Format date using the new fechaPago field
        let formattedDate = '-';
        if (payment.fechaPago) {
            try {
                const date = new Date(payment.fechaPago);
                formattedDate = date.toLocaleDateString('es-ES', {
                    day: '2-digit',
                    month: '2-digit',
                    year: 'numeric',
                    hour: '2-digit',
                    minute: '2-digit'
                });
            } catch (error) {
                console.warn('Error formatting payment date:', error);
            }
        }
        
        // Create payment row with badge for payment type
        const row = document.createElement('tr');
        
        // Determine badge class based on payment type
        const tipoPago = payment.tipoPagoNombre?.toLowerCase() || '';
        let badgeClass = 'badge-otro';
        let iconClass = 'bi-question-circle';
        
        switch(tipoPago) {
            case 'efectivo':
                badgeClass = 'badge-efectivo';
                iconClass = 'bi-cash';
                break;
            case 'yape':
                badgeClass = 'badge-yape';
                iconClass = 'bi-phone';
                break;
            case 'plin':
                badgeClass = 'badge-plin';
                iconClass = 'bi-phone-fill';
                break;
            case 'transferencia':
                badgeClass = 'badge-transferencia';
                iconClass = 'bi-bank';
                break;
            case 'adelanto':
                badgeClass = 'badge-adelanto';
                iconClass = 'bi-credit-card-2-front';
                break;
            case 'parcial':
                badgeClass = 'badge-parcial';
                iconClass = 'bi-credit-card';
                break;
        }
        
        // Create a nice-looking badge for payment type
        const badge = `<span class="badge rounded-pill ${badgeClass}"><i class="bi ${iconClass} me-1"></i>${payment.tipoPagoNombre || '-'}</span>`;
        
        row.innerHTML = `
            <td>${payment.id}</td>
            <td>${badge}</td>
            <td>S/${amount.toFixed(2)}</td>
            <td>${formattedDate}</td>
        `;
        tableBody.appendChild(row);
    });
    
    // Update total
    if (paymentsTotalElement) {
        paymentsTotalElement.textContent = `S/${totalAmount.toFixed(2)}`;
    }
}

// Añade esta función para el formato de fecha correcto
function formatDateForApi(dateStr) {
    if (!dateStr) return null;
    
    try {
        const date = new Date(dateStr);
        if (isNaN(date.getTime())) return null;
        
        // Formato yyyy-MM-dd como lo espera el API
        return date.toISOString().split('T')[0];
    } catch (e) {
        console.error('Error formatting date:', e);
        return null;
    }
}

// Y usar esta función en updateReservation:
// dentro de updateData:
fechaEjecucion: formatDateForApi(document.getElementById('editEventDate').value),

// Añadir estas funciones para gestionar la edición de reservas

// Mejora de la función para abrir el modal de edición
window.openEditReservationModal = async function(reservationId) {
    console.log('Opening edit modal for reservation:', reservationId);
    
    try {
        // Get the modal element
        const modalElement = document.getElementById('editReservationModal');
        if (!modalElement) {
            console.error('Modal element not found in DOM!');
            alert('Error: No se encontró el modal de edición en la página');
            return;
        }
        
        // Show loading state first
        const loadingElement = document.getElementById('editReservationLoading');
        const formElement = document.getElementById('editReservationForm');
        const errorElement = document.getElementById('editReservationError');
        
        if (loadingElement) loadingElement.style.display = 'block';
        if (formElement) formElement.style.display = 'none';
        if (errorElement) errorElement.style.display = 'none';
        
        // Initialize and show modal
        if (typeof bootstrap !== 'undefined') {
            let modal = bootstrap.Modal.getInstance(modalElement);
            if (!modal) {
                modal = new bootstrap.Modal(modalElement);
            }
            modal.show();
        } else {
            console.error('Bootstrap is not defined. Please check that Bootstrap JS is loaded correctly.');
            alert('Error: Bootstrap no está disponible');
            return;
        }
        
        // Now fetch the data
        console.log('Fetching reservation data...');
        const response = await fetch(`/Reservas?handler=ReservationDetails&id=${reservationId}`, {
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        });
        
        if (!response.ok) {
            throw new Error(`Error ${response.status}: ${response.statusText}`);
        }
        
        const result = await response.json();
        
        // Cargar servicios disponibles para la selección
        await loadAvailableServicesForEdit();
        
        // Fill form with reservation data if successful
        if (result && result.success) {
            console.log('Filling form with data:', result.data);
            fillEditReservationForm(result.data);
            
            // Hide loading, show form
            if (loadingElement) loadingElement.style.display = 'none';
            if (formElement) formElement.style.display = 'block';
        } else {
            throw new Error(result?.message || 'Error desconocido al cargar los datos');
        }
    } catch (error) {
        console.error('Error opening edit modal:', error);
        
        // Show error message
        const loadingElement = document.getElementById('editReservationLoading');
        const errorElement = document.getElementById('editReservationError');
        const errorMessageElement = document.getElementById('editReservationErrorMessage');
        
        if (loadingElement) loadingElement.style.display = 'none';
        if (errorElement) errorElement.style.display = 'block';
        if (errorMessageElement) errorMessageElement.textContent = error.message || 'Error al cargar los datos de la reserva.';
    }
};

// Variable separada para los servicios en el modal de edición
let selectedServicesEdit = [];

// Función simplificada para cargar servicios disponibles (solo uno permitido)
async function loadAvailableServicesForEdit() {
    try {
        console.log('Configurando servicios disponibles para edición');
        const serviceSelect = document.getElementById('editServiceSelect');
        const editSelectedServiceDisplay = document.getElementById('editSelectedServiceDisplay');
        const editSelectedServiceName = document.getElementById('editSelectedServiceName');
        const editSelectedServicePrice = document.getElementById('editSelectedServicePrice');
        
        if (!serviceSelect) return;
        
        // Configurar el evento change para selección única
        serviceSelect.onchange = function() {
            if (this.value) {
                const selectedOption = this.options[this.selectedIndex];
                const serviceId = this.value;
                const serviceName = selectedOption.getAttribute('data-nombre');
                const servicePrice = parseFloat(selectedOption.getAttribute('data-precio'));
                
                // Reemplazar cualquier servicio previo con el nuevo (solo uno permitido)
                selectedServicesEdit = [{
                    id: serviceId,
                    nombre: serviceName,
                    precio: servicePrice
                }];
                
                // Mostrar el servicio seleccionado
                if (editSelectedServiceDisplay && editSelectedServiceName && editSelectedServicePrice) {
                    editSelectedServiceName.textContent = serviceName;
                    editSelectedServicePrice.textContent = `S/${servicePrice.toFixed(2)}`;
                    editSelectedServiceDisplay.classList.remove('d-none');
                }
                
                // Actualizar precio total si no fue editado manualmente
                const totalPriceInput = document.getElementById('editTotalPrice');
                if (totalPriceInput && !totalPriceInput.hasAttribute('data-manually-edited')) {
                    totalPriceInput.value = servicePrice.toFixed(2);
                }
                
                // Ocultar mensaje de error si estaba visible
                const editServicesHelp = document.getElementById('editServicesHelp');
                if (editServicesHelp) {
                    editServicesHelp.classList.add('d-none');
                }
            }
        };
        
        console.log('Servicios configurados correctamente para edición');
    } catch (error) {
        console.error('Error configuring available services:', error);
    }
}

// Función simplificada para actualizar servicios en el modal de edición (solo uno permitido)
function updateEditServicesTable() {
    // Esta función ya no es necesaria ya que manejamos la UI directamente
    // en loadAvailableServicesForEdit, pero mantenemos por compatibilidad
    console.log('Edit services updated');
}

// Variable para controlar si el precio fue editado manualmente en el modal de edición
let priceManuallyEditedEdit = false;

// Función para llenar el formulario de edición con los datos de la reserva
function fillEditReservationForm(reservation) {
    try {
        console.log('Filling edit form with data:', reservation);
        
        // Reset manual price edit flag
        priceManuallyEditedEdit = false;
        
        // Set hidden ID field
        document.getElementById('editReservationId').value = reservation.id || '';
        
        // Set basic fields
        document.getElementById('editEventName').value = reservation.nombreEvento || '';
        
        // Format date properly for datetime-local input
        if (reservation.fechaEjecucion) {
            const date = new Date(reservation.fechaEjecucion);
            if (!isNaN(date.getTime())) {
                const formattedDate = date.toISOString().slice(0, 16);
                document.getElementById('editEventDate').value = formattedDate;
            }
        }
        
        // CORRECCIÓN PARA EL TIPO DE EVENTO
        console.log("Tipo de evento:", reservation.tipoEventoNombre);
        const typeSelect = document.getElementById('editEventType');
        if (typeSelect && reservation.tipoEventoNombre) {
            // Reiniciar selección
            typeSelect.selectedIndex = 0;
            
            // Iterar todas las opciones para encontrar coincidencia
            let found = false;
            for (let i = 0; i < typeSelect.options.length; i++) {
                if (typeSelect.options[i].value.toLowerCase() === reservation.tipoEventoNombre.toLowerCase()) {
                    typeSelect.selectedIndex = i;
                    found = true;
                    console.log('Tipo de evento encontrado:', typeSelect.options[i].value);
                    break;
                }
            }
            
            // Si no se encuentra, usar la primera opción no deshabilitada
            if (!found) {
                console.warn('Tipo de evento no encontrado:', reservation.tipoEventoNombre);
                // Seleccionar "Otro" como alternativa si existe
                for (let i = 0; i < typeSelect.options.length; i++) {
                    if (typeSelect.options[i].value === "Otro") {
                        typeSelect.selectedIndex = i;
                        break;
                    }
                    // Si no hay "Otro", seleccionar la primera opción válida
                    if (!typeSelect.options[i].disabled && i > 0) {
                        typeSelect.selectedIndex = i;
                        break;
                    }
                }
            }
        }
        
        // Estado
        if (reservation.estado) {
            const statusSelect = document.getElementById('editEventStatus');
            for (let i = 0; i < statusSelect.options.length; i++) {
                if (statusSelect.options[i].value.toLowerCase() === reservation.estado.toLowerCase()) {
                    statusSelect.selectedIndex = i;
                    break;
                }
            }
        }
        
        // Set text fields
        document.getElementById('editEventDescription').value = reservation.descripcion || '';
        
        // Set price fields
        document.getElementById('editTotalPrice').value = reservation.precioTotal || 0;
        document.getElementById('editAdvancePrice').value = reservation.precioAdelanto || 0;
        
        // Cargar servicios de la reserva (solo uno permitido)
        selectedServicesEdit = [];
        
        // La API devuelve solo un servicio principal, no un array de servicios
        if (reservation.servicioId && reservation.nombreServicio) {
            console.log('Agregando servicio principal:', reservation.nombreServicio);
            
            const serviceData = {
                id: reservation.servicioId,
                nombre: reservation.nombreServicio,
                precio: parseFloat(reservation.precioTotal) || 0
            };
            
            selectedServicesEdit.push(serviceData);
            
            // Actualizar UI para mostrar el servicio seleccionado
            const serviceSelect = document.getElementById('editServiceSelect');
            const editSelectedServiceDisplay = document.getElementById('editSelectedServiceDisplay');
            const editSelectedServiceName = document.getElementById('editSelectedServiceName');
            const editSelectedServicePrice = document.getElementById('editSelectedServicePrice');
            
            if (serviceSelect) {
                // Seleccionar el servicio en el dropdown
                for (let i = 0; i < serviceSelect.options.length; i++) {
                    if (serviceSelect.options[i].value === reservation.servicioId) {
                        serviceSelect.selectedIndex = i;
                        break;
                    }
                }
            }
            
            if (editSelectedServiceDisplay && editSelectedServiceName && editSelectedServicePrice) {
                editSelectedServiceName.textContent = serviceData.nombre;
                editSelectedServicePrice.textContent = `S/${serviceData.precio.toFixed(2)}`;
                editSelectedServiceDisplay.classList.remove('d-none');
            }
        }
        
        // No necesitamos llamar updateEditServicesTable ya que manejamos la UI directamente
        
        // Configurar evento para el total price para detectar ediciones manuales
        const editTotalPrice = document.getElementById('editTotalPrice');
        if (editTotalPrice) {
            // Clonar para eliminar listeners previos
            const newInput = editTotalPrice.cloneNode(true);
            editTotalPrice.parentNode.replaceChild(newInput, editTotalPrice);
            
            newInput.addEventListener('input', function() {
                priceManuallyEditedEdit = true;
            });
        }
        
        // Configurar el botón de resetear precio
        const resetTotalPriceBtn = document.getElementById('editResetTotalPrice');
        if (resetTotalPriceBtn) {
            // Clonar para eliminar listeners previos
            const newBtn = resetTotalPriceBtn.cloneNode(true);
            resetTotalPriceBtn.parentNode.replaceChild(newBtn, resetTotalPriceBtn);
            
            newBtn.addEventListener('click', function() {
                priceManuallyEditedEdit = false;
                updateEditServicesTable();
            });
        }
        
        console.log('Form filled successfully with selected services:', selectedServicesEdit);
    } catch (error) {
        console.error('Error filling edit form:', error);
        throw new Error('Error al llenar el formulario con los datos de la reserva');
    }
}

// Actualizar la función updateReservation para incluir servicios
window.updateReservation = async function() {
    try {
        console.log('Updating reservation...');
        const reservationId = document.getElementById('editReservationId').value;
        
        if (!reservationId) {
            console.error('No reservation ID found');
            showAlert('danger', 'Error: No se encontró el ID de la reserva');
            return;
        }
        
        // Get the update button and show loading state
        const button = document.getElementById('updateReservationBtn');
        const originalHtml = button.innerHTML;
        
        // Validar que haya un servicio seleccionado
        if (!selectedServicesEdit || selectedServicesEdit.length === 0) {
            const editServicesHelp = document.getElementById('editServicesHelp');
            if (editServicesHelp) {
                editServicesHelp.classList.remove('d-none');
            }
            showAlert('warning', 'Debe seleccionar un servicio para la reserva');
            return;
        } else {
            const editServicesHelp = document.getElementById('editServicesHelp');
            if (editServicesHelp) {
                editServicesHelp.classList.add('d-none');
            }
        }
        
        button.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Guardando...';
        button.disabled = true;
        
        // Prepare data format exactly as API expects
        const updateData = {
            nombreEvento: document.getElementById('editEventName').value || '',
            fechaEjecucion: document.getElementById('editEventDate').value ? 
                            new Date(document.getElementById('editEventDate').value).toISOString().split('T')[0] : 
                            new Date().toISOString().split('T')[0],
            descripcion: document.getElementById('editEventDescription').value || '',
            estado: document.getElementById('editEventStatus').value || 'Pendiente',
            precioTotal: parseFloat(document.getElementById('editTotalPrice').value || 0),
            servicioId: selectedServicesEdit && selectedServicesEdit.length > 0 ? selectedServicesEdit[0].id : null,
            precioAdelanto: parseFloat(document.getElementById('editAdvancePrice').value || 0),
            tipoEventoNombre: document.getElementById('editEventType').value || 'Otro'
        };
        
        console.log('Sending update data:', updateData);
        
        // Obtener token CSRF para la solicitud
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        const token = tokenElement ? tokenElement.value : '';
        
        console.log('CSRF token found:', !!token);
        console.log('Request URL will be:', `/Reservas?handler=UpdateReservation&id=${reservationId}`);
        
        // Preparar headers
        const headers = {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        };
        
        // Solo agregar token si existe
        if (token) {
            headers['RequestVerificationToken'] = token;
        }
        
        // Usar el handler de Razor Pages con el ID en el query string
        const response = await fetch(`/Reservas?handler=UpdateReservation&id=${reservationId}`, {
            method: 'POST',
            headers: headers,
            body: JSON.stringify(updateData)
        });
        
        const responseText = await response.text();
        console.log('Server response:', responseText);
        console.log('Response status:', response.status);
        
        let result;
        try {
            result = JSON.parse(responseText);
        } catch (e) {
            console.error('Error parsing JSON response:', e);
            // Si no es JSON, verificar si el status es 200 (éxito)
            if (response.ok) {
                result = { success: true, message: 'Reserva actualizada exitosamente' };
            } else {
                throw new Error(`Error en la respuesta del servidor: ${responseText.substring(0, 100)}...`);
            }
        }
        
        if (!response.ok) {
            throw new Error(`Error ${response.status}: ${result?.message || response.statusText}`);
        }
        
        if (response.ok) {
            // Show success message and close modal
            console.log('Reservation updated successfully');
            
            // Show success toast/alert using Bootstrap
            showAlert('success', 'Reserva actualizada exitosamente');
            
            // Close modal properly
            const modalElement = document.getElementById('editReservationModal');
            const modal = bootstrap.Modal.getInstance(modalElement);
            if (modal) {
                modal.hide();
            }
            
            // Clean up modal state and reload page
            setTimeout(() => {
                const backdrops = document.querySelectorAll('.modal-backdrop');
                backdrops.forEach(backdrop => backdrop.remove());
                document.body.classList.remove('modal-open');
                document.body.style.overflow = '';
                document.body.style.paddingRight = '';
                
                // Recargar página para mostrar cambios
                window.location.reload();
            }, 500);
        } else {
            throw new Error(result?.message || 'Error desconocido al actualizar la reserva');
        }
    } catch (error) {
        console.error('Error updating reservation:', error);
        showAlert('danger', 'Error al actualizar la reserva: ' + (error.message || 'Error desconocido'));
    } finally {
        // Restore button state
        const button = document.getElementById('updateReservationBtn');
        if (button) {
            button.innerHTML = '<i class="bi bi-save me-2"></i>Guardar Cambios';
            button.disabled = false;
        }
    }
};

// Función para abrir el modal de confirmación de eliminación
window.openDeleteReservationModal = function(id, name, fecha) {
    console.log('Opening delete confirmation for reservation:', id);
    
    try {
        // Rellenar modal con los datos
        const idField = document.getElementById('deleteReservationId');
        const detailsField = document.getElementById('deleteReservationDetails');
        
        if (idField) idField.value = id;
        if (detailsField) detailsField.textContent = `${name} (${id}) - ${fecha}`;
        
        // Mostrar el modal usando setTimeout para evitar retornar cualquier valor
        setTimeout(() => {
            const modalElement = document.getElementById('deleteReservationModal');
            if (typeof bootstrap !== 'undefined' && modalElement) {
                // Limpiar cualquier modal anterior
                const oldBackdrop = document.querySelector('.modal-backdrop');
                if (oldBackdrop) {
                    oldBackdrop.remove();
                }
                
                // Restablecer clases del body
                document.body.classList.remove('modal-open');
                document.body.style.paddingRight = '';
                
                // Crear y mostrar el modal
                const modal = new bootstrap.Modal(modalElement);
                modal.show();
            }
        }, 0);
    } catch (error) {
        console.error('Error opening delete modal:', error);
    }
    
    // No devolver ningún valor
    // Esto es crucial: las funciones en JavaScript devuelven undefined si no
    // se especifica un valor de retorno, pero algunas bibliotecas pueden convertir
    // undefined a null, lo que podría ser nuestro problema
};

// Modificar la función existente

window.deleteReservation = async function() {
    try {
        const id = document.getElementById('deleteReservationId').value;
        
        if (!id) {
            console.error('No reservation ID found');
            return;
        }
        
        // Mostrar loading state en el botón
        const button = document.getElementById('confirmDeleteBtn');
        const originalHtml = button.innerHTML;
        button.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Eliminando...';
        button.disabled = true;
        
        // Obtener token CSRF
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        const token = tokenElement ? tokenElement.value : '';
        
        // Enviar solicitud
        const response = await fetch('/Reservas?handler=DeleteReservation', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({ id: id })
        });
        
        console.log('Response status:', response.status);
        
        // Leer el texto de la respuesta primero
        const responseText = await response.text();
        console.log('Response text:', responseText);
        
        // Solo intentar parsear como JSON si hay contenido
        let result = null;
        if (responseText && responseText.trim()) {
            try {
                result = JSON.parse(responseText);
                console.log('Parsed result:', result);
            } catch (e) {
                console.warn('Response is not valid JSON:', e);
                // No lanzar error aquí, seguir con el flujo
            }
        }
        
        // Consideramos exitoso si:
        // 1. La respuesta tiene código 200-299 O
        // 2. Tenemos un resultado con success=true
        const isSuccess = response.ok || (result && result.success === true);
        
        if (isSuccess) {
            console.log('Delete operation successful');
            
            // Cerrar modal
            const modalElement = document.getElementById('deleteReservationModal');
            const modal = bootstrap.Modal.getInstance(modalElement);
            if (modal) {
                modal.hide();
                
                // Limpiar backdrop
                setTimeout(() => {
                    const backdrop = document.querySelector('.modal-backdrop');
                    if (backdrop) backdrop.remove();
                    document.body.classList.remove('modal-open');
                    document.body.style.paddingRight = '';
                }, 300);
            }
            
            // Mostrar mensaje de éxito
            showAlert('success', 'Reserva eliminada exitosamente');
            
            // Recargar página después de un breve delay
            setTimeout(() => {
                window.location.href = '/Reservas?success=true&action=delete';
            }, 1500);
            
            return; // Importante: salir de la función aquí si todo fue exitoso
        }
        
        // Si llegamos aquí, hubo un problema
        const errorMessage = (result && result.message) 
            ? result.message 
            : `Error ${response.status}: ${response.statusText || 'Error desconocido'}`;
            
        throw new Error(errorMessage);
        
    } catch (error) {
        console.error('Error deleting reservation:', error);
        
        // Mostrar mensaje de error de forma más suave (sin alert)
        showAlert('danger', 'Error al eliminar la reserva: ' + (error.message || 'Error desconocido'));
    } finally {
        // Restaurar estado del botón
        const button = document.getElementById('confirmDeleteBtn');
        if (button) {
            button.innerHTML = '<i class="bi bi-trash me-2"></i>Eliminar';
            button.disabled = false;
        }
    }
};

// Si no está ya definida, añade la función showAlert
if (typeof window.showAlert !== 'function') {
    window.showAlert = function(type, message) {
        // Crear elemento de alerta
        const alertDiv = document.createElement('div');
        alertDiv.className = `alert alert-${type} alert-dismissible fade show position-fixed top-0 start-50 translate-middle-x mt-4`;
        alertDiv.style.zIndex = '9999';
        alertDiv.style.maxWidth = '80%';
        
        alertDiv.innerHTML = `
            <i class="bi bi-${type === 'success' ? 'check-circle' : 'exclamation-triangle'} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        `;
        
        // Añadir al DOM
        document.body.appendChild(alertDiv);
        
        // Auto-eliminar después de 5 segundos
        setTimeout(() => {
            alertDiv.classList.remove('show');
            setTimeout(() => alertDiv.remove(), 150);
        }, 5000);
    };
}

// Añadir esta función para manejar correctamente el cierre del modal

window.setupDeleteModal = function() {
    // Obtener referencias a los elementos
    const deleteModal = document.getElementById('deleteReservationModal');
    const cancelButton = deleteModal.querySelector('button.btn-secondary');
    const closeButton = deleteModal.querySelector('button.btn-close');
    
    // Función para asegurar que el backdrop se elimine correctamente
    const cleanupModal = () => {
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) {
            backdrop.remove();
        }
        document.body.classList.remove('modal-open');
        document.body.style.paddingRight = '';
    };
    
    // Manejar clic en el botón Cancelar
    if (cancelButton) {
        cancelButton.addEventListener('click', function(e) {
            // Importante: prevenir comportamiento por defecto
            e.preventDefault();
            
            const modal = bootstrap.Modal.getInstance(deleteModal);
            if (modal) {
                modal.hide();
                // Dar tiempo para que el modal termine la animación y luego limpiar
                setTimeout(cleanupModal, 300);
            }
            
            // Importante: asegurarse de no devolver nada
            return false;
        });
    }
    
    // También manejar el botón de cierre (X) con la misma lógica
    if (closeButton) {
        closeButton.addEventListener('click', function(e) {
            e.preventDefault();
            const modal = bootstrap.Modal.getInstance(deleteModal);
            if (modal) {
                modal.hide();
                setTimeout(cleanupModal, 300);
            }
            return false;
        });
    }
    
    // Manejar evento hidden.bs.modal
    deleteModal.addEventListener('hidden.bs.modal', function(e) {
        cleanupModal();
    });
};

// Update this function to handle payments data with fechaPago
function fillPaymentsTable(payments) {
    console.log('Filling payments table with data:', payments);
    
    // Guardar los datos de pagos para la boleta
    window.reservationPayments = payments;
    
    const tableBody = document.getElementById('paymentTableBody');
    const noPaymentsMessage = document.getElementById('noPaymentsMessage');
    const paymentsTable = document.getElementById('paymentsTable');
    const paymentsTotalElement = document.getElementById('paymentsTotal');
    
    if (!tableBody || !noPaymentsMessage || !paymentsTable) {
        console.warn('Payment table elements not found in DOM');
        return;
    }
    
    // Clear previous content
    tableBody.innerHTML = '';
    
    // Handle no payments case
    if (!payments || payments.length === 0) {
        paymentsTable.style.display = 'none';
        noPaymentsMessage.style.display = 'block';
        return;
    }
    
    // We have payments
    paymentsTable.style.display = 'table';
    noPaymentsMessage.style.display = 'none';
    
    // Calculate total
    let totalAmount = 0;
    
    // Add rows for each payment
    payments.forEach(payment => {
        const amount = parseFloat(payment.monto) || 0;
        totalAmount += amount;
        
        // Format date using the new fechaPago field
        let formattedDate = '-';
        if (payment.fechaPago) {
            try {
                const date = new Date(payment.fechaPago);
                formattedDate = date.toLocaleDateString('es-ES', {
                    day: '2-digit',
                    month: '2-digit',
                    year: 'numeric',
                    hour: '2-digit',
                    minute: '2-digit'
                });
            } catch (error) {
                console.warn('Error formatting payment date:', error);
            }
        }
        
        // Create payment row with badge for payment type
        const row = document.createElement('tr');
        
        // Determine badge class based on payment type
        const tipoPago = payment.tipoPagoNombre?.toLowerCase() || '';
        let badgeClass = 'badge-otro';
        let iconClass = 'bi-question-circle';
        
        switch(tipoPago) {
            case 'efectivo':
                badgeClass = 'badge-efectivo';
                iconClass = 'bi-cash';
                break;
            case 'yape':
                badgeClass = 'badge-yape';
                iconClass = 'bi-phone';
                break;
            case 'plin':
                badgeClass = 'badge-plin';
                iconClass = 'bi-phone-fill';
                break;
            case 'transferencia':
                badgeClass = 'badge-transferencia';
                iconClass = 'bi-bank';
                break;
            case 'adelanto':
                badgeClass = 'badge-adelanto';
                iconClass = 'bi-credit-card-2-front';
                break;
            case 'parcial':
                badgeClass = 'badge-parcial';
                iconClass = 'bi-credit-card';
                break;
        }
        
        // Create a nice-looking badge for payment type
        const badge = `<span class="badge rounded-pill ${badgeClass}"><i class="bi ${iconClass} me-1"></i>${payment.tipoPagoNombre || '-'}</span>`;
        
        row.innerHTML = `
            <td>${payment.id}</td>
            <td>${badge}</td>
            <td>S/${amount.toFixed(2)}</td>
            <td>${formattedDate}</td>
        `;
        tableBody.appendChild(row);
    });
    
    // Update total
    if (paymentsTotalElement) {
        paymentsTotalElement.textContent = `S/${totalAmount.toFixed(2)}`;
    }
}