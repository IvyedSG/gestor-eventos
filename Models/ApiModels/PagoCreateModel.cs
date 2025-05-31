namespace gestor_eventos.Models.ApiModels
{
    public class PagoCreateModel
    {
        public string IdReserva { get; set; }
        public string NombreTipoPago { get; set; }
        public string Monto { get; set; }
    }
}