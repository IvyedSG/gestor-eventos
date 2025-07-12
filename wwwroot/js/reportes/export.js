// Funcionalidad de exportación a Excel

import { fetchWithToken, setButtonLoading, restoreButton } from './utils.js';
import { getFilterDates } from './filters.js';
import { showNotification } from './alerts.js';

export function initializeExport() {
    setupExportButton();
    console.log('Export functionality initialized successfully');
}

function setupExportButton() {
    const exportBtn = document.getElementById('exportToExcel');
    if (exportBtn) {
        exportBtn.addEventListener('click', handleExportToExcel);
    }
}

async function handleExportToExcel() {
    const exportBtn = document.getElementById('exportToExcel');
    const originalHtml = setButtonLoading(exportBtn, 'Generando...');
    
    try {
        // Obtener fechas del filtro
        const { fechaInicioRaw, fechaFinRaw } = getFilterDates();

        // Construir URL con parámetros
        let url = '/Reportes?handler=ExportToExcel';
        const params = new URLSearchParams();
        
        if (fechaInicioRaw) {
            params.append('fechaInicio', fechaInicioRaw);
        }
        if (fechaFinRaw) {
            params.append('fechaFin', fechaFinRaw);
        }

        if (params.toString()) {
            url += '&' + params.toString();
        }

        console.log('Exportando a:', url);

        // Realizar la descarga
        const response = await fetchWithToken(url);
        
        if (!response.ok) {
            throw new Error('Error al generar el archivo Excel');
        }

        // Obtener el blob y crear la descarga
        const blob = await response.blob();
        const contentDisposition = response.headers.get('Content-Disposition');
        
        let filename = 'Reporte_Gestor_Eventos.xlsx';
        if (contentDisposition) {
            const filenameMatch = contentDisposition.match(/filename="([^"]+)"/);
            if (filenameMatch) {
                filename = filenameMatch[1];
            }
        }

        // Crear elemento de descarga temporal
        const url2 = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url2;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url2);
        document.body.removeChild(a);

        console.log('Archivo Excel descargado exitosamente:', filename);
        
        // Mostrar mensaje de éxito
        showNotification('Archivo Excel generado y descargado exitosamente', 'success');

    } catch (error) {
        console.error('Error al exportar a Excel:', error);
        showNotification('Error al generar el archivo Excel: ' + error.message, 'error');
    } finally {
        // Restaurar botón
        restoreButton(exportBtn, originalHtml);
    }
}