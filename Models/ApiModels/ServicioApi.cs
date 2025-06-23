using System.Collections.Generic;

namespace gestor_eventos.Models.ApiModels
{
    public class ServicioApi
    {
        public string Id { get; set; } = string.Empty;
        public string NombreServicio { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal PrecioBase { get; set; }
        public List<ServicioItemApi> Items { get; set; } = new List<ServicioItemApi>();
        public int TotalItems { get; set; }
    }

    public class ServicioItemApi
    {
        public string Id { get; set; } = string.Empty;
        public string InventarioId { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public string NombreItem { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
        public string PrecioActual { get; set; } = string.Empty;
        public int StockDisponible { get; set; }
    }
}