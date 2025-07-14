namespace gestor_eventos.Models.ApiModels
{
    public class PagoCreateModel
    {
        public string IdReserva { get; set; } = string.Empty;
        public string NombreTipoPago { get; set; } = string.Empty;
        public string NombreReserva { get; set; } = string.Empty;
        public string Monto { get; set; } = string.Empty;
    }
}