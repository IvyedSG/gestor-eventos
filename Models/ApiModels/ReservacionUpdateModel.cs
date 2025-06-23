public class ReservacionUpdateModel
{
    public string nombreEvento { get; set; } = string.Empty;
    public string fechaEjecucion { get; set; } = string.Empty;
    public string descripcion { get; set; } = string.Empty;
    public string estado { get; set; } = string.Empty;
    public decimal precioTotal { get; set; }
    public string servicioId { get; set; } = string.Empty;
    public decimal precioAdelanto { get; set; }
    public string tipoEventoNombre { get; set; } = string.Empty;  // Asegúrate de que solo existe esta propiedad, no tipoEventoId
}