namespace gestor_eventos.Models.ApiModels
{
    public class PagoApi
    {
        public string Id { get; set; } = string.Empty;
        public string IdReserva { get; set; } = string.Empty;
        public string IdTipoPago { get; set; } = string.Empty;
        public string Monto { get; set; } = string.Empty;
        public string TipoPagoNombre { get; set; } = string.Empty;
        public string NombreReserva { get; set; } = string.Empty;
        public DateTime FechaPago { get; set; }  // Nueva propiedad
    }
}