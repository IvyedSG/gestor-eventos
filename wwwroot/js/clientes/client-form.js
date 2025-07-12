// Manejo de formularios de cliente (crear/editar)

import { showAlert, insertAlert, getAntiForgeryToken, setButtonLoading, restoreButton } from './utils.js';

export function initializeClientForm() {
    setupClientTypeToggle();
    setupEditClientTypeToggle();
    setupNewClientForm();
    setupEditClientForm();
    setupFormValidation();
    setupInputRestrictions(); // *** AGREGAR ESTA LÍNEA ***
    console.log('Client form initialized successfully');
}

// *** NUEVA FUNCIÓN: Configurar validaciones en tiempo real ***
function setupFormValidation() {
    // Validaciones para formulario nuevo
    setupFieldValidation('clientEmail', 'email');
    setupFieldValidation('clientPhone', 'phone');
    setupFieldValidation('clientRuc', 'ruc');
    
    // Validaciones para formulario de edición
    setupFieldValidation('editClientEmail', 'email');
    setupFieldValidation('editClientPhone', 'phone');
    setupFieldValidation('editClientRuc', 'ruc');
}

// *** NUEVA FUNCIÓN: Configurar validación por campo ***
function setupFieldValidation(fieldId, validationType) {
    const field = document.getElementById(fieldId);
    if (!field) return;
    
    field.addEventListener('input', function() {
        validateField(this, validationType);
    });
    
    field.addEventListener('blur', function() {
        validateField(this, validationType);
    });
}

// *** NUEVA FUNCIÓN: Validar campo individual ***
function validateField(field, validationType) {
    const value = field.value.trim();
    let isValid = true;
    let errorMessage = '';
    
    // Limpiar estilos previos
    field.classList.remove('is-valid', 'is-invalid');
    
    // Obtener o crear div de error
    let errorDiv = field.parentNode.querySelector('.invalid-feedback');
    if (!errorDiv) {
        errorDiv = document.createElement('div');
        errorDiv.className = 'invalid-feedback';
        field.parentNode.appendChild(errorDiv);
    }
    
    switch (validationType) {
        case 'email':
            if (value && !isValidEmail(value)) {
                isValid = false;
                errorMessage = 'Por favor ingresa un correo electrónico válido';
            }
            break;
            
        case 'phone':
            if (value && !isValidPhone(value)) {
                isValid = false;
                errorMessage = 'El teléfono debe contener solo números (9 dígitos)';
            }
            break;
            
        case 'ruc':
            if (value && !isValidRuc(value)) {
                isValid = false;
                errorMessage = 'El RUC debe contener solo números (11 dígitos)';
            }
            break;
    }
    
    // Aplicar estilos de validación
    if (value) { // Solo validar si hay contenido
        if (isValid) {
            field.classList.add('is-valid');
            errorDiv.textContent = '';
        } else {
            field.classList.add('is-invalid');
            errorDiv.textContent = errorMessage;
        }
    } else {
        // Si está vacío, remover cualquier validación visual
        errorDiv.textContent = '';
    }
    
    return isValid;
}

// *** FUNCIONES DE VALIDACIÓN ***
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

function isValidPhone(phone) {
    // Solo números, exactamente 9 dígitos para Perú
    const phoneRegex = /^[0-9]{9}$/;
    return phoneRegex.test(phone);
}

function isValidRuc(ruc) {
    // Solo números, exactamente 11 dígitos para RUC en Perú
    const rucRegex = /^[0-9]{11}$/;
    return rucRegex.test(ruc);
}

// *** NUEVA FUNCIÓN: Validar formulario completo ***
function validateForm(formType = 'new') {
    let isValid = true;
    const prefix = formType === 'new' ? 'client' : 'editClient';
    
    // Validar campos obligatorios
    const requiredFields = [
        { id: `${prefix}Name`, name: 'nombre' },
        { id: `${prefix}Email`, name: 'correo electrónico' },
        { id: `${prefix}Phone`, name: 'teléfono' }
    ];
    
    requiredFields.forEach(field => {
        const element = document.getElementById(field.id);
        if (element && !element.value.trim()) {
            element.classList.add('is-invalid');
            
            let errorDiv = element.parentNode.querySelector('.invalid-feedback');
            if (!errorDiv) {
                errorDiv = document.createElement('div');
                errorDiv.className = 'invalid-feedback';
                element.parentNode.appendChild(errorDiv);
            }
            errorDiv.textContent = `El campo ${field.name} es obligatorio`;
            isValid = false;
        }
    });
    
    // Validar tipo de cliente
    const clientTypeRadios = document.querySelectorAll(`input[name="${formType === 'new' ? 'clientType' : 'editClientType'}"]`);
    const isClientTypeSelected = Array.from(clientTypeRadios).some(radio => radio.checked);
    
    if (!isClientTypeSelected) {
        showAlert('warning', 'Por favor selecciona el tipo de cliente (Individual o Empresa)');
        isValid = false;
    }
    
    // Validar formato de campos específicos
    const emailField = document.getElementById(`${prefix}Email`);
    const phoneField = document.getElementById(`${prefix}Phone`);
    const rucField = document.getElementById(`${prefix}Ruc`);
    
    if (emailField && emailField.value.trim() && !validateField(emailField, 'email')) {
        isValid = false;
    }
    
    if (phoneField && phoneField.value.trim() && !validateField(phoneField, 'phone')) {
        isValid = false;
    }
    
    if (rucField && rucField.value.trim() && !validateField(rucField, 'ruc')) {
        isValid = false;
    }
    
    // Validar campos específicos de empresa
    const isCompany = formType === 'new' ? 
        document.getElementById('companyClient')?.checked : 
        document.getElementById('editCompanyClient')?.checked;
    
    if (isCompany) {
        const rucField = document.getElementById(`${prefix}Ruc`);
        const razonSocialField = document.getElementById(`${prefix}RazonSocial`);
        
        if (rucField && !rucField.value.trim()) {
            rucField.classList.add('is-invalid');
            let errorDiv = rucField.parentNode.querySelector('.invalid-feedback');
            if (!errorDiv) {
                errorDiv = document.createElement('div');
                errorDiv.className = 'invalid-feedback';
                rucField.parentNode.appendChild(errorDiv);
            }
            errorDiv.textContent = 'El RUC es obligatorio para empresas';
            isValid = false;
        }
        
        if (razonSocialField && !razonSocialField.value.trim()) {
            razonSocialField.classList.add('is-invalid');
            let errorDiv = razonSocialField.parentNode.querySelector('.invalid-feedback');
            if (!errorDiv) {
                errorDiv = document.createElement('div');
                errorDiv.className = 'invalid-feedback';
                razonSocialField.parentNode.appendChild(errorDiv);
            }
            errorDiv.textContent = 'La razón social es obligatoria para empresas';
            isValid = false;
        }
    }
    
    return isValid;
}

function setupClientTypeToggle() {
    const individualClient = document.getElementById('individualClient');
    const companyClient = document.getElementById('companyClient');
    const companyFields = document.querySelectorAll('.company-field');
    const nameLabel = document.getElementById('nameLabel');

    if (individualClient) {
        individualClient.addEventListener('change', function() {
            if (nameLabel) nameLabel.textContent = 'Nombre completo';
            companyFields.forEach(field => field.style.display = 'none');
            
            // Limpiar validaciones de campos de empresa
            const rucField = document.getElementById('clientRuc');
            const razonSocialField = document.getElementById('clientRazonSocial');
            if (rucField) {
                rucField.value = '';
                rucField.classList.remove('is-invalid', 'is-valid');
            }
            if (razonSocialField) {
                razonSocialField.value = '';
                razonSocialField.classList.remove('is-invalid', 'is-valid');
            }
        });
    }
    
    if (companyClient) {
        companyClient.addEventListener('change', function() {
            if (nameLabel) nameLabel.textContent = 'Nombre de la empresa';
            companyFields.forEach(field => field.style.display = 'block');
        });
    }
}

function setupEditClientTypeToggle() {
    const editIndividualClient = document.getElementById('editIndividualClient');
    const editCompanyClient = document.getElementById('editCompanyClient');
    const editCompanyFields = document.querySelectorAll('.edit-company-field');
    const editNameLabel = document.getElementById('editNameLabel');

    if (editIndividualClient) {
        editIndividualClient.addEventListener('change', function() {
            if (editNameLabel) editNameLabel.textContent = 'Nombre completo';
            editCompanyFields.forEach(field => field.style.display = 'none');
            
            // Limpiar validaciones de campos de empresa
            const rucField = document.getElementById('editClientRuc');
            const razonSocialField = document.getElementById('editClientRazonSocial');
            if (rucField) {
                rucField.value = '';
                rucField.classList.remove('is-invalid', 'is-valid');
            }
            if (razonSocialField) {
                razonSocialField.value = '';
                razonSocialField.classList.remove('is-invalid', 'is-valid');
            }
        });
    }
    
    if (editCompanyClient) {
        editCompanyClient.addEventListener('change', function() {
            if (editNameLabel) editNameLabel.textContent = 'Nombre de la empresa';
            editCompanyFields.forEach(field => field.style.display = 'block');
        });
    }
}

function setupNewClientForm() {
    const saveNewClientBtn = document.getElementById('saveNewClient');
    if (saveNewClientBtn) {
        saveNewClientBtn.addEventListener('click', handleCreateClient);
    }
}

function setupEditClientForm() {
    const saveEditClientBtn = document.getElementById('saveEditClient');
    if (saveEditClientBtn) {
        saveEditClientBtn.addEventListener('click', handleUpdateClient);
    }
}

async function handleCreateClient() {
    const button = document.getElementById('saveNewClient');
    
    // *** VALIDAR FORMULARIO ANTES DE ENVIAR ***
    if (!validateForm('new')) {
        showAlert('warning', 'Por favor corrige los errores en el formulario');
        return;
    }
    
    const originalHtml = setButtonLoading(button, 'Guardando...');
    
    try {
        const clientData = collectNewClientData();
        if (!clientData) return;
        
        const token = getAntiForgeryToken();
        
        const response = await fetch('/Clientes?handler=CreateCliente', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(clientData)
        });
        
        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || `Error al crear cliente: ${response.statusText}`);
        }
        
        const result = await response.json();
        
        if (result.success) {
            // Cerrar modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('newClientModal'));
            modal.hide();
            
            // Limpiar formulario
            clearNewClientForm();
            
            // Mostrar mensaje de éxito
            insertAlert('Cliente creado exitosamente', 'success');
            
            // Recargar página
            setTimeout(() => {
                window.location.reload();
            }, 1500);
        } else {
            throw new Error(result.message || 'No se pudo crear el cliente');
        }
    } catch (error) {
        console.error('Error:', error);
        showAlert('danger', error.message || 'Ha ocurrido un error al procesar la solicitud');
    } finally {
        restoreButton(button, originalHtml);
    }
}

async function handleUpdateClient() {
    const button = document.getElementById('saveEditClient');
    
    // *** VALIDAR FORMULARIO ANTES DE ENVIAR ***
    if (!validateForm('edit')) {
        showAlert('warning', 'Por favor corrige los errores en el formulario');
        return;
    }
    
    const originalHtml = setButtonLoading(button, 'Guardando...');
    
    try {
        const clientData = collectEditClientData();
        if (!clientData) return;
        
        const token = getAntiForgeryToken();
        
        const response = await fetch('/Clientes?handler=UpdateCliente', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(clientData)
        });
        
        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || `Error al actualizar cliente: ${response.statusText}`);
        }
        
        const result = await response.json();
        
        if (result.success) {
            // Cerrar modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('editClientModal'));
            modal.hide();
            
            // Mostrar mensaje de éxito
            insertAlert('Cliente actualizado exitosamente', 'success');
            
            // Recargar página
            setTimeout(() => {
                window.location.reload();
            }, 1500);
        } else {
            throw new Error(result.message || 'No se pudo actualizar el cliente');
        }
    } catch (error) {
        console.error('Error:', error);
        showAlert('danger', error.message || 'Ha ocurrido un error al procesar la solicitud');
    } finally {
        restoreButton(button, originalHtml);
    }
}

function collectNewClientData() {
    const clientType = document.querySelector('input[name="clientType"]:checked')?.value;
    const clientName = document.getElementById('clientName')?.value;
    const clientEmail = document.getElementById('clientEmail')?.value;
    const clientPhone = document.getElementById('clientPhone')?.value;
    const clientAddress = document.getElementById('clientAddress')?.value;
    
    return {
        tipoCliente: clientType === 'Individual' ? 'INDIVIDUAL' : 'EMPRESA',
        nombre: clientName,
        correoElectronico: clientEmail,
        telefono: clientPhone,
        direccion: clientAddress || '',
        ruc: clientType === 'Empresa' ? document.getElementById('clientRuc')?.value || '' : '',
        razonSocial: clientType === 'Empresa' ? document.getElementById('clientRazonSocial')?.value || '' : ''
    };
}

function collectEditClientData() {
    const clientId = document.getElementById('editClientId').value;
    const clientType = document.querySelector('input[name="editClientType"]:checked')?.value;
    const clientName = document.getElementById('editClientName')?.value;
    const clientEmail = document.getElementById('editClientEmail')?.value;
    const clientPhone = document.getElementById('editClientPhone')?.value;
    const clientAddress = document.getElementById('editClientAddress')?.value;
    
    return {
        id: clientId,
        tipoCliente: clientType === 'Individual' ? 'INDIVIDUAL' : 'EMPRESA',
        nombre: clientName,
        correoElectronico: clientEmail,
        telefono: clientPhone,
        direccion: clientAddress || '',
        ruc: clientType === 'Empresa' ? document.getElementById('editClientRuc')?.value || '' : '',
        razonSocial: clientType === 'Empresa' ? document.getElementById('editClientRazonSocial')?.value || '' : ''
    };
}

// *** NUEVA FUNCIÓN: Limpiar formulario ***
function clearNewClientForm() {
    // Limpiar campos
    const fields = ['clientName', 'clientEmail', 'clientPhone', 'clientAddress', 'clientRuc', 'clientRazonSocial'];
    fields.forEach(fieldId => {
        const field = document.getElementById(fieldId);
        if (field) {
            field.value = '';
            field.classList.remove('is-valid', 'is-invalid');
        }
    });
    
    // Resetear tipo de cliente
    const individualRadio = document.getElementById('individualClient');
    if (individualRadio) {
        individualRadio.checked = true;
        individualRadio.dispatchEvent(new Event('change'));
    }
    
    // Ocultar campos de empresa
    const companyFields = document.querySelectorAll('.company-field');
    companyFields.forEach(field => field.style.display = 'none');
}

// *** FUNCIÓN PARA RESTRINGIR ENTRADA DE CARACTERES ***
function setupInputRestrictions() {
    // Restringir teléfono a solo números
    const phoneInputs = ['clientPhone', 'editClientPhone'];
    phoneInputs.forEach(inputId => {
        const input = document.getElementById(inputId);
        if (input) {
            input.addEventListener('input', function(e) {
                // Remover cualquier carácter que no sea número
                this.value = this.value.replace(/[^0-9]/g, '');
                
                // Limitar a 9 dígitos
                if (this.value.length > 9) {
                    this.value = this.value.slice(0, 9);
                }
            });
            
            input.addEventListener('keypress', function(e) {
                // Prevenir entrada de caracteres no numéricos
                if (!/[0-9]/.test(e.key) && !['Backspace', 'Delete', 'Tab', 'Enter', 'ArrowLeft', 'ArrowRight'].includes(e.key)) {
                    e.preventDefault();
                }
            });
        }
    });
    
    // Restringir RUC a solo números
    const rucInputs = ['clientRuc', 'editClientRuc'];
    rucInputs.forEach(inputId => {
        const input = document.getElementById(inputId);
        if (input) {
            input.addEventListener('input', function(e) {
                // Remover cualquier carácter que no sea número
                this.value = this.value.replace(/[^0-9]/g, '');
                
                // Limitar a 11 dígitos
                if (this.value.length > 11) {
                    this.value = this.value.slice(0, 11);
                }
            });
            
            input.addEventListener('keypress', function(e) {
                // Prevenir entrada de caracteres no numéricos
                if (!/[0-9]/.test(e.key) && !['Backspace', 'Delete', 'Tab', 'Enter', 'ArrowLeft', 'ArrowRight'].includes(e.key)) {
                    e.preventDefault();
                }
            });
        }
    });
}