using System;
using System.Collections.Generic;

namespace gestor_eventos.Models.ApiModels
{
    public class ReservacionCreateModel
    {
        public string nombreEvento { get; set; }
        public string fechaEvento { get; set; }
        public string horaEvento { get; set; }
        public string tipoEvento { get; set; }
        public string descripcion { get; set; }
        public string estado { get; set; }
        public decimal precioTotal { get; set; }
        public string clienteId { get; set; }
        public string nombreCliente { get; set; }
        public string correoCliente { get; set; }
        public string telefonoCliente { get; set; }
        public List<string> servicios { get; set; } = new List<string>();
    }
}