using System;
using System.Collections.Generic;

namespace GestorEventos.Models.ApiModels
{
    public class DashboardResponse
    {
        public Metricas Metricas { get; set; }
        public ProximasReservas ProximasReservas { get; set; }
        public ActividadReciente ActividadReciente { get; set; }
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
        public string Id { get; set; }
        public string NombreEvento { get; set; } // Cambio de Nombre a NombreEvento
        public string FechaEvento { get; set; }  // Cambio de DateTime Fecha a string FechaEvento
        public string HoraEvento { get; set; }   // Cambio de TimeSpan a string HoraEvento
        public string Descripcion { get; set; }
        
        // Propiedades calculadas para mantener compatibilidad con el código existente
        public DateTime Fecha => DateTime.TryParse(FechaEvento, out var date) ? date : DateTime.Now;
        public TimeSpan HoraInicio => TimeSpan.TryParse(HoraEvento, out var time) ? time : TimeSpan.Zero;
        public TimeSpan HoraFin => HoraInicio.Add(TimeSpan.FromHours(2)); // Estimamos 2 horas por defecto
        public string Nombre => NombreEvento;
    }

    public class ActividadReciente
    {
        public List<Actividad> Actividades { get; set; } = new List<Actividad>();
    }

    public class Actividad
    {
        public string Id { get; set; }
        public string Tipo { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string TiempoTranscurrido { get; set; }
    }
}