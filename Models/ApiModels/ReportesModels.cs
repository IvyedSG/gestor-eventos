using System;
using System.Collections.Generic;

namespace GestorEventos.Models.ApiModels
{
    // Modelos para Resumen Ejecutivo
    public class ResumenEjecutivoResponse
    {
        public int TotalClientes { get; set; }
        public int ClientesNuevosUltimoMes { get; set; }
        public double TasaRetencionClientes { get; set; }
        public int TotalReservas { get; set; }
        public int ReservasUltimoMes { get; set; }
        public decimal IngresosTotales { get; set; }
        public decimal IngresosUltimoMes { get; set; }
        public double TasaConversionReservas { get; set; }
        public decimal MontoPromedioReserva { get; set; }
        public double PorcentajePagosCompletos { get; set; }
        public double PromedioDiasPago { get; set; }
        public int TotalServicios { get; set; }
        public int ServiciosActivos { get; set; }
        public string ServicioMasFrecuente { get; set; } = string.Empty;
        public int TotalItems { get; set; }
        public double TasaDisponibilidadPromedio { get; set; }
        public string ItemMasUtilizado { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public DateTime FechaGeneracion { get; set; }
    }

    // Modelos para Reportes de Clientes
    public class ReportesClientesResponse
    {
        public List<ClientesNuevosPorMes> ClientesNuevosPorMes { get; set; } = new();
        public List<PromedioAdelantoPorCliente> PromedioAdelantoPorCliente { get; set; } = new();
        public TasaRetencionClientes TasaRetencionClientes { get; set; } = new();
    }

    public class ClientesNuevosPorMes
    {
        public int Año { get; set; }
        public int Mes { get; set; }
        public string NombreMes { get; set; } = string.Empty;
        public int CantidadClientesNuevos { get; set; }
    }

    public class PromedioAdelantoPorCliente
    {
        public string ClienteId { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public double PromedioAdelantoPorc { get; set; }
        public int CantidadReservas { get; set; }
    }

    public class TasaRetencionClientes
    {
        public int TotalClientes { get; set; }
        public int ClientesConMultiplesReservas { get; set; }
        public double PorcentajeMultiplesReservas { get; set; }
        public double TasaRetencion { get; set; }
    }

    // Modelos para Reportes de Items/Inventario
    public class ReportesItemsResponse
    {
        public List<ItemMasUtilizado> ItemsMasUtilizados { get; set; } = new();
        public List<StockPromedioPorTipoServicio> StockPromedioPorTipoServicio { get; set; } = new();
        public List<TasaDisponibilidad> TasaDisponibilidad { get; set; } = new();
    }

    public class ItemMasUtilizado
    {
        public string InventarioId { get; set; } = string.Empty;
        public string NombreItem { get; set; } = string.Empty;
        public int TotalCantidadUtilizada { get; set; }
        public int FrecuenciaUso { get; set; }
        public double PromedioUsoPorServicio { get; set; }
    }

    public class StockPromedioPorTipoServicio
    {
        public string ServicioId { get; set; } = string.Empty;
        public string NombreServicio { get; set; } = string.Empty;
        public double StockPromedioUtilizado { get; set; }
        public int CantidadDetalles { get; set; }
    }

    public class TasaDisponibilidad
    {
        public string InventarioId { get; set; } = string.Empty;
        public string NombreItem { get; set; } = string.Empty;
        public int Stock { get; set; }
        public int StockDisponible { get; set; }
        public double TasaDisponibilidadPorc { get; set; }
    }

    // Modelos para Reportes de Pagos
    public class ReportesPagosResponse
    {
        public List<MontoPromedioPorPago> MontoPromedioPorPago { get; set; } = new();
        public PromedioDiasReservaPago PromedioDiasReservaPago { get; set; } = new();
        public List<ReservaPagoIncompleto> ReservasPagosIncompletos { get; set; } = new();
        public List<TasaUsoMetodoPago> TasaUsoMetodoPago { get; set; } = new();
        public List<TendenciaMensualIngresos> TendenciaMensualIngresos { get; set; } = new();
    }

    public class MontoPromedioPorPago
    {
        public string TipoPago { get; set; } = string.Empty;
        public decimal MontoPromedio { get; set; }
        public int CantidadPagos { get; set; }
        public decimal MontoTotal { get; set; }
    }

    public class PromedioDiasReservaPago
    {
        public double PromedioDias { get; set; }
        public int CantidadReservasConPagos { get; set; }
        public double DiasMinimo { get; set; }
        public double DiasMaximo { get; set; }
    }

    public class ReservaPagoIncompleto
    {
        public string ReservaId { get; set; } = string.Empty;
        public string NombreEvento { get; set; } = string.Empty;
        public string ClienteRazonSocial { get; set; } = string.Empty;
        public decimal PrecioTotal { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal MontoPendiente { get; set; }
        public double PorcentajePagado { get; set; }
    }

    public class TasaUsoMetodoPago
    {
        public string TipoPago { get; set; } = string.Empty;
        public int CantidadUsos { get; set; }
        public decimal MontoTotal { get; set; }
        public double PorcentajeUso { get; set; }
    }

    public class TendenciaMensualIngresos
    {
        public int Año { get; set; }
        public int Mes { get; set; }
        public string NombreMes { get; set; } = string.Empty;
        public decimal MontoTotal { get; set; }
        public int CantidadPagos { get; set; }
        public decimal MontoPromedio { get; set; }
    }

    // Modelos para Reportes de Reservas
    public class ReportesReservasResponse
    {
        public List<ReservaPorMes> ReservasPorMes { get; set; } = new();
        public List<IngresoPromedioPorTipoEvento> IngresosPromedioPorTipoEvento { get; set; } = new();
        public List<ReservaAdelantoAlto> ReservasAdelantoAlto { get; set; } = new();
        public DuracionPromedioReservas DuracionPromedioReservas { get; set; } = new();
        public TasaConversionEstado TasaConversionEstado { get; set; } = new();
        public List<DistribucionReservaPorCliente> DistribucionReservasPorCliente { get; set; } = new();
    }

    public class ReservaPorMes
    {
        public int Año { get; set; }
        public int Mes { get; set; }
        public string NombreMes { get; set; } = string.Empty;
        public int CantidadReservas { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal MontoPromedio { get; set; }
    }

    public class IngresoPromedioPorTipoEvento
    {
        public string TipoEvento { get; set; } = string.Empty;
        public decimal IngresoPromedio { get; set; }
        public int CantidadReservas { get; set; }
        public decimal IngresoTotal { get; set; }
        public decimal IngresoMinimo { get; set; }
        public decimal IngresoMaximo { get; set; }
    }

    public class ReservaAdelantoAlto
    {
        public string ReservaId { get; set; } = string.Empty;
        public string NombreEvento { get; set; } = string.Empty;
        public string ClienteRazonSocial { get; set; } = string.Empty;
        public decimal PrecioTotal { get; set; }
        public decimal MontoAdelanto { get; set; }
        public double PorcentajeAdelanto { get; set; }
    }

    public class DuracionPromedioReservas
    {
        public double DuracionPromedioDias { get; set; }
        public int CantidadReservas { get; set; }
        public double DuracionMinimaDias { get; set; }
        public double DuracionMaximaDias { get; set; }
    }

    public class TasaConversionEstado
    {
        public int ReservasPendientes { get; set; }
        public int ReservasConfirmadas { get; set; }
        public int ReservasCanceladas { get; set; }
        public int ReservasFinalizadas { get; set; } // *** NUEVO CAMPO ***
        public double TasaConversionPendienteConfirmado { get; set; }
        public double TasaCancelacion { get; set; }
        public double TasaFinalizacion { get; set; } // *** NUEVO CAMPO ***
    }

    public class DistribucionReservaPorCliente
    {
        public string ClienteId { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public int TotalReservas { get; set; }
        public int ReservasUltimosTresMeses { get; set; }
        public decimal MontoTotalReservas { get; set; }
        public DateTime UltimaReserva { get; set; }
    }

    // Modelos para Reportes de Servicios
    public class ReportesServiciosResponse
    {
        public List<ServicioMasFrecuente> ServiciosMasFrecuentes { get; set; } = new();
        public List<VariacionIngresosMensualesServicio> VariacionIngresosMensualesServicio { get; set; } = new();
        public List<PromedioItemsPorServicio> PromedioItemsPorServicio { get; set; } = new();
        public List<ServicioSinReserva> ServiciosSinReservas { get; set; } = new();
        public List<ServicioEventoCancelado> ServiciosEventosCancelados { get; set; } = new();
    }

    public class ServicioMasFrecuente
    {
        public string ServicioId { get; set; } = string.Empty;
        public string NombreServicio { get; set; } = string.Empty;
        public int CantidadReservas { get; set; }
        public double PorcentajeUso { get; set; }
        public decimal IngresoTotal { get; set; }
        public decimal IngresoPromedio { get; set; }
    }

    public class VariacionIngresosMensualesServicio
    {
        public string ServicioId { get; set; } = string.Empty;
        public string NombreServicio { get; set; } = string.Empty;
        public int Año { get; set; }
        public int Mes { get; set; }
        public string NombreMes { get; set; } = string.Empty;
        public decimal MontoMensual { get; set; }
        public int CantidadReservas { get; set; }
        public double VariacionPorc { get; set; }
    }

    public class PromedioItemsPorServicio
    {
        public string ServicioId { get; set; } = string.Empty;
        public string NombreServicio { get; set; } = string.Empty;
        public double PromedioItemsUsados { get; set; }
        public int TotalDetalles { get; set; }
        public int CantidadReservas { get; set; }
    }

    public class ServicioSinReserva
    {
        public string ServicioId { get; set; } = string.Empty;
        public string NombreServicio { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal PrecioBase { get; set; }
        public int DiasInactivo { get; set; }
    }

    public class ServicioEventoCancelado
    {
        public string ServicioId { get; set; } = string.Empty;
        public string NombreServicio { get; set; } = string.Empty;
        public int TotalReservas { get; set; }
        public int ReservasCanceladas { get; set; }
        public double PorcentajeCancelacion { get; set; }
        public decimal MontoPerdidasCancelacion { get; set; }
    }
}