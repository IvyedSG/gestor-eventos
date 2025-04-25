using System;
using System.Collections.Generic;

namespace gestor_eventos.Models.ApiModels
{
    public class ServicioUpdateModel
    {
        public string Id { get; set; }
        public string NombreServicio { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioBase { get; set; }
        public string TipoEvento { get; set; }
        public string Imagenes { get; set; }
        public List<ServicioItemUpdateModel> Items { get; set; } = new List<ServicioItemUpdateModel>();
    }

    public class ServicioItemUpdateModel
    {
        public string OriginalItemId { get; set; }
        public Guid InventarioId { get; set; }
        public int Cantidad { get; set; }
    }
}