public class ReservacionUpdateModel
{
    public string nombreEvento { get; set; }
    public string fechaEjecucion { get; set; }
    public string descripcion { get; set; }
    public string estado { get; set; }
    public decimal precioTotal { get; set; }
    public string servicioId { get; set; }
    public decimal precioAdelanto { get; set; }
    public string tipoEventoNombre { get; set; }  // Asegúrate de que solo existe esta propiedad, no tipoEventoId
}