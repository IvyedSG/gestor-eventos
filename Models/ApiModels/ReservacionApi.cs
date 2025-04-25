using System;
using System.Collections.Generic;

namespace gestor_eventos.Models.ApiModels
{
    public class ReservacionApi
    {
        public Guid Id { get; set; }
        public string NombreEvento { get; set; }
        public DateTime FechaEvento { get; set; }
        public string HoraEvento { get; set; }
        public string TipoEvento { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
        public decimal PrecioTotal { get; set; }
        public string NombreCliente { get; set; }
        public string CorreoCliente { get; set; }
        public string TelefonoCliente { get; set; }
        public Guid ServicioId { get; set; }
        public DateTime FechaReserva { get; set; }
        public List<ServicioReservacionApi> Servicios { get; set; } = new List<ServicioReservacionApi>();
    }

    public class ServicioReservacionApi
    {
        public Guid ServicioId { get; set; }
        public string NombreServicio { get; set; }
        public int CantidadItems { get; set; }
        public decimal Precio { get; set; }
    }
}