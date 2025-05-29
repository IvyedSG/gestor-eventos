using System.Collections.Generic;

namespace gestor_eventos.Models.ApiModels
{
    public class ServicioApi
    {
        public string Id { get; set; }
        public string NombreServicio { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioBase { get; set; }
        public List<ServicioItemApi> Items { get; set; } = new List<ServicioItemApi>();
        public int TotalItems { get; set; }
    }

    public class ServicioItemApi
    {
        public string Id { get; set; }
        public string InventarioId { get; set; }
        public int Cantidad { get; set; }
        public string NombreItem { get; set; }
        public string Estado { get; set; }
        public string Fecha { get; set; }
        public string PrecioActual { get; set; }
        public int StockDisponible { get; set; }
    }
}