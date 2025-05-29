using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace gestor_eventos.Models.ApiModels
{
    public class ServicioCreateModel
    {
        public string NombreServicio { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioBase { get; set; }
        public List<ServicioItemCreateModel> Items { get; set; } = new List<ServicioItemCreateModel>();
    }

    public class ServicioItemCreateModel
    {
        public string InventarioId { get; set; }
        public int Cantidad { get; set; }
        
        [RegularExpression("^(Nuevo|Dañado|Roto)$", ErrorMessage = "El estado debe ser 'Nuevo', 'Dañado' o 'Roto'")]
        public string Estado { get; set; } = "Nuevo";
        
        public string PrecioActual { get; set; }
    }
}