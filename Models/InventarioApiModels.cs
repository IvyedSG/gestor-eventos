using System;

namespace gestor_eventos.Models
{
    public class InventarioItemApi
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Stock { get; set; }
        public string Categoria { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}