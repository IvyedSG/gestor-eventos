using System;
using System.Collections.Generic;

namespace GestorEventos.Models.ApiModels
{
    public class DashboardResponse
    {
        public Metricas Metricas { get; set; } = new();
        public ProximasReservas ProximasReservas { get; set; } = new();
        public ActividadReciente ActividadReciente { get; set; } = new();
    }

    public class Metricas
    {
        public int TotalInventarioItems { get; set; }
        public int EventosActivos { get; set; }
        public int TotalClientes { get; set; }
        public int CantidadServicios { get; set; }
        public int ReservasConfirmadasMes { get; set; }
        public int ReservasPendientesMes { get; set; }
        public decimal IngresosEstimadosMes { get; set; }
    }

    public class ProximasReservas
    {
        public List<ReservaResumen> Reservas { get; set; } = new List<ReservaResumen>();
    }

    public class ReservaResumen
    {
        public string Id { get; set; } = string.Empty;
        public string NombreEvento { get; set; } = string.Empty; 
        public string FechaEjecucion { get; set; } = string.Empty; // Cambiado de FechaEvento
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty; // Agregado
        
        // Propiedades calculadas actualizadas
        public DateTime Fecha => DateTime.TryParse(FechaEjecucion, out var date) ? date : DateTime.Now;
        public string Nombre => NombreEvento;
    }

    public class ActividadReciente
    {
        public List<Actividad> Actividades { get; set; } = new List<Actividad>();
    }

    public class Actividad
    {
        public string Id { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public string TiempoTranscurrido { get; set; } = string.Empty;
    }
}