/**
 * Script para la generación de boletas de reservas
 */

// Variable para almacenar los datos de la reserva actual
let currentReservationData = null;

// Inicializar los eventos cuando el DOM está listo
document.addEventListener('DOMContentLoaded', function() {
    // Configurar el botón para generar boleta en el modal de detalles
    const generateReceiptBtn = document.getElementById('generateReceiptBtn');
    if (generateReceiptBtn) {
        generateReceiptBtn.addEventListener('click', handleGenerateReceipt);
    }
    
    // Configurar botones en el modal de boleta
    const printReceiptBtn = document.getElementById('printReceiptBtn');
    if (printReceiptBtn) {
        printReceiptBtn.addEventListener('click', handlePrintReceipt);
    }
    
    const downloadReceiptBtn = document.getElementById('downloadReceiptBtn');
    if (downloadReceiptBtn) {
        downloadReceiptBtn.addEventListener('click', handleDownloadReceipt);
    }
});

/**
 * Manejador para el botón "Generar Boleta"
 */
function handleGenerateReceipt() {
    try {
        console.log('Generando boleta...');
        
        // Verificar que tenemos datos de reserva
        if (!window.currentReservationData) {
            console.error('No hay datos de reserva disponibles');
            alert('Error: No hay datos disponibles para generar la boleta');
            return;
        }
        
        // Guardar los datos para su uso en el modal de boleta
        currentReservationData = window.currentReservationData;
        console.log('Datos de reserva para boleta:', currentReservationData);
        
        // Generar el contenido HTML de la boleta
        const receiptContent = generateReceiptHTML(currentReservationData);
        
        // Insertar el contenido en el modal
        const receiptContentContainer = document.getElementById('receiptContent');
        if (receiptContentContainer) {
            receiptContentContainer.innerHTML = receiptContent;
        } else {
            console.error('Contenedor de boleta no encontrado');
            return;
        }
        
        // Ocultar el modal de detalles
        const viewModal = bootstrap.Modal.getInstance(document.getElementById('viewReservationModal'));
        if (viewModal) {
            viewModal.hide();
            
            // Dar tiempo para que se cierre antes de abrir el otro
            setTimeout(() => {
                // Mostrar el modal de boleta
                const reciboModal = document.getElementById('reciboModal');
                if (reciboModal) {
                    const bsReciboModal = new bootstrap.Modal(reciboModal);
                    bsReciboModal.show();
                }
            }, 500);
        }
    } catch (error) {
        console.error('Error al generar la boleta:', error);
        alert('Error al generar la boleta: ' + error.message);
    }
}

/**
 * Genera el HTML para la boleta usando los datos de la reserva
 */
function generateReceiptHTML(reservation) {
    try {
        // Obtener fecha actual formateada
        const now = new Date();
        const formattedNow = now.toLocaleDateString('es-ES', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        }) + ' ' + now.toLocaleTimeString('es-ES', {
            hour: '2-digit',
            minute: '2-digit'
        });
        
        // Formatear la fecha del evento
        const eventDate = new Date(reservation.fechaEjecucion);
        const formattedEventDate = eventDate.toLocaleDateString('es-ES', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
        
        // Formatear montos
        const formatCurrency = (amount) => {
            return 'S/ ' + parseFloat(amount).toFixed(2);
        };
        
        // Determinar clase CSS para el estado
        let statusClass = '';
        let statusText = reservation.estado || 'Pendiente';
        
        switch(statusText.toLowerCase()) {
            case 'confirmado':
            case 'confirmed':
                statusClass = 'recibo-badge-confirmado';
                statusText = 'Confirmado';
                break;
            case 'pendiente':
            case 'pending':
                statusClass = 'recibo-badge-pendiente';
                statusText = 'Pendiente';
                break;
            case 'cancelado':
            case 'cancelled':
            case 'canceled':
                statusClass = 'recibo-badge-cancelado';
                statusText = 'Cancelado';
                break;
            default:
                statusClass = 'badge-secondary';
        }
        
        // Generar HTML para servicios
        const serviceHTML = `
            <tr>
                <td>${reservation.servicioId || '-'}</td>
                <td>${reservation.nombreServicio || 'Servicio no especificado'}</td>
                <td class="text-end">${formatCurrency(reservation.precioTotal)}</td>
            </tr>
        `;
        
        // Calcular saldo pendiente
        const totalPagado = reservation.totalPagado || 0;
        const saldoPendiente = reservation.precioTotal - totalPagado;
        
        // Generar HTML para pagos si existen
        let paymentsHTML = '';
        if (window.reservationPayments && window.reservationPayments.length > 0) {
            window.reservationPayments.forEach(payment => {
                const paymentDate = new Date(payment.fechaPago).toLocaleDateString('es-ES', {
                    day: '2-digit',
                    month: '2-digit',
                    year: 'numeric'
                });
                
                paymentsHTML += `
                    <tr>
                        <td>${payment.id}</td>
                        <td>${payment.tipoPagoNombre || 'No especificado'}</td>
                        <td>${formatCurrency(payment.monto)}</td>
                        <td>${paymentDate}</td>
                    </tr>
                `;
            });
        } else {
            paymentsHTML = `
                <tr>
                    <td colspan="4" class="text-center">No hay pagos registrados</td>
                </tr>
            `;
        }
        
        // Construcción del HTML completo de la boleta
        return `
            <div class="recibo-container">
                <div class="recibo-header">
                    <div class="row">
                        <div class="col-md-6">
                            <h1 class="recibo-title">EVENTUS</h1>
                            <p class="recibo-subtitle">Gestión Profesional de Eventos</p>
                            <p class="mb-0">RUC: 20605123456</p>
                            <p class="mb-0">Av. Principal 123, Lima</p>
                            <p class="mb-0">Tel: (01) 555-1234</p>
                        </div>
                        <div class="col-md-6 text-end">
                            <div class="recibo-number">BOLETA N°: B-${reservation.id}</div>
                            <div class="recibo-date">Fecha de emisión: ${formattedNow}</div>
                        </div>
                    </div>
                </div>
                
                <div class="recibo-body">
                    <div class="recibo-section">
                        <h2 class="recibo-section-title">Información del Evento</h2>
                        <div class="row">
                            <div class="col-md-6">
                                <div class="recibo-row">
                                    <div class="recibo-label">Nombre del evento:</div>
                                    <div class="recibo-value">${reservation.nombreEvento || '-'}</div>
                                </div>
                                <div class="recibo-row">
                                    <div class="recibo-label">Tipo:</div>
                                    <div class="recibo-value">${reservation.tipoEventoNombre || '-'}</div>
                                </div>
                                <div class="recibo-row">
                                    <div class="recibo-label">Fecha y hora:</div>
                                    <div class="recibo-value">${formattedEventDate}</div>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="recibo-row">
                                    <div class="recibo-label">Estado:</div>
                                    <div class="recibo-value">
                                        <span class="recibo-badge ${statusClass}">${statusText}</span>
                                    </div>
                                </div>
                                <div class="recibo-row">
                                    <div class="recibo-label">ID de Reserva:</div>
                                    <div class="recibo-value">${reservation.id}</div>
                                </div>
                                <div class="recibo-row">
                                    <div class="recibo-label">Fecha de registro:</div>
                                    <div class="recibo-value">
                                        ${new Date(reservation.fechaRegistro).toLocaleDateString('es-ES', {
                                            day: '2-digit',
                                            month: '2-digit',
                                            year: 'numeric'
                                        })}
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    <div class="recibo-section">
                        <h2 class="recibo-section-title">Información del Cliente</h2>
                        <div class="row">
                            <div class="col-md-6">
                                <div class="recibo-row">
                                    <div class="recibo-label">Nombre:</div>
                                    <div class="recibo-value">${reservation.nombreCliente || '-'}</div>
                                </div>
                                <div class="recibo-row">
                                    <div class="recibo-label">Email:</div>
                                    <div class="recibo-value">${reservation.correoCliente || '-'}</div>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="recibo-row">
                                    <div class="recibo-label">Teléfono:</div>
                                    <div class="recibo-value">${reservation.telefonoCliente || '-'}</div>
                                </div>
                                <div class="recibo-row">
                                    <div class="recibo-label">ID Cliente:</div>
                                    <div class="recibo-value">${reservation.clienteId || '-'}</div>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    <div class="recibo-section">
                        <h2 class="recibo-section-title">Servicios Contratados</h2>
                        <table class="recibo-table">
                            <thead>
                                <tr>
                                    <th>ID Servicio</th>
                                    <th>Descripción</th>
                                    <th class="text-end">Precio</th>
                                </tr>
                            </thead>
                            <tbody>
                                ${serviceHTML}
                            </tbody>
                            <tfoot>
                                <tr class="recibo-total">
                                    <td colspan="2" class="text-end">Total:</td>
                                    <td class="text-end">${formatCurrency(reservation.precioTotal)}</td>
                                </tr>
                            </tfoot>
                        </table>
                    </div>
                    
                    <div class="recibo-section">
                        <h2 class="recibo-section-title">Historial de Pagos</h2>
                        <table class="recibo-table">
                            <thead>
                                <tr>
                                    <th>ID Pago</th>
                                    <th>Tipo</th>
                                    <th>Monto</th>
                                    <th>Fecha</th>
                                </tr>
                            </thead>
                            <tbody>
                                ${paymentsHTML}
                            </tbody>
                            <tfoot>
                                <tr class="recibo-total">
                                    <td colspan="2" class="text-end">Total pagado:</td>
                                    <td colspan="2">${formatCurrency(totalPagado)}</td>
                                </tr>
                                <tr class="recibo-total">
                                    <td colspan="2" class="text-end">Saldo pendiente:</td>
                                    <td colspan="2">${formatCurrency(saldoPendiente)}</td>
                                </tr>
                            </tfoot>
                        </table>
                    </div>
                    
                    <div class="recibo-section">
                        <div class="row">
                            <div class="col-md-6">
                                <h2 class="recibo-section-title">Observaciones</h2>
                                <p>${reservation.descripcion || 'Sin observaciones adicionales.'}</p>
                            </div>
                            <div class="col-md-6 text-center">
                                <div class="recibo-qr">
                                    <img src="https://api.qrserver.com/v1/create-qr-code/?size=100x100&data=${encodeURIComponent(`RESERVA:${reservation.id}|CLIENTE:${reservation.nombreCliente || '-'}|EVENTO:${reservation.nombreEvento || '-'}`)}" alt="QR Code" />
                                    <p class="mt-2 small">Escanea para verificar</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                
                <div class="recibo-footer">
                    <p>¡Gracias por confiar en nosotros para su evento!</p>
                    <p class="small mb-0">Este documento es un comprobante de reserva y no un comprobante fiscal.</p>
                </div>
                
                <div class="recibo-terms">
                    <p><strong>Términos y Condiciones:</strong></p>
                    <ul>
                        <li>Esta reserva está sujeta a los términos y condiciones generales de contratación de EVENTUS.</li>
                        <li>Para cancelaciones, se debe notificar con al menos 15 días de anticipación.</li>
                        <li>El pago total debe realizarse 7 días antes del evento para confirmar la reserva.</li>
                    </ul>
                </div>
            </div>
        `;
    } catch (error) {
        console.error('Error al generar HTML de boleta:', error);
        return `<div class="alert alert-danger">Error al generar la boleta: ${error.message}</div>`;
    }
}

/**
 * Manejador para el botón de imprimir
 */
function handlePrintReceipt() {
    try {
        console.log('Imprimiendo boleta...');
        
        // Obtener el contenido del recibo
        const receiptContent = document.getElementById('receiptContent');
        if (!receiptContent) {
            throw new Error('No se encontró el contenido del recibo');
        }
        
        // Crear un iframe temporal (invisible)
        const iframe = document.createElement('iframe');
        iframe.style.position = 'absolute';
        iframe.style.top = '-9999px';
        iframe.style.left = '-9999px';
        iframe.style.width = '800px'; // Ancho fijo para mantener diseño
        iframe.style.height = '1200px'; // Alto suficiente para todo el contenido
        document.body.appendChild(iframe);
        
        // Esperar a que el iframe esté listo
        iframe.onload = function() {
            try {
                // Acceder al documento del iframe
                const iframeDoc = iframe.contentDocument || iframe.contentWindow.document;
                
                // Obtener todos los estilos del documento principal
                const styles = Array.from(document.styleSheets)
                    .map(styleSheet => {
                        try {
                            // Intentamos acceder a las reglas CSS - esto puede fallar por CORS
                            return Array.from(styleSheet.cssRules)
                                .map(rule => rule.cssText)
                                .join('\n');
                        } catch (e) {
                            // Si falla por CORS, simplemente vinculamos la hoja de estilo externa
                            if (styleSheet.href) {
                                return `@import url("${styleSheet.href}");`;
                            }
                            return '';
                        }
                    })
                    .join('\n');
                
                // Escribir el HTML completo para la impresión, incluyendo todos los estilos
                iframeDoc.open();
                iframeDoc.write(`
                    <!DOCTYPE html>
                    <html lang="es">
                    <head>
                        <meta charset="UTF-8">
                        <title>Boleta de Reserva #${currentReservationData?.id || ''}</title>
                        <link rel="stylesheet" href="/lib/bootstrap/dist/css/bootstrap.min.css" />
                        <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.1/font/bootstrap-icons.css">
                        <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap" rel="stylesheet">
                        <link rel="stylesheet" href="/css/recibo-styles.css" />
                        <style>
                            body {
                                padding: 0;
                                margin: 0;
                                background-color: white;
                                overflow: hidden;
                            }
                            
                            /* Anular cualquier estilo de media print que pueda estar causando problemas */
                            @media print {
                                * { visibility: visible !important; }
                                body { margin: 0; padding: 0; }
                                .recibo-container { 
                                    box-shadow: none !important;
                                    margin: 0 auto !important;
                                    padding: 0 !important;
                                }
                            }
                            
                            /* Estilos adicionales aquí */
                            ${styles}
                        </style>
                    </head>
                    <body>
                        <div style="max-width: 800px; margin: 0 auto; padding: 0;">
                            ${receiptContent.innerHTML}
                        </div>
                    </body>
                    </html>
                `);
                iframeDoc.close();
                
                // Esperar a que todos los recursos se carguen correctamente
                setTimeout(function() {
                    // Imprimir el iframe
                    iframe.contentWindow.print();
                    
                    // Eliminar el iframe después de imprimir
                    setTimeout(function() {
                        document.body.removeChild(iframe);
                    }, 1000);
                }, 1000); // Tiempo ampliado para asegurar carga completa
                
            } catch (error) {
                console.error('Error en el iframe:', error);
                document.body.removeChild(iframe);
                alert('Error al preparar la impresión: ' + error.message);
            }
        };
        
        // Iniciar la carga del iframe
        iframe.src = 'about:blank';
        
    } catch (error) {
        console.error('Error al imprimir boleta:', error);
        alert('Error al imprimir: ' + error.message);
    }
}

/**
 * Manejador para el botón de descarga como PDF
 * Nota: Requiere la biblioteca html2pdf.js
 */
function handleDownloadReceipt() {
    try {
        // Comprobar si la biblioteca html2pdf.js está disponible
        if (typeof html2pdf === 'undefined') {
            // Cargar la biblioteca dinámicamente si no está disponible
            const script = document.createElement('script');
            script.src = 'https://cdnjs.cloudflare.com/ajax/libs/html2pdf.js/0.10.1/html2pdf.bundle.min.js';
            script.onload = generatePDF;
            document.head.appendChild(script);
        } else {
            generatePDF();
        }
    } catch (error) {
        console.error('Error al descargar PDF:', error);
        alert('Error al generar PDF: ' + error.message);
    }
}

/**
 * Función que genera y descarga el PDF
 */
function generatePDF() {
    const element = document.getElementById('receiptContent');
    const options = {
        margin: 10,
        filename: `Boleta_Reserva_${currentReservationData?.id || 'Detalle'}.pdf`,
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: { scale: 2 },
        jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait' }
    };
    
    // Mostrar indicador de carga
    const downloadBtn = document.getElementById('downloadReceiptBtn');
    const originalBtnText = downloadBtn.innerHTML;
    downloadBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Generando...';
    downloadBtn.disabled = true;
    
    // Generar el PDF
    html2pdf()
        .set(options)
        .from(element)
        .save()
        .then(() => {
            console.log('PDF generado correctamente');
            // Restaurar el botón
            downloadBtn.innerHTML = originalBtnText;
            downloadBtn.disabled = false;
        })
        .catch(error => {
            console.error('Error al generar PDF:', error);
            alert('Error al generar el PDF: ' + error.message);
            downloadBtn.innerHTML = originalBtnText;
            downloadBtn.disabled = false;
        });
}