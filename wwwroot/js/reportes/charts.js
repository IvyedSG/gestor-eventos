// Manejo de todos los gráficos de reportes

import { hideChartLoading, showNoDataMessage } from './utils.js';

// Almacenar instancias de gráficos
export let charts = {
    revenue: null,
    reservationStatus: null,
    popularServices: null,
    clientGrowth: null,
    topItems: null,
    eventTypeProfit: null,
    reservationTrend: null // Nuevo
};

export function initializeCharts() {
    console.log('Charts module initialized successfully');
}

export function renderAllCharts(reportesData) {
    console.log('Iniciando renderizado de todos los gráficos (actualizado)');
    try {
        renderRevenueChart(reportesData.pagos);
        renderReservationStatusChart(reportesData.reservas); // Actualizado
        renderPopularServicesChart(reportesData.servicios);
        renderClientGrowthChart(reportesData.clientes);
        renderTopItemsChart(reportesData.items);
        renderEventTypeProfitChart(reportesData.reservas);
        renderReservationTrendChart(reportesData.reservas); // Nuevo - opcional
        console.log('Todos los gráficos renderizados (incluidas reservas finalizadas)');
    } catch (error) {
        console.error('Error al renderizar gráficos:', error);
    }
}

function renderRevenueChart(pagosData) {
    console.log('Renderizando gráfico de ingresos...');
    const canvas = document.getElementById('revenueChart');
    if (!canvas) {
        console.error('Canvas revenueChart no encontrado');
        return;
    }
    
    if (!pagosData) {
        console.warn('Datos de pagos no disponibles para gráfico de ingresos');
        hideChartLoading('revenueChart');
        showNoDataMessage('revenueChart', 'No hay datos de pagos disponibles');
        return;
    }

    // Destroy existing chart
    if (charts.revenue) {
        charts.revenue.destroy();
    }

    const tendenciaData = pagosData.tendenciaMensualIngresos || [];
    
    console.log('Datos de tendencia mensual:', tendenciaData);
    
    if (tendenciaData.length === 0) {
        console.warn('No hay datos de tendencia mensual');
        hideChartLoading('revenueChart');
        showNoDataMessage('revenueChart', 'No hay ingresos registrados en el período seleccionado');
        return;
    }
    
    // Remover el event listener del canvas si existía
    canvas.onclick = null;
    
    const labels = tendenciaData.map(item => item.nombreMes);
    const data = tendenciaData.map(item => item.montoTotal);

    charts.revenue = new Chart(canvas, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Ingresos Mensuales',
                data: data,
                borderColor: 'rgb(79, 70, 229)',
                backgroundColor: 'rgba(79, 70, 229, 0.1)',
                tension: 0.4,
                fill: true
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function(value) {
                            return 'S/' + value.toLocaleString('es-PE');
                        }
                    }
                }
            },
            plugins: {
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            return context.dataset.label + ': S/' + context.parsed.y.toLocaleString('es-PE', {minimumFractionDigits: 2});
                        }
                    }
                }
            }
        }
    });
    
    setTimeout(() => {
        hideChartLoading('revenueChart');
        console.log('Gráfico de ingresos renderizado exitosamente');
    }, 100);
}

function renderReservationStatusChart(reservasData) {
    console.log('Renderizando gráfico de estados de reservas...');
    const canvas = document.getElementById('reservationStatusChart');
    if (!canvas) {
        console.error('Canvas reservationStatusChart no encontrado');
        return;
    }
    
    if (!reservasData || !reservasData.tasaConversionEstado) {
        console.warn('Datos de reservas no disponibles');
        hideChartLoading('reservationStatusChart');
        showNoDataMessage('reservationStatusChart', 'No hay datos de reservas disponibles');
        return;
    }

    if (charts.reservationStatus) {
        charts.reservationStatus.destroy();
    }

    const tasaConversion = reservasData.tasaConversionEstado;
    console.log('Datos de estados (actualizado):', tasaConversion);
    
    // *** ACTUALIZADO: Incluir reservas finalizadas ***
    const totalReservas = tasaConversion.reservasPendientes + 
                         tasaConversion.reservasConfirmadas + 
                         tasaConversion.reservasCanceladas + 
                         (tasaConversion.reservasFinalizadas || 0); // Nuevo campo
    
    if (totalReservas === 0) {
        console.warn('No hay reservas para mostrar');
        hideChartLoading('reservationStatusChart');
        showNoDataMessage('reservationStatusChart', 'No hay reservas registradas en el período seleccionado');
        return;
    }
    
    // *** ACTUALIZADO: Agregar datos de reservas finalizadas ***
    const data = [
        tasaConversion.reservasPendientes,
        tasaConversion.reservasConfirmadas,
        tasaConversion.reservasCanceladas,
        tasaConversion.reservasFinalizadas || 0 // Nuevo
    ];

    const labels = ['Pendientes', 'Confirmadas', 'Canceladas', 'Finalizadas']; // Actualizado

    charts.reservationStatus = new Chart(canvas, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: [
                    '#F59E0B', // Pendientes - Amarillo
                    '#10B981', // Confirmadas - Verde
                    '#EF4444', // Canceladas - Rojo
                    '#6366F1'  // Finalizadas - Azul/Púrpura
                ]
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom'
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const label = context.label || '';
                            const value = context.parsed;
                            const percentage = ((value / totalReservas) * 100).toFixed(1);
                            return `${label}: ${value} (${percentage}%)`;
                        }
                    }
                }
            }
        }
    });
    
    setTimeout(() => {
        hideChartLoading('reservationStatusChart');
        console.log('Gráfico de estados renderizado exitosamente con reservas finalizadas');
    }, 100);
}

// *** NUEVA FUNCIÓN: Gráfico de reservas por mes ***
function renderReservationTrendChart(reservasData) {
    console.log('Renderizando gráfico de tendencia de reservas...');
    const canvas = document.getElementById('reservationTrendChart');
    if (!canvas) {
        console.warn('Canvas reservationTrendChart no encontrado - probablemente opcional');
        return;
    }
    
    if (!reservasData || !reservasData.reservasPorMes) {
        console.warn('Datos de reservas por mes no disponibles');
        hideChartLoading('reservationTrendChart');
        showNoDataMessage('reservationTrendChart', 'No hay datos de reservas mensuales disponibles');
        return;
    }

    if (charts.reservationTrend) {
        charts.reservationTrend.destroy();
    }

    const reservasPorMes = reservasData.reservasPorMes || [];
    console.log('Datos de reservas por mes:', reservasPorMes);
    
    if (reservasPorMes.length === 0) {
        console.warn('No hay datos de reservas mensuales');
        hideChartLoading('reservationTrendChart');
        showNoDataMessage('reservationTrendChart', 'No hay reservas mensuales en el período seleccionado');
        return;
    }
    
    const labels = reservasPorMes.map(item => item.nombreMes);
    const cantidadData = reservasPorMes.map(item => item.cantidadReservas);
    const montoData = reservasPorMes.map(item => item.montoTotal);

    charts.reservationTrend = new Chart(canvas, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Cantidad de Reservas',
                    data: cantidadData,
                    borderColor: 'rgb(99, 102, 241)',
                    backgroundColor: 'rgba(99, 102, 241, 0.1)',
                    tension: 0.4,
                    yAxisID: 'y'
                },
                {
                    label: 'Monto Total (S/)',
                    data: montoData,
                    borderColor: 'rgb(16, 185, 129)',
                    backgroundColor: 'rgba(16, 185, 129, 0.1)',
                    tension: 0.4,
                    yAxisID: 'y1'
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: {
                mode: 'index',
                intersect: false,
            },
            scales: {
                y: {
                    type: 'linear',
                    display: true,
                    position: 'left',
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Cantidad'
                    }
                },
                y1: {
                    type: 'linear',
                    display: true,
                    position: 'right',
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Monto (S/)'
                    },
                    grid: {
                        drawOnChartArea: false,
                    },
                    ticks: {
                        callback: function(value) {
                            return 'S/' + value.toLocaleString();
                        }
                    }
                }
            },
            plugins: {
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            if (context.datasetIndex === 1) {
                                return `${context.dataset.label}: S/${context.parsed.y.toLocaleString()}`;
                            }
                            return `${context.dataset.label}: ${context.parsed.y}`;
                        }
                    }
                }
            }
        }
    });
    
    setTimeout(() => {
        hideChartLoading('reservationTrendChart');
        console.log('Gráfico de tendencia de reservas renderizado exitosamente');
    }, 100);
}

function renderPopularServicesChart(serviciosData) {
    console.log('Renderizando gráfico de servicios populares...');
    const canvas = document.getElementById('popularServicesChart');
    if (!canvas) {
        console.error('Canvas popularServicesChart no encontrado');
        return;
    }
    
    if (!serviciosData) {
        console.warn('Datos de servicios no disponibles');
        hideChartLoading('popularServicesChart');
        showNoDataMessage('popularServicesChart', 'No hay datos de servicios disponibles');
        return;
    }

    if (charts.popularServices) {
        charts.popularServices.destroy();
    }

    const serviciosDataList = serviciosData.serviciosMasFrecuentes || [];
    console.log('Datos de servicios:', serviciosDataList);
    
    if (serviciosDataList.length === 0) {
        console.warn('No hay servicios para mostrar');
        hideChartLoading('popularServicesChart');
        showNoDataMessage('popularServicesChart', 'No hay servicios con reservas en el período seleccionado');
        return;
    }
    
    const labels = serviciosDataList.map(item => item.nombreServicio);
    const data = serviciosDataList.map(item => item.cantidadReservas);

    charts.popularServices = new Chart(canvas, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Reservas',
                data: data,
                backgroundColor: 'rgba(79, 70, 229, 0.8)'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
    
    setTimeout(() => {
        hideChartLoading('popularServicesChart');
        console.log('Gráfico de servicios renderizado exitosamente');
    }, 100);
}

function renderClientGrowthChart(clientesData) {
    console.log('Renderizando gráfico de crecimiento de clientes...');
    const canvas = document.getElementById('clientGrowthChart');
    if (!canvas) {
        console.error('Canvas clientGrowthChart no encontrado');
        return;
    }
    
    if (!clientesData) {
        console.warn('Datos de clientes no disponibles');
        hideChartLoading('clientGrowthChart');
        showNoDataMessage('clientGrowthChart', 'No hay datos de clientes disponibles');
        return;
    }

    if (charts.clientGrowth) {
        charts.clientGrowth.destroy();
    }

    const clientesDataList = clientesData.clientesNuevosPorMes || [];
    console.log('Datos de clientes nuevos:', clientesDataList);
    
    if (clientesDataList.length === 0) {
        console.warn('No hay datos de crecimiento de clientes');
        hideChartLoading('clientGrowthChart');
        showNoDataMessage('clientGrowthChart', 'No hay registros de clientes nuevos en el período seleccionado');
        return;
    }
    
    const labels = clientesDataList.map(item => item.nombreMes);
    const data = clientesDataList.map(item => item.cantidadClientesNuevos);

    charts.clientGrowth = new Chart(canvas, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Clientes Nuevos',
                data: data,
                borderColor: 'rgb(16, 185, 129)',
                backgroundColor: 'rgba(16, 185, 129, 0.1)',
                tension: 0.4,
                fill: true
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
    
    setTimeout(() => {
        hideChartLoading('clientGrowthChart');
        console.log('Gráfico de clientes renderizado exitosamente');
    }, 100);
}

function renderTopItemsChart(itemsData) {
    console.log('Renderizando gráfico de items más utilizados...');
    const canvas = document.getElementById('topItemsChart');
    if (!canvas) {
        console.error('Canvas topItemsChart no encontrado');
        return;
    }
    
    if (!itemsData) {
        console.warn('Datos de items no disponibles');
        hideChartLoading('topItemsChart');
        showNoDataMessage('topItemsChart', 'No hay datos de inventario disponibles');
        return;
    }

    if (charts.topItems) {
        charts.topItems.destroy();
    }

    const itemsDataList = itemsData.itemsMasUtilizados?.slice(0, 5) || [];
    console.log('Datos de items más utilizados:', itemsDataList);
    
    if (itemsDataList.length === 0) {
        console.warn('No hay items utilizados para mostrar');
        hideChartLoading('topItemsChart');
        showNoDataMessage('topItemsChart', 'No hay items utilizados en el período seleccionado');
        return;
    }
    
    const labels = itemsDataList.map(item => item.nombreItem);
    const data = itemsDataList.map(item => item.totalCantidadUtilizada);

    charts.topItems = new Chart(canvas, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Cantidad Utilizada',
                data: data,
                backgroundColor: 'rgba(245, 158, 11, 0.8)'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            indexAxis: 'y',
            scales: {
                x: {
                    beginAtZero: true
                }
            }
        }
    });
    
    setTimeout(() => {
        hideChartLoading('topItemsChart');
        console.log('Gráfico de items renderizado exitosamente');
    }, 100);
}

function renderEventTypeProfitChart(reservasData) {
    console.log('Renderizando gráfico de ingresos por tipo de evento...');
    const canvas = document.getElementById('eventTypeProfitChart');
    if (!canvas) {
        console.error('Canvas eventTypeProfitChart no encontrado');
        return;
    }
    
    if (!reservasData) {
        console.warn('Datos de reservas no disponibles para tipo de evento');
        hideChartLoading('eventTypeProfitChart');
        showNoDataMessage('eventTypeProfitChart', 'No hay datos de reservas disponibles');
        return;
    }

    if (charts.eventTypeProfit) {
        charts.eventTypeProfit.destroy();
    }

    const eventosData = reservasData.ingresosPromedioPorTipoEvento || [];
    console.log('Datos de ingresos por tipo de evento:', eventosData);
    
    if (eventosData.length === 0) {
        console.warn('No hay datos de ingresos por tipo de evento');
        hideChartLoading('eventTypeProfitChart');
        showNoDataMessage('eventTypeProfitChart', 'No hay tipos de evento con ingresos en el período seleccionado');
        return;
    }
    
    const labels = eventosData.map(item => item.tipoEvento);
    const data = eventosData.map(item => item.ingresoTotal);

    charts.eventTypeProfit = new Chart(canvas, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Ingresos Totales',
                data: data,
                backgroundColor: 'rgba(139, 69, 19, 0.8)'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function(value) {
                            return 'S/' + value.toLocaleString();
                        }
                    }
                }
            },
            plugins: {
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            return 'Ingresos: S/' + context.parsed.y.toLocaleString();
                        }
                    }
                }
            }
        }
    });
    
    setTimeout(() => {
        hideChartLoading('eventTypeProfitChart');
        console.log('Gráfico de tipo de evento renderizado exitosamente');
    }, 100);
}