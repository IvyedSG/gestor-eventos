// Manejo de carga de datos de reportes

import { fetchWithToken, showAllChartLoadings, formatCurrency } from './utils.js';
import { getFilterDates } from './filters.js';
import { renderAllCharts } from './charts.js';
import { showErrorAlert } from './alerts.js';

// Variable global para almacenar datos de reportes
export let reportesData = {
    resumenEjecutivo: null,
    clientes: null,
    items: null,
    pagos: null,
    reservas: null,
    servicios: null
};

export function initializeReportsData() {
    // Hacer la función de carga disponible globalmente
    window.loadAllReports = loadAllReports;
    console.log('Reports data module initialized successfully');
}

async function loadAllReports() {
    console.log('loadAllReports - Iniciando carga de reportes');
    
    // Mostrar todos los overlays de carga
    showAllChartLoadings();
    
    const { fechaInicio, fechaFin } = getFilterDates();
    
    console.log('Fechas para consulta:', { fechaInicio, fechaFin });
    
    try {
        console.log('Haciendo llamadas a la API...');
        
        // Cargar todos los reportes en paralelo
        const [resumenResp, clientesResp, itemsResp, pagosResp, reservasResp, serviciosResp] = await Promise.all([
            fetchWithToken(`/Reportes?handler=ResumenEjecutivo&fechaInicio=${fechaInicio}&fechaFin=${fechaFin}`),
            fetchWithToken(`/Reportes?handler=ReportesClientes&fechaInicio=${fechaInicio}&fechaFin=${fechaFin}`),
            fetchWithToken(`/Reportes?handler=ReportesItems&fechaInicio=${fechaInicio}&fechaFin=${fechaFin}&top=10`),
            fetchWithToken(`/Reportes?handler=ReportesPagos&fechaInicio=${fechaInicio}&fechaFin=${fechaFin}`),
            fetchWithToken(`/Reportes?handler=ReportesReservas&fechaInicio=${fechaInicio}&fechaFin=${fechaFin}`),
            fetchWithToken(`/Reportes?handler=ReportesServicios&fechaInicio=${fechaInicio}&fechaFin=${fechaFin}&top=10`)
        ]);

        console.log('Respuestas recibidas:', {
            resumen: resumenResp.status,
            clientes: clientesResp.status,
            items: itemsResp.status,
            pagos: pagosResp.status,
            reservas: reservasResp.status,
            servicios: serviciosResp.status
        });

        // Procesar respuestas exitosas
        await processResponses(resumenResp, clientesResp, itemsResp, pagosResp, reservasResp, serviciosResp);
        
        // Actualizar UI
        console.log('Actualizando tarjetas de resumen...');
        updateSummaryCards();
        
        // Renderizar gráficos
        console.log('Renderizando gráficos...');
        renderAllCharts(reportesData);
        
    } catch (error) {
        console.error('Error al cargar reportes:', error);
        showErrorAlert('Error al cargar los reportes. Por favor, intente nuevamente. Error: ' + error.message);
    }
}

async function processResponses(resumenResp, clientesResp, itemsResp, pagosResp, reservasResp, serviciosResp) {
    // Procesar resumen ejecutivo
    if (resumenResp.ok) {
        reportesData.resumenEjecutivo = await resumenResp.json();
        console.log('Resumen ejecutivo cargado:', reportesData.resumenEjecutivo);
    }
    
    // Procesar clientes
    if (clientesResp.ok) {
        reportesData.clientes = await clientesResp.json();
        console.log('Clientes cargado:', reportesData.clientes);
    }
    
    // Procesar items
    if (itemsResp.ok) {
        reportesData.items = await itemsResp.json();
        console.log('Items cargado:', reportesData.items);
    }
    
    // Procesar pagos con manejo especial de errores
    if (pagosResp.ok) {
        const pagosText = await pagosResp.text();
        console.log('Respuesta de pagos (texto raw):', pagosText);
        
        try {
            if (pagosText && pagosText.trim() !== '' && pagosText.trim() !== 'null') {
                reportesData.pagos = JSON.parse(pagosText);
                console.log('Pagos cargado exitosamente:', reportesData.pagos);
            } else {
                console.warn('Respuesta de pagos está vacía o es null');
                reportesData.pagos = null;
            }
        } catch (parseError) {
            console.error('Error al parsear respuesta de pagos:', parseError);
            console.error('Contenido que falló al parsear:', pagosText);
            reportesData.pagos = null;
        }
    } else {
        console.error('Error en endpoint de pagos:', {
            status: pagosResp.status,
            statusText: pagosResp.statusText,
            url: pagosResp.url
        });
        
        try {
            const errorText = await pagosResp.text();
            console.error('Respuesta de error de pagos:', errorText);
        } catch (e) {
            console.error('No se pudo leer el error de pagos:', e);
        }
        
        reportesData.pagos = null;
    }
    
    // Procesar reservas
    if (reservasResp.ok) {
        reportesData.reservas = await reservasResp.json();
        console.log('Reservas cargado (con finalizadas):', reportesData.reservas);
        
        // *** NUEVO: Log específico para verificar estados ***
        if (reportesData.reservas?.tasaConversionEstado) {
            const estados = reportesData.reservas.tasaConversionEstado;
            console.log('Estados de reservas actualizados:', {
                pendientes: estados.reservasPendientes,
                confirmadas: estados.reservasConfirmadas,
                canceladas: estados.reservasCanceladas,
                finalizadas: estados.reservasFinalizadas || 0,
                tasaFinalizacion: estados.tasaFinalizacion || 0
            });
        }
    }
    
    // Procesar servicios
    if (serviciosResp.ok) {
        reportesData.servicios = await serviciosResp.json();
        console.log('Servicios cargado:', reportesData.servicios);
    }
}

function updateSummaryCards() {
    console.log('Actualizando tarjetas de resumen con:', reportesData.resumenEjecutivo);
    
    if (reportesData.resumenEjecutivo) {
        const resumen = reportesData.resumenEjecutivo;
        
        const totalRevenueEl = document.getElementById('totalRevenue');
        const totalEventsEl = document.getElementById('totalEvents');
        const activeClientsEl = document.getElementById('activeClients');
        const conversionRateEl = document.getElementById('conversionRate');
        
        if (totalRevenueEl) {
            totalRevenueEl.textContent = formatCurrency(resumen.ingresosTotales);
        }
        
        if (totalEventsEl) {
            totalEventsEl.textContent = resumen.totalReservas.toString();
        }
        
        if (activeClientsEl) {
            activeClientsEl.textContent = resumen.totalClientes.toString();
        }
        
        if (conversionRateEl) {
            conversionRateEl.textContent = `${resumen.tasaConversionReservas.toFixed(1)}%`;
        }
        
        console.log('Tarjetas actualizadas correctamente');
    } else {
        console.warn('No hay datos de resumen ejecutivo para actualizar las tarjetas');
        
        // Mostrar valores por defecto cuando no hay datos
        const elements = {
            'totalRevenue': 'S/0.00',
            'totalEvents': '0',
            'activeClients': '0',
            'conversionRate': '0%'
        };
        
        Object.entries(elements).forEach(([id, value]) => {
            const element = document.getElementById(id);
            if (element) {
                element.textContent = value;
            }
        });
    }
}