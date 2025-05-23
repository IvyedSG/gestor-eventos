using System;

namespace GestorEventos.Models.ApiModels
{
    public class ClienteApi
    {
        public string Id { get; set; }
        public string TipoCliente { get; set; }
        public string Direccion { get; set; }
        public string Ruc { get; set; }
        public string RazonSocial { get; set; }
        public string UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
        public string CorreoUsuario { get; set; }
        public int TotalReservas { get; set; }
        public DateTime? UltimaFechaReserva { get; set; }
    }
}