// Manejo del formulario de creación de reservas

import { showAlert } from './utils.js';

// Variables globales para el formulario
let selectedServices = [];
let priceManuallyEdited = false;

export function initializeReservationForm() {
    setupClientSelection();
    setupServiceSelection();
    setupPriceManagement();
    setupFormSubmission();
    setupModalReset();
}

function setupClientSelection() {
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
    
    // *** NUEVA VALIDACIÓN: Validación en tiempo real para fecha del evento ***
    const newEventDate = document.getElementById('newEventDate');
    if (newEventDate) {
        newEventDate.addEventListener('change', function() {
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

function setupServiceSelection() {
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
                
                selectedServices = [{
                    id: serviceId,
                    name: serviceName,
                    price: servicePrice
                }];
                
                if (selectedServiceDisplay && selectedServiceName && selectedServicePrice) {
                    selectedServiceName.textContent = serviceName;
                    selectedServicePrice.textContent = `S/${servicePrice.toFixed(2)}`;
                    selectedServiceDisplay.classList.remove('d-none');
                }
                
                updateTotalPrice();
                this.style.display = 'none';
                
                const servicesHelp = document.getElementById('servicesHelp');
                if (servicesHelp) {
                    servicesHelp.classList.add('d-none');
                }
            }
        });
    }
    
    // Función global para remover servicio
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
}

function setupPriceManagement() {
    const totalPriceInput = document.getElementById('totalPrice');
    if (totalPriceInput) {
        totalPriceInput.addEventListener('input', function() {
            priceManuallyEdited = true;
            this.classList.add('border-warning');
            
            const priceInfoText = document.querySelector('.form-text');
            if (priceInfoText) {
                priceInfoText.innerHTML = `
                    <i class="bi bi-exclamation-triangle-fill text-warning me-1"></i>
                    Precio modificado manualmente (S/${this.value})
                `;
            }
        });
    }

    const resetTotalPriceBtn = document.getElementById('resetTotalPrice');
    if (resetTotalPriceBtn) {
        resetTotalPriceBtn.addEventListener('click', function() {
            priceManuallyEdited = false;
            
            let servicesTotalAmount = 0;
            selectedServices.forEach(service => {
                servicesTotalAmount += service.price;
            });
            
            if (totalPriceInput) {
                totalPriceInput.value = servicesTotalAmount.toFixed(2);
                totalPriceInput.classList.remove('border-warning');
                
                const priceInfoText = document.querySelector('.form-text');
                if (priceInfoText) {
                    priceInfoText.innerHTML = `
                        <i class="bi bi-info-circle me-1"></i>
                        Este valor se actualiza automáticamente según los servicios seleccionados.
                    `;
                }
            }
        });
    }
}

function updateTotalPrice() {
    let servicesTotalAmount = 0;
    
    if (selectedServices.length > 0) {
        servicesTotalAmount = selectedServices[0].price;
    }
    
    const totalPriceInput = document.getElementById('totalPrice');
    if (!priceManuallyEdited && totalPriceInput) {
        totalPriceInput.value = servicesTotalAmount.toFixed(2);
    }
    
    const servicesHelp = document.getElementById('servicesHelp');
    if (servicesHelp) {
        if (selectedServices.length > 0) {
            servicesHelp.classList.add('d-none');
        } else {
            servicesHelp.classList.remove('d-none');
        }
    }
}

function setupFormSubmission() {
    const createReservationBtn = document.getElementById('createReservationBtn');
    
    if (createReservationBtn) {
        createReservationBtn.addEventListener('click', async function(event) {
            event.preventDefault();
            
            const button = this;
            
            try {
                if (!validateReservationForm()) {
                    return;
                }
                
                const originalHtml = button.innerHTML;
                button.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Creando...';
                button.disabled = true;
                
                const reservationData = collectFormData();
                
                console.log('Enviando datos para crear reserva:', reservationData);
                
                const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
                const token = tokenElement ? tokenElement.value : '';
                
                const response = await fetch('/Reservas', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': token
                    },
                    body: JSON.stringify(reservationData)
                });
                
                // DEBUGGING COMPLETO DE LA RESPUESTA
                console.log('=== RESPONSE DEBUGGING ===');
                console.log('Response status:', response.status);
                console.log('Response ok:', response.ok);
                console.log('Response headers:', Object.fromEntries(response.headers.entries()));
                
                // Leer la respuesta como texto primero
                const responseText = await response.text();
                console.log('Response text completo:', responseText);
                console.log('Response text length:', responseText.length);
                
                let result = null;
                try {
                    if (responseText && responseText.trim()) {
                        result = JSON.parse(responseText);
                        console.log('Parsed result:', result);
                    } else {
                        console.log('Response text está vacío');
                        result = { message: 'Respuesta vacía del servidor' };
                    }
                } catch (e) {
                    console.error('Error parsing JSON:', e);
                    console.log('Raw response that failed to parse:', responseText);
                    result = { message: responseText || 'Error al procesar la respuesta' };
                }
                
                console.log('=== END RESPONSE DEBUGGING ===');
                
                if (response.ok && result && result.success !== false) {
                    if (typeof bootstrap !== 'undefined') {
                        const modal = bootstrap.Modal.getInstance(document.getElementById('newReservationModal'));
                        if (modal) modal.hide();
                    }
                    
                    showAlert('success', 'Reserva creada exitosamente');
                    
                    setTimeout(() => {
                        window.location.href = '/Reservas?success=true';
                    }, 1500);
                } else {
                    // Buscar información de stock en toda la respuesta
                    const fullResponseString = JSON.stringify(result) + ' ' + responseText;
                    const errorMessage = parseCompleteErrorMessage(fullResponseString, result?.message);
                    throw new Error(errorMessage);
                }
            } catch (error) {
                console.error('Error al crear la reserva:', error);
                showAlert('danger', error.message || 'Ocurrió un error al crear la reserva');
            } finally {
                // Restaurar botón
                if (button) {
                    button.innerHTML = '<i class="bi bi-save me-1"></i>Crear Reserva';
                    button.disabled = false;
                }
            }
        });
    }
}

// Nueva función para parsear errores de forma más amplia - VERSIÓN ULTRA SIMPLE
function parseCompleteErrorMessage(fullResponse, primaryMessage) {
    console.log('=== PARSING COMPLETE ERROR ===');
    console.log('Primary message:', primaryMessage);
    console.log('Full response:', fullResponse);
    
    // Convertir a string para análisis
    const messageStr = String(primaryMessage || '').toLowerCase();
    const fullResponseStr = String(fullResponse || '').toLowerCase();
    
    // Buscar errores de fecha
    if (messageStr.includes('fecha anterior') || messageStr.includes('fecha pasada') || 
        messageStr.includes('día actual') || messageStr.includes('date') && messageStr.includes('past')) {
        console.log('✓ Error de fecha detectado');
        return primaryMessage; // Devolver el mensaje tal como viene del backend
    }
    
    // Buscar en el mensaje principal
    if (messageStr.includes('stock') || messageStr.includes('insuficiente') || messageStr.includes('inventario')) {
        console.log('✓ Mensaje principal contiene información de stock');
        return primaryMessage; // Devolver el mensaje tal como viene del backend
    }
    
    // Buscar en toda la respuesta
    if (fullResponseStr.includes('stock') || fullResponseStr.includes('insuficiente') || fullResponseStr.includes('inventario')) {
        console.log('✓ Respuesta completa contiene información de stock');
        
        // Intentar extraer el mensaje de stock de la respuesta completa
        const stockMatch = fullResponseStr.match(/stock insuficiente[^"]*([^"]*)/i);
        if (stockMatch) {
            return stockMatch[0];
        }
        
        return 'No hay stock suficiente para realizar esta operación';
    }
    
    // Si no hay información de stock, devolver mensaje genérico
    console.log('✗ No se encontraron indicios de error de stock o fecha');
    return 'Error al crear la reserva. Por favor, intente nuevamente.';
}

function collectFormData() {
    const newEventName = document.getElementById('newEventName');
    const newEventDate = document.getElementById('newEventDate');
    const newEventDescription = document.getElementById('newEventDescription');
    const newEventStatus = document.getElementById('newEventStatus');
    const totalPrice = document.getElementById('totalPrice');
    const clientSelect = document.getElementById('clientSelect');
    const newEventType = document.getElementById('newEventType');
    const advancePrice = document.getElementById('advancePrice');
    const existingClient = document.getElementById('existingClient');
    
    const newClientName = document.getElementById('newClientName');
    const newClientEmail = document.getElementById('newClientEmail');
    const newClientPhone = document.getElementById('newClientPhone');
    
    return {
        nombreEvento: newEventName ? newEventName.value : "",
        fechaEjecucion: newEventDate ? newEventDate.value : "",
        descripcion: newEventDescription ? newEventDescription.value : "",
        estado: newEventStatus ? newEventStatus.value : "Pendiente",
        precioTotal: totalPrice ? parseFloat(totalPrice.value) : 0,
        tipoEventoNombre: newEventType ? newEventType.value : "",
        servicioId: selectedServices.length > 0 ? selectedServices[0].id : null,
        precioAdelanto: advancePrice ? parseFloat(advancePrice.value) : 0,
        
        clienteId: (existingClient && existingClient.checked) ? clientSelect.value : null,
        nombreCliente: (!existingClient || !existingClient.checked) ? 
            (newClientName ? newClientName.value : "") : null,
        correoElectronico: (!existingClient || !existingClient.checked) ? 
            (newClientEmail ? newClientEmail.value : "") : null,
        telefono: (!existingClient || !existingClient.checked) ? 
            (newClientPhone ? newClientPhone.value : "") : null
    };
}

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
        isValid = validateNewClientFields() && isValid;
    }
    
    // Validar datos del evento
    isValid = validateEventFields() && isValid;
    
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

function validateNewClientFields() {
    let isValid = true;
    
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
    
    return isValid;
}

function validateEventFields() {
    let isValid = true;
    
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
            // *** NUEVA VALIDACIÓN: Verificar que la fecha no sea en el pasado ***
            const selectedDate = new Date(eventDateInput.value);
            const today = new Date();
            
            // Resetear las horas para comparar solo las fechas
            today.setHours(0, 0, 0, 0);
            selectedDate.setHours(0, 0, 0, 0);
            
            if (selectedDate < today) {
                eventDateInput.classList.add('is-invalid');
                showAlert('warning', 'La fecha del evento no puede ser anterior a la fecha actual');
                isValid = false;
            } else {
                eventDateInput.classList.remove('is-invalid');
            }
        }
    }
    
    return isValid;
}

function setupModalReset() {
    const newReservationModal = document.getElementById('newReservationModal');
    if (newReservationModal) {
        newReservationModal.addEventListener('show.bs.modal', function() {
            priceManuallyEdited = false;
            selectedServices = [];
            
            const totalPriceInput = document.getElementById('totalPrice');
            if (totalPriceInput) {
                totalPriceInput.value = "0.00";
            }
            
            const serviceSelect = document.getElementById('serviceSelect');
            const selectedServiceDisplay = document.getElementById('selectedServiceDisplay');
            
            if (serviceSelect) {
                serviceSelect.selectedIndex = 0;
                serviceSelect.style.display = 'block';
            }
            
            if (selectedServiceDisplay) {
                selectedServiceDisplay.classList.add('d-none');
            }
            
            const servicesHelp = document.getElementById('servicesHelp');
            if (servicesHelp) {
                servicesHelp.classList.add('d-none');
            }
        });
    }
}

// Función para parsear mensajes de error específicos - VERSIÓN CON DEBUGGING COMPLETO
function parseErrorMessage(originalMessage) {
    console.log('=== DEBUGGING ERROR MESSAGE ===');
    console.log('Mensaje original completo:', originalMessage);
    console.log('Tipo del mensaje:', typeof originalMessage);
    console.log('Longitud del mensaje:', originalMessage ? originalMessage.length : 0);
    
    // Convertir a string si no lo es
    const messageStr = String(originalMessage || '').toLowerCase();
    console.log('Mensaje en minúsculas:', messageStr);
    
    // Buscar palabras clave relacionadas con stock
    const stockKeywords = ['stock', 'inventario', 'disponible', 'insuficiente', 'falta', 'agotado'];
    const hasStockKeyword = stockKeywords.some(keyword => messageStr.includes(keyword));
    
    console.log('Contiene palabras de stock:', hasStockKeyword);
    console.log('Palabras encontradas:', stockKeywords.filter(keyword => messageStr.includes(keyword)));
    
    if (hasStockKeyword) {
        console.log('✓ Es un error relacionado con stock');
        return 'No hay stock suficiente para realizar esta reserva. Por favor, seleccione otro servicio o verifique la disponibilidad.';
    }
    
    console.log('✗ No es un error de stock');
    console.log('=== END DEBUGGING ===');
    
    // Si no es error de stock, mensaje genérico
    return 'Error al crear la reserva. Por favor, intente nuevamente.';
}

export { selectedServices };