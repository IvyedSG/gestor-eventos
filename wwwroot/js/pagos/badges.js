// Manejo de badges de tipos de pago

export function initializeBadges() {
    convertirTiposPagoABadges();
}

function convertirTiposPagoABadges() {
    // Mapeo de valores a nombres formateados
    const nombresTiposPago = {
        'efectivo': 'Efectivo',
        'yape': 'Yape',
        'plin': 'Plin',
        'transferencia': 'Transferencia bancaria',
        'adelanto': 'Adelanto',
        'parcial': 'Pago parcial',
        'otro': 'Otro'
    };
    
    const tiposPago = document.querySelectorAll('.tipo-pago-badge');
    
    tiposPago.forEach(elemento => {
        const tipo = elemento.getAttribute('data-tipo');
        let badgeClass = 'badge-otro';
        let icon = 'bi-question-circle';
        
        switch(tipo) {
            case 'efectivo':
                badgeClass = 'badge-efectivo';
                icon = 'bi-cash';
                break;
            case 'yape':
                badgeClass = 'badge-yape';
                icon = 'bi-phone';
                break;
            case 'plin':
                badgeClass = 'badge-plin';
                icon = 'bi-phone-fill';
                break;
            case 'transferencia':
                badgeClass = 'badge-transferencia';
                icon = 'bi-bank';
                break;
            case 'adelanto':
                badgeClass = 'badge-adelanto';
                icon = 'bi-credit-card-2-front';
                break;
            case 'parcial':
                badgeClass = 'badge-parcial';
                icon = 'bi-credit-card';
                break;
        }
        
        // Usar el nombre formateado en lugar del texto original
        const nombreFormateado = nombresTiposPago[tipo] || elemento.textContent;
        elemento.className = `tipo-badge ${badgeClass}`;
        elemento.innerHTML = `<i class="bi ${icon}"></i>${nombreFormateado}`;
    });
}