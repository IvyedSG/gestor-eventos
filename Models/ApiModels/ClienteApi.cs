using System;

namespace GestorEventos.Models.ApiModels
{
    public class ClienteApi
    {
        public string Id { get; set; }
        public string TipoCliente { get; set; }
        public string Nombre { get; set; }
        public string CorreoElectronico { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}