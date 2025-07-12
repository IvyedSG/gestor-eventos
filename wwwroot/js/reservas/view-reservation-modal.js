// Manejo del modal de visualización de reservas

import { safeSetText, safeSetHtml, formatDate, formatCurrency } from './utils.js';

export async function openReservationDetailsModal(reservationId) {
    console.log('Opening modal for reservation:', reservationId);
    
    const modalElement = document.getElementById('viewReservationModal');
    if (!modalElement) {
        console.error('Modal element not found!');
        return;
    }
    
    try {
        if (typeof bootstrap === 'undefined') {
            console.error('Bootstrap no está disponible');
            return;
        }
        
        let modal = bootstrap.Modal.getInstance(modalElement);
        if (!modal) {
            modal = new bootstrap.Modal(modalElement);
        }
        
        modal.show();
        
        // Mostrar estado de carga
        const loadingElement = document.getElementById('reservationLoading');
        const detailsElement = document.getElementById('reservationDetails');
        const errorElement = document.getElementById('reservationError');
        
        if (loadingElement) loadingElement.style.display = 'block';
        if (detailsElement) detailsElement.style.display = 'none';
        if (errorElement) errorElement.style.display = 'none';
        
        // Obtener token antifalsificación
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        const token = tokenElement ? tokenElement.value : '';
        
        console.log('Fetching reservation details for ID:', reservationId);
        
        // Obtener detalles de la reserva
        const response = await fetch(`/Reservas?handler=ReservationDetails&id=${reservationId}`, {
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': token
            }
        });
        
        const responseText = await response.text();
        console.log('Response received:', responseText.substring(0, 100) + '...');
        
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
        
        // Llenar el modal con los datos de la reserva
        fillReservationModal(result.data);
        
        // Obtener historial de pagos
        await fetchPaymentHistory(reservationId, token);
        
        // Ocultar carga, mostrar detalles
        if (loadingElement) loadingElement.style.display = 'none';
        if (detailsElement) detailsElement.style.display = 'block';
        
    } catch (error) {
        console.error('Error loading reservation details:', error);
        
        const loadingElement = document.getElementById('reservationLoading');
        const errorElement = document.getElementById('reservationError');
        const errorMessageElement = document.getElementById('reservationErrorMessage');
        
        if (loadingElement) loadingElement.style.display = 'none';
        if (errorElement) errorElement.style.display = 'block';
        if (errorMessageElement) errorMessageElement.textContent = error.message || 'Error al cargar los detalles.';
    }
}

async function fetchPaymentHistory(reservationId, token) {
    try {
        console.log('Fetching payment history for reservation:', reservationId);
        
        const paymentsResponse = await fetch(`/Reservas?handler=ReservationPayments&id=${reservationId}`, {
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': token
            }
        });
        
        if (paymentsResponse.ok) {
            const paymentsText = await paymentsResponse.text();
            let paymentsResult;
            
            try {
                paymentsResult = JSON.parse(paymentsText);
                if (paymentsResult.success) {
                    fillPaymentsTable(paymentsResult.data);
                }
            } catch (error) {
                console.error('Error parsing payments data:', error);
            }
        } else {
            console.warn('Failed to fetch payment history:', paymentsResponse.status);
        }
    } catch (error) {
        console.error('Error fetching payment history:', error);
    }
}

function fillReservationModal(reservation) {
    console.log('Filling modal with reservation data:', reservation);
    
    try {
        // Almacenar los datos para uso en la generación de boleta
        window.currentReservationData = reservation;
        
        // Información general de la reserva
        safeSetText('reservationId', reservation.id);
        safeSetText('reservationName', reservation.nombreEvento);
        safeSetText('reservationType', reservation.tipoEventoNombre);
        
        // Formatear y mostrar el estado con estilo apropiado
        const status = (reservation.estado || 'Pendiente').toUpperCase();
        let statusText = '';
        
        switch(status) {
            case 'CONFIRMADO':
            case 'CONFIRMED':
                statusText = '<span class="badge bg-success">Confirmado</span>';
                break;
            case 'PENDIENTE':
            case 'PENDING':
                statusText = '<span class="badge bg-warning text-dark">Pendiente</span>';
                break;
            case 'CANCELADO':
            case 'CANCELED':
            case 'CANCELLED':
                statusText = '<span class="badge bg-danger">Cancelado</span>';
                break;
            case 'FINALIZADO':
            case 'COMPLETED':
            case 'FINISHED':
                statusText = '<span class="badge bg-info">Finalizado</span>';
                break;
            default:
                statusText = `<span class="badge bg-secondary">${reservation.estado || '-'}</span>`;
        }
        
        safeSetHtml('reservationStatus', statusText);
        
        // Fechas
        safeSetText('reservationEventDate', formatDate(reservation.fechaEjecucion));
        safeSetText('reservationRegistrationDate', formatDate(reservation.fechaRegistro));
        
        // Información del cliente
        safeSetText('clientName', reservation.nombreCliente);
        safeSetText('clientEmail', reservation.correoCliente);
        safeSetText('clientPhone', reservation.telefonoCliente);
        safeSetText('clientId', reservation.clienteId);
        
        // Detalles del servicio
        safeSetText('serviceName', reservation.nombreServicio);
        safeSetText('serviceId', reservation.servicioId);
        
        // Información de pago
        safeSetText('reservationTotalPrice', formatCurrency(reservation.precioTotal));
        safeSetText('reservationAdvancePrice', formatCurrency(reservation.precioAdelanto));
        safeSetText('reservationTotalPaid', formatCurrency(reservation.totalPagado));
        safeSetText('reservationLastPayment', reservation.ultimoPago ? formatDate(reservation.ultimoPago) : '-');
        
        // Descripción
        safeSetText('reservationDescription', reservation.descripcion || 'Sin descripción');
        
        console.log('Modal filled successfully with reservation data');
    } catch (error) {
        console.error('Error filling modal with data:', error);
    }
}

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
    
    // Limpiar contenido previo
    tableBody.innerHTML = '';
    
    // Manejar caso sin pagos
    if (!payments || payments.length === 0) {
        paymentsTable.style.display = 'none';
        noPaymentsMessage.style.display = 'block';
        return;
    }
    
    // Tenemos pagos
    paymentsTable.style.display = 'table';
    noPaymentsMessage.style.display = 'none';
    
    // Calcular total
    let totalAmount = 0;
    
    // Agregar filas para cada pago
    payments.forEach(payment => {
        const amount = parseFloat(payment.monto) || 0;
        totalAmount += amount;
        
        // Formatear fecha
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
        
        // Crear fila de pago con badge para tipo de pago
        const row = document.createElement('tr');
        
        // Determinar clase de badge según tipo de pago
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
        
        // Crear badge atractivo para tipo de pago
        const badge = `<span class="badge rounded-pill ${badgeClass}"><i class="bi ${iconClass} me-1"></i>${payment.tipoPagoNombre || '-'}</span>`;
        
        row.innerHTML = `
            <td>${payment.id}</td>
            <td>${badge}</td>
            <td>S/${amount.toFixed(2)}</td>
            <td>${formattedDate}</td>
        `;
        tableBody.appendChild(row);
    });
    
    // Actualizar total
    if (paymentsTotalElement) {
        paymentsTotalElement.textContent = `S/${totalAmount.toFixed(2)}`;
    }
}