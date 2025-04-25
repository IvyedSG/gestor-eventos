using System;
using System.Collections.Generic;

namespace gestor_eventos.Models.ApiModels
{
    public class ReservacionUpdateModel
    {
        public string nombreEvento { get; set; }
        public string fechaEvento { get; set; }
        public string horaEvento { get; set; }
        public string tipoEvento { get; set; }
        public string descripcion { get; set; }
        public string estado { get; set; }
        public decimal precioTotal { get; set; }
        public List<ItemToAdd> itemsToAdd { get; set; } = new List<ItemToAdd>();
        public List<string> itemsToRemove { get; set; } = new List<string>();
        
        public class ItemToAdd
        {
            public string servicioId { get; set; }
        }
    }
}