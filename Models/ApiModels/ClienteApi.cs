using System;

namespace GestorEventos.Models.ApiModels
{
    public class ClienteApi
    {
        public string Id { get; set; } = string.Empty;
        public string TipoCliente { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Ruc { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string CorreoUsuario { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public int TotalReservas { get; set; }
        public DateTime? UltimaFechaReserva { get; set; }
    }
}