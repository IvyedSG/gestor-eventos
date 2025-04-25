using System;
using System.Collections.Generic;

namespace gestor_eventos.Models.ApiModels
{
    public class ServicioCreateModel
    {
        public string NombreServicio { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioBase { get; set; }
        public string TipoEvento { get; set; }
        public string Imagenes { get; set; }
        public List<ServicioItemCreateModel> Items { get; set; } = new List<ServicioItemCreateModel>();
    }

    public class ServicioItemCreateModel
    {
        public Guid InventarioId { get; set; }
        public int Cantidad { get; set; }
    }
}