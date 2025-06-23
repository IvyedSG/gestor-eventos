using System.Collections.Generic;

namespace gestor_eventos.Models.ApiModels
{
    public class ServicioEditModel
    {
        public string NombreServicio { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal PrecioBase { get; set; }
        public List<ServicioItemCreateModel> ItemsToAdd { get; set; } = new List<ServicioItemCreateModel>();
        public List<string> ItemsToRemove { get; set; } = new List<string>();
    }
}