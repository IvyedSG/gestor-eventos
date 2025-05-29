using System;
using System.Collections.Generic;

namespace gestor_eventos.Models.ApiModels
{
    public class ReservacionApi
    {
        public string Id { get; set; }
        public string NombreEvento { get; set; }
        public DateTime FechaEjecucion { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
        public decimal PrecioTotal { get; set; }
        public string ClienteId { get; set; }
        public string NombreCliente { get; set; }
        public string CorreoCliente { get; set; }
        public string TelefonoCliente { get; set; }
        public string TipoEventoId { get; set; }
        public string TipoEventoNombre { get; set; }
        public string ServicioId { get; set; }
        public string NombreServicio { get; set; }
        public decimal PrecioAdelanto { get; set; }
        public decimal? TotalPagado { get; set; }
        public DateTime? UltimoPago { get; set; }
    }

    public class ReservacionCreateModel
    {
        public string NombreEvento { get; set; }
        public string FechaEjecucion { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
        public decimal PrecioTotal { get; set; }
        public string ClienteId { get; set; }
        // New fields for client registration
        public string NombreCliente { get; set; }
        public string CorreoElectronico { get; set; }
        public string Telefono { get; set; }
        public string TipoEventoNombre { get; set; }
        public string ServicioId { get; set; }
        public decimal PrecioAdelanto { get; set; }
    }

    public class ReservacionUpdateModel
    {
        // IMPORTANTE: Los nombres deben coincidir exactamente con lo que espera la API
        public string nombreEvento { get; set; }
        public string fechaEjecucion { get; set; }
        public string descripcion { get; set; }
        public string estado { get; set; }
        public decimal precioTotal { get; set; }
        public string tipoEventoId { get; set; }
        public string servicioId { get; set; }
        public decimal precioAdelanto { get; set; }
        
        // No incluir propiedades adicionales que no espera la API
    }
}