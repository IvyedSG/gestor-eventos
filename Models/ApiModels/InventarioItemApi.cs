using System;
using System.Text.Json.Serialization;

namespace gestor_eventos.Models
{
    public class InventarioItemApi
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Stock { get; set; }
        public int StockDisponible { get; set; }
        public int ItemsEnUso { get; set; }
        
        // Propiedad original que coincide con la API
        public string Preciobase { get; set; } = string.Empty;
        
        // Propiedad calculada para mantener compatibilidad con el código existente
        [JsonIgnore]
        public string PrecioBase 
        { 
            get => Preciobase; 
            set => Preciobase = value; 
        }
        
        public DateTime FechaRegistro { get; set; }
    }
}