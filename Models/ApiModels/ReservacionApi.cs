using System;
using System.Collections.Generic;

namespace gestor_eventos.Models.ApiModels
{
    public class ReservacionApi
    {
        public string Id { get; set; } = string.Empty;
        public string NombreEvento { get; set; } = string.Empty;
        public DateTime FechaEjecucion { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public decimal PrecioTotal { get; set; }
        public string ClienteId { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public string CorreoCliente { get; set; } = string.Empty;
        public string TelefonoCliente { get; set; } = string.Empty;
        public string TipoEventoId { get; set; } = string.Empty;
        public string TipoEventoNombre { get; set; } = string.Empty;
        public string ServicioId { get; set; } = string.Empty;
        public string NombreServicio { get; set; } = string.Empty;
        public decimal PrecioAdelanto { get; set; }
        public decimal? TotalPagado { get; set; }
        public DateTime? UltimoPago { get; set; }
    }

    public class ReservacionCreateModel
    {
        public string NombreEvento { get; set; } = string.Empty;
        public string FechaEjecucion { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public decimal PrecioTotal { get; set; }
        public string ClienteId { get; set; } = string.Empty;
        // New fields for client registration
        public string NombreCliente { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string TipoEventoNombre { get; set; } = string.Empty;
        public string ServicioId { get; set; } = string.Empty;
        public decimal PrecioAdelanto { get; set; }
    }

    public class ReservacionUpdateModel
    {
        public string nombreEvento { get; set; } = string.Empty;
        public string fechaEjecucion { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public decimal precioTotal { get; set; }
        public string servicioId { get; set; } = string.Empty;
        public decimal precioAdelanto { get; set; }
        public string tipoEventoNombre { get; set; } = string.Empty;  // Cambiar tipoEventoId a tipoEventoNombre
    }
}