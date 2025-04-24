using System;
using System.Collections.Generic;

namespace gestor_eventos.Models.ApiModels
{
    public class ServicioApi
    {
        public Guid Id { get; set; }
        public string NombreServicio { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioBase { get; set; }
        public string TipoEvento { get; set; }
        public string Imagenes { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string UsuarioId { get; set; }
        public List<ServicioItemApi> Items { get; set; } = new List<ServicioItemApi>();
        public int TotalItems { get; set; }
    }

    public class ServicioItemApi
    {
        public Guid Id { get; set; }
        public Guid InventarioId { get; set; }
        public int Cantidad { get; set; }
        public string NombreItem { get; set; }
        public string CategoriaItem { get; set; }
        public int StockActual { get; set; }
    }
}