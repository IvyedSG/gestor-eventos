using System;

namespace gestor_eventos.Models.ApiModels
{
    public class PagoDto
    {
        public Guid Id { get; set; }
        public Guid ReservaId { get; set; }
        public string IdReserva => ReservaId.ToString();
        public string TipoPago { get; set; } // EFECTIVO, TARJETA, etc.
        public string IdTipoPago => TipoPago;
        public string TipoPagoNombre => TipoPago;
        public decimal Monto { get; set; }
        public string Estado { get; set; } // PENDIENTE, COMPLETADO, CANCELADO, etc.
        public string Descripcion { get; set; }
        public DateTime FechaPago { get; set; }
        public string ComprobanteUrl { get; set; }
        
        // Propiedades adicionales para mostrar en la interfaz
        public string NombreReserva { get; set; }
    }
}