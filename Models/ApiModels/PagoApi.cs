namespace gestor_eventos.Models.ApiModels
{
    public class PagoApi
    {
        public string Id { get; set; }
        public string IdReserva { get; set; }
        public string IdTipoPago { get; set; }
        public string Monto { get; set; }
        public string TipoPagoNombre { get; set; }
        public string NombreReserva { get; set; }
        public DateTime FechaPago { get; set; }  // Nueva propiedad
    }
}