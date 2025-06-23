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
        public string FechaEvento { get; set; } = string.Empty; 
        public string HoraEvento { get; set; } = string.Empty;  
        public string Descripcion { get; set; } = string.Empty;
        
 
        public DateTime Fecha => DateTime.TryParse(FechaEvento, out var date) ? date : DateTime.Now;
        public TimeSpan HoraInicio => TimeSpan.TryParse(HoraEvento, out var time) ? time : TimeSpan.Zero;
        public TimeSpan HoraFin => HoraInicio.Add(TimeSpan.FromHours(2));
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