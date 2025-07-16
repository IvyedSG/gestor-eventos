using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using GestorEventos.Models.ApiModels;

namespace gestor_eventos.Services
{
    public class ExcelExportService
    {
        public byte[] ExportReportesToExcel(
            ResumenEjecutivoResponse? resumenEjecutivo,
            ReportesClientesResponse? reportesClientes,
            ReportesPagosResponse? reportesPagos,
            ReportesReservasResponse? reportesReservas,
            ReportesServiciosResponse? reportesServicios,
            ReportesItemsResponse? reportesItems,
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage();
            
            // Crear hojas para cada sección con datos completos
            CreateResumenEjecutivoSheet(package, resumenEjecutivo, fechaInicio, fechaFin);
            CreateClientesSheet(package, reportesClientes);
            CreatePagosSheet(package, reportesPagos);
            CreateReservasSheet(package, reportesReservas, fechaInicio, fechaFin);
            CreateServiciosSheet(package, reportesServicios);
            CreateInventarioSheet(package, reportesItems);
            
            return package.GetAsByteArray();
        }

        private void CreateResumenEjecutivoSheet(ExcelPackage package, ResumenEjecutivoResponse? data, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var worksheet = package.Workbook.Worksheets.Add("Resumen Ejecutivo");
            
            // Encabezado principal
            worksheet.Cells["A1"].Value = "REPORTE EJECUTIVO - GESTOR DE EVENTOS";
            worksheet.Cells["A1:H1"].Merge = true;
            ApplyHeaderStyle(worksheet.Cells["A1:H1"], Color.FromArgb(68, 114, 196));
            
            int row = 3;
            
            if (data != null)
            {
                // Información del período y generación
                worksheet.Cells[$"A{row}"].Value = "INFORMACIÓN DEL REPORTE";
                worksheet.Cells[$"A{row}:H{row}"].Merge = true;
                ApplySectionHeaderStyle(worksheet.Cells[$"A{row}:H{row}"]);
                row++;
                
                worksheet.Cells[$"A{row}"].Value = "Período de Análisis";
                worksheet.Cells[$"B{row}"].Value = $"{data.FechaInicio:dd/MM/yyyy} - {data.FechaFin:dd/MM/yyyy}";
                ApplyDataRowStyle(worksheet.Cells[$"A{row}:B{row}"], true);
                row++;
                
                worksheet.Cells[$"A{row}"].Value = "Fecha de Generación";
                worksheet.Cells[$"B{row}"].Value = data.FechaGeneracion.ToString("dd/MM/yyyy HH:mm:ss");
                ApplyDataRowStyle(worksheet.Cells[$"A{row}:B{row}"], false);
                row += 3;
                
                // SECCIÓN 1: Métricas de Clientes
                worksheet.Cells[$"A{row}"].Value = "MÉTRICAS DE CLIENTES";
                worksheet.Cells[$"A{row}:H{row}"].Merge = true;
                ApplySectionHeaderStyle(worksheet.Cells[$"A{row}:H{row}"]);
                row++;
                
                worksheet.Cells[$"A{row}"].Value = "Métrica";
                worksheet.Cells[$"B{row}"].Value = "Valor";
                worksheet.Cells[$"C{row}"].Value = "Descripción";
                ApplyTableHeaderStyle(worksheet.Cells[$"A{row}:C{row}"]);
                row++;
                
                var metricasClientes = new[]
                {
                    ("Total Clientes", data.TotalClientes.ToString(), "Número total de clientes registrados"),
                    ("Nuevos (Último Mes)", data.ClientesNuevosUltimoMes.ToString(), "Clientes registrados en el último mes"),
                    ("Tasa de Retención", $"{data.TasaRetencionClientes:F1}%", "Porcentaje de clientes que mantienen actividad")
                };
                
                foreach (var (metrica, valor, descripcion) in metricasClientes)
                {
                    worksheet.Cells[$"A{row}"].Value = metrica;
                    worksheet.Cells[$"B{row}"].Value = valor;
                    worksheet.Cells[$"C{row}"].Value = descripcion;
                    ApplyDataRowStyle(worksheet.Cells[$"A{row}:C{row}"], row % 2 == 0);
                    row++;
                }
                row += 2;
                
                // SECCIÓN 2: Métricas de Reservas e Ingresos
                worksheet.Cells[$"A{row}"].Value = "MÉTRICAS DE RESERVAS E INGRESOS";
                worksheet.Cells[$"A{row}:H{row}"].Merge = true;
                ApplySectionHeaderStyle(worksheet.Cells[$"A{row}:H{row}"]);
                row++;
                
                worksheet.Cells[$"A{row}"].Value = "Métrica";
                worksheet.Cells[$"B{row}"].Value = "Valor";
                worksheet.Cells[$"C{row}"].Value = "Descripción";
                ApplyTableHeaderStyle(worksheet.Cells[$"A{row}:C{row}"]);
                row++;
                
                var metricasReservas = new[]
                {
                    ("Total Reservas", data.TotalReservas.ToString(), "Número total de reservas realizadas"),
                    ("Reservas (Último Mes)", data.ReservasUltimoMes.ToString(), "Reservas realizadas en el último mes"),
                    ("Ingresos Totales", data.IngresosTotales.ToString("C", new System.Globalization.CultureInfo("es-PE")), "Monto total de ingresos generados"),
                    ("Ingresos (Último Mes)", data.IngresosUltimoMes.ToString("C", new System.Globalization.CultureInfo("es-PE")), "Ingresos del último mes"),
                    ("Monto Promedio/Reserva", data.MontoPromedioReserva.ToString("C", new System.Globalization.CultureInfo("es-PE")), "Promedio de ingresos por reserva"),
                    ("Tasa de Conversión", $"{data.TasaConversionReservas:F1}%", "Porcentaje de conversión de consultas a reservas")
                };
                
                foreach (var (metrica, valor, descripcion) in metricasReservas)
                {
                    worksheet.Cells[$"A{row}"].Value = metrica;
                    worksheet.Cells[$"B{row}"].Value = valor;
                    worksheet.Cells[$"C{row}"].Value = descripcion;
                    ApplyDataRowStyle(worksheet.Cells[$"A{row}:C{row}"], row % 2 == 0);
                    row++;
                }
                row += 2;
                
                // SECCIÓN 3: Métricas de Pagos
                worksheet.Cells[$"A{row}"].Value = "MÉTRICAS DE PAGOS";
                worksheet.Cells[$"A{row}:H{row}"].Merge = true;
                ApplySectionHeaderStyle(worksheet.Cells[$"A{row}:H{row}"]);
                row++;
                
                worksheet.Cells[$"A{row}"].Value = "Métrica";
                worksheet.Cells[$"B{row}"].Value = "Valor";
                worksheet.Cells[$"C{row}"].Value = "Descripción";
                ApplyTableHeaderStyle(worksheet.Cells[$"A{row}:C{row}"]);
                row++;
                
                var metricasPagos = new[]
                {
                    ("% Pagos Completos", $"{data.PorcentajePagosCompletos:F1}%", "Porcentaje de reservas con pagos completados"),
                    ("Promedio Días de Pago", $"{data.PromedioDiasPago:F2}", "Tiempo promedio entre reserva y pago completo")
                };
                
                foreach (var (metrica, valor, descripcion) in metricasPagos)
                {
                    worksheet.Cells[$"A{row}"].Value = metrica;
                    worksheet.Cells[$"B{row}"].Value = valor;
                    worksheet.Cells[$"C{row}"].Value = descripcion;
                    ApplyDataRowStyle(worksheet.Cells[$"A{row}:C{row}"], row % 2 == 0);
                    row++;
                }
                row += 2;
                
                // SECCIÓN 4: Métricas de Servicios
                worksheet.Cells[$"A{row}"].Value = "MÉTRICAS DE SERVICIOS";
                worksheet.Cells[$"A{row}:H{row}"].Merge = true;
                ApplySectionHeaderStyle(worksheet.Cells[$"A{row}:H{row}"]);
                row++;
                
                worksheet.Cells[$"A{row}"].Value = "Métrica";
                worksheet.Cells[$"B{row}"].Value = "Valor";
                worksheet.Cells[$"C{row}"].Value = "Descripción";
                ApplyTableHeaderStyle(worksheet.Cells[$"A{row}:C{row}"]);
                row++;
                
                var metricasServicios = new[]
                {
                    ("Total Servicios", data.TotalServicios.ToString(), "Número total de servicios registrados"),
                    ("Servicios Activos", data.ServiciosActivos.ToString(), "Servicios disponibles para reservas"),
                    ("Servicio Más Frecuente", data.ServicioMasFrecuente, "Servicio con mayor número de reservas")
                };
                
                foreach (var (metrica, valor, descripcion) in metricasServicios)
                {
                    worksheet.Cells[$"A{row}"].Value = metrica;
                    worksheet.Cells[$"B{row}"].Value = valor;
                    worksheet.Cells[$"C{row}"].Value = descripcion;
                    ApplyDataRowStyle(worksheet.Cells[$"A{row}:C{row}"], row % 2 == 0);
                    row++;
                }
                row += 2;
                
                // SECCIÓN 5: Métricas de Inventario
                worksheet.Cells[$"A{row}"].Value = "MÉTRICAS DE INVENTARIO";
                worksheet.Cells[$"A{row}:H{row}"].Merge = true;
                ApplySectionHeaderStyle(worksheet.Cells[$"A{row}:H{row}"]);
                row++;
                
                worksheet.Cells[$"A{row}"].Value = "Métrica";
                worksheet.Cells[$"B{row}"].Value = "Valor";
                worksheet.Cells[$"C{row}"].Value = "Descripción";
                ApplyTableHeaderStyle(worksheet.Cells[$"A{row}:C{row}"]);
                row++;
                
                var metricasInventario = new[]
                {
                    ("Total Items", data.TotalItems.ToString(), "Número total de items en inventario"),
                    ("Tasa Disponibilidad Promedio", $"{data.TasaDisponibilidadPromedio:F1}%", "Porcentaje promedio de disponibilidad de items"),
                    ("Item Más Utilizado", data.ItemMasUtilizado, "Item con mayor frecuencia de uso en eventos")
                };
                
                foreach (var (metrica, valor, descripcion) in metricasInventario)
                {
                    worksheet.Cells[$"A{row}"].Value = metrica;
                    worksheet.Cells[$"B{row}"].Value = valor;
                    worksheet.Cells[$"C{row}"].Value = descripcion;
                    ApplyDataRowStyle(worksheet.Cells[$"A{row}:C{row}"], row % 2 == 0);
                    row++;
                }
            }
            
            // Ajustar columnas
            worksheet.Cells.AutoFitColumns();
            worksheet.Column(1).Width = 25;
            worksheet.Column(2).Width = 20;
            worksheet.Column(3).Width = 50;
        }

        private void CreateClientesSheet(ExcelPackage package, ReportesClientesResponse? data)
        {
            var worksheet = package.Workbook.Worksheets.Add("Clientes");
            
            // Encabezado
            worksheet.Cells["A1"].Value = "REPORTE DE CLIENTES";
            worksheet.Cells["A1:G1"].Merge = true;
            ApplyHeaderStyle(worksheet.Cells["A1:G1"], Color.FromArgb(70, 130, 180));
            
            int row = 3;
            
            // SECCIÓN 1: Clientes Nuevos por Mes
            if (data?.ClientesNuevosPorMes != null && data.ClientesNuevosPorMes.Any())
            {
                worksheet.Cells[$"A{row}"].Value = "CLIENTES NUEVOS POR MES";
                worksheet.Cells[$"A{row}:G{row}"].Merge = true;
                ApplySectionHeaderStyle(worksheet.Cells[$"A{row}:G{row}"]);
                row++;
                
                worksheet.Cells[$"A{row}"].Value = "Año";
                worksheet.Cells[$"B{row}"].Value = "Mes";
                worksheet.Cells[$"C{row}"].Value = "Nombre Mes";
                worksheet.Cells[$"D{row}"].Value = "Cantidad Clientes Nuevos";
                ApplyTableHeaderStyle(worksheet.Cells[$"A{row}:D{row}"]);
                row++;
                
                foreach (var clienteMes in data.ClientesNuevosPorMes.OrderBy(x => x.Año).ThenBy(x => x.Mes))
                {
                    worksheet.Cells[$"A{row}"].Value = clienteMes.Año;
                    worksheet.Cells[$"B{row}"].Value = clienteMes.Mes;
                    worksheet.Cells[$"C{row}"].Value = clienteMes.NombreMes;
                    worksheet.Cells[$"D{row}"].Value = clienteMes.CantidadClientesNuevos;
                    ApplyDataRowStyle(worksheet.Cells[$"A{row}:D{row}"], row % 2 == 0);
                    row++;
                }
                row += 2; // Espacio entre secciones
            }
            
            // SECCIÓN 2: Top Clientes por Adelanto
            if (data?.PromedioAdelantoPorCliente != null && data.PromedioAdelantoPorCliente.Any())
            {
                worksheet.Cells[$"A{row}"].Value = "TOP CLIENTES POR ADELANTO";
                worksheet.Cells[$"A{row}:G{row}"].Merge = true;
                ApplySectionHeaderStyle(worksheet.Cells[$"A{row}:G{row}"]);
                row++;
                
                worksheet.Cells[$"A{row}"].Value = "Cliente ID";
                worksheet.Cells[$"B{row}"].Value = "Razón Social";
                worksheet.Cells[$"C{row}"].Value = "Promedio Adelanto %";
                worksheet.Cells[$"D{row}"].Value = "Cantidad Reservas";
                ApplyTableHeaderStyle(worksheet.Cells[$"A{row}:D{row}"]);
                row++;
                
                foreach (var cliente in data.PromedioAdelantoPorCliente)
                {
                    worksheet.Cells[$"A{row}"].Value = cliente.ClienteId;
                    worksheet.Cells[$"B{row}"].Value = string.IsNullOrEmpty(cliente.RazonSocial) ? "Sin especificar" : cliente.RazonSocial;
                    worksheet.Cells[$"C{row}"].Value = cliente.PromedioAdelantoPorc / 100.0;
                    worksheet.Cells[$"C{row}"].Style.Numberformat.Format = "0.00%";
                    worksheet.Cells[$"D{row}"].Value = cliente.CantidadReservas;
                    ApplyDataRowStyle(worksheet.Cells[$"A{row}:D{row}"], row % 2 == 0);
                    row++;
                }
                row += 2; // Espacio entre secciones
            }
            
            // SECCIÓN 3: Tasa de Retención de Clientes
            if (data?.TasaRetencionClientes != null)
            {
                worksheet.Cells[$"A{row}"].Value = "TASA DE RETENCIÓN DE CLIENTES";
                worksheet.Cells[$"A{row}:G{row}"].Merge = true;
                ApplySectionHeaderStyle(worksheet.Cells[$"A{row}:G{row}"]);
                row++;
                
                worksheet.Cells[$"A{row}"].Value = "Métrica";
                worksheet.Cells[$"B{row}"].Value = "Valor";
                ApplyTableHeaderStyle(worksheet.Cells[$"A{row}:B{row}"]);
                row++;
                
                var retencion = data.TasaRetencionClientes;
                var metricas = new[]
                {
                    ("Total de Clientes", retencion.TotalClientes.ToString()),
                    ("Clientes con Múltiples Reservas", retencion.ClientesConMultiplesReservas.ToString()),
                    ("% Múltiples Reservas", $"{retencion.PorcentajeMultiplesReservas:F2}%"),
                    ("Tasa de Retención", $"{retencion.TasaRetencion:F2}%")
                };
                
                foreach (var (metrica, valor) in metricas)
                {
                    worksheet.Cells[$"A{row}"].Value = metrica;
                    worksheet.Cells[$"B{row}"].Value = valor;
                    ApplyDataRowStyle(worksheet.Cells[$"A{row}:B{row}"], row % 2 == 0);
                    row++;
                }
            }
            
            worksheet.Cells.AutoFitColumns();
            worksheet.Column(1).Width = 25; // Para nombres largos
            worksheet.Column(2).Width = 25; // Para razón social
        }

        private void CreatePagosSheet(ExcelPackage package, ReportesPagosResponse? data)
        {
            var worksheet = package.Workbook.Worksheets.Add("Pagos e Ingresos");
            
            // Encabezado
            worksheet.Cells["A1"].Value = "REPORTE DE PAGOS E INGRESOS";
            worksheet.Cells["A1:H1"].Merge = true;
            ApplyHeaderStyle(worksheet.Cells["A1:H1"], Color.FromArgb(34, 139, 34));
            
            int row = 3;
            
            if (data != null)
            {
                // SECCIÓN 1: Monto Promedio por Tipo de Pago
                if (data.MontoPromedioPorPago != null && data.MontoPromedioPorPago.Any())
                {
                    worksheet.Cells[$"A{row}"].Value = "MONTO PROMEDIO POR TIPO DE PAGO";
                    worksheet.Cells[$"A{row}:H{row}"].Merge = true;
                    ApplySectionHeaderStyle(worksheet.Cells[$"A{row}:H{row}"]);
                    row++;
                    
                    worksheet.Cells[$"A{row}"].Value = "Tipo de Pago";
                    worksheet.Cells[$"B{row}"].Value = "Monto Promedio";
                    worksheet.Cells[$"C{row}"].Value = "Cantidad Pagos";
                    worksheet.Cells[$"D{row}"].Value = "Monto Total";
                    ApplyTableHeaderStyle(worksheet.Cells[$"A{row}:D{row}"]);
                    row++;
                    
                    foreach (var pago in data.MontoPromedioPorPago)
                    {
                        worksheet.Cells[$"A{row}"].Value = pago.TipoPago;
                        worksheet.Cells[$"B{row}"].Value = pago.MontoPromedio;
                        worksheet.Cells[$"B{row}"].Style.Numberformat.Format = "\"S/. \"#,##0.00";
                        worksheet.Cells[$"C{row}"].Value = pago.CantidadPagos;
                        worksheet.Cells[$"D{row}"].Value = pago.MontoTotal;
                        worksheet.Cells[$"D{row}"].Style.Numberformat.Format = "\"S/. \"#,##0.00";
                        ApplyDataRowStyle(worksheet.Cells[$"A{row}:D{row}"], row % 2 == 0);
                        row++;
                    }
                    row += 2; // Espacio entre secciones
                }
                
                // SECCIÓN 2: Promedio Días entre Reserva y Pago
                if (data.PromedioDiasReservaPago != null)
                {
                    worksheet.Cells[$"A{row}"].Value = "TIEMPO PROMEDIO RESERVA-PAGO";
                    worksheet.Cells[$"A{row}:H{row}"].Merge = true;
                    ApplySectionHeaderStyle(worksheet.Cells[$"A{row}:H{row}"]);
                    row++;
                    
                    worksheet.Cells[$"A{row}"].Value = "Métrica";
                    worksheet.Cells[$"B{row}"].Value = "Valor";
                    ApplyTableHeaderStyle(worksheet.Cells[$"A{row}:B{row}"]);
                    row++;
                    
                    var promedioDias = data.PromedioDiasReservaPago;
                    var metricas = new[]
                    {
                        ("Promedio de Días", $"{promedioDias.PromedioDias:F2} días"),
                        ("Reservas con Pagos", promedioDias.CantidadReservasConPagos.ToString()),
                        ("Tiempo Mínimo", $"{promedioDias.DiasMinimo:F2} días"),
                        ("Tiempo Máximo", $"{promedioDias.DiasMaximo:F2} días")
                    };
                    
                    foreach (var (metrica, valor) in metricas)
                    {
                        worksheet.Cells[$"A{row}"].Value = metrica;
                        worksheet.Cells[$"B{row}"].Value = valor;
                        ApplyDataRowStyle(worksheet.Cells[$"A{row}:B{row}"], row % 2 == 0);
                        row++;
                    }
                    row += 2; // Espacio entre secciones
                }
                
                // SECCIÓN 3: Reservas con Pagos Incompletos
                if (data.ReservasPagosIncompletos != null && data.ReservasPagosIncompletos.Any())
                {
                    worksheet.Cells[$"A{row}"].Value = "RESERVAS CON PAGOS PENDIENTES";
                    worksheet.Cells[$"A{row}:H{row}"].Merge = true;
                    ApplySectionHeaderStyle(worksheet.Cells[$"A{row}:H{row}"]);
                    row++;
                    
                    worksheet.Cells[$"A{row}"].Value = "Reserva ID";
                    worksheet.Cells[$"B{row}"].Value = "Evento";
                    worksheet.Cells[$"C{row}"].Value = "Cliente";
                    worksheet.Cells[$"D{row}"].Value = "Precio Total";
                    worksheet.Cells[$"E{row}"].Value = "Total Pagado";
                    worksheet.Cells[$"F{row}"].Value = "Monto Pendiente";
                    worksheet.Cells[$"G{row}"].Value = "% Pagado";
                    ApplyTableHeaderStyle(worksheet.Cells[$"A{row}:G{row}"]);
                    row++;
                    
                    foreach (var reserva in data.ReservasPagosIncompletos.OrderBy(x => x.PorcentajePagado))
                    {
                        worksheet.Cells[$"A{row}"].Value = reserva.ReservaId;
                        worksheet.Cells[$"B{row}"].Value = reserva.NombreEvento;
                        worksheet.Cells[$"C{row}"].Value = string.IsNullOrEmpty(reserva.ClienteRazonSocial) ? "Sin especificar" : reserva.ClienteRazonSocial;
                        worksheet.Cells[$"D{row}"].Value = reserva.PrecioTotal;
                        worksheet.Cells[$"D{row}"].Style.Numberformat.Format = "\"S/. \"#,##0.00";
                        worksheet.Cells[$"E{row}"].Value = reserva.TotalPagado;
                        worksheet.Cells[$"E{row}"].Style.Numberformat.Format = "\"S/. \"#,##0.00";
                        worksheet.Cells[$"F{row}"].Value = reserva.MontoPendiente;
                        worksheet.Cells[$"F{row}"].Style.Numberformat.Format = "\"S/. \"#,##0.00";
                        worksheet.Cells[$"G{row}"].Value = reserva.PorcentajePagado / 100;
                        worksheet.Cells[$"G{row}"].Style.Numberformat.Format = "0.00%";
                        ApplyDataRowStyle(worksheet.Cells[$"A{row}:G{row}"], row % 2 == 0);
                        row++;
                    }
                    row += 2; // Espacio entre secciones
                }
                
                // SECCIÓN 4: Distribución por Método de Pago
                if (data.TasaUsoMetodoPago != null && data.TasaUsoMetodoPago.Any())
                {
                    worksheet.Cells[$"A{row}"].Value = "DISTRIBUCIÓN POR MÉTODO DE PAGO";
                    worksheet.Cells[$"A{row}:H{row}"].Merge = true;
                    ApplySectionHeaderStyle(worksheet.Cells[$"A{row}:H{row}"]);
                    row++;
                    
                    worksheet.Cells[$"A{row}"].Value = "Método de Pago";
                    worksheet.Cells[$"B{row}"].Value = "Cantidad Usos";
                    worksheet.Cells[$"C{row}"].Value = "Porcentaje Uso";
                    worksheet.Cells[$"D{row}"].Value = "Total Monto";
                    ApplyTableHeaderStyle(worksheet.Cells[$"A{row}:D{row}"]);
                    row++;
                    
                    foreach (var metodo in data.TasaUsoMetodoPago)
                    {
                        worksheet.Cells[$"A{row}"].Value = metodo.TipoPago;
                        worksheet.Cells[$"B{row}"].Value = metodo.CantidadUsos;
                        worksheet.Cells[$"C{row}"].Value = metodo.PorcentajeUso / 100;
                        worksheet.Cells[$"C{row}"].Style.Numberformat.Format = "0.00%";
                        worksheet.Cells[$"D{row}"].Value = metodo.MontoTotal;
                        worksheet.Cells[$"D{row}"].Style.Numberformat.Format = "\"S/. \"#,##0.00";
                        ApplyDataRowStyle(worksheet.Cells[$"A{row}:D{row}"], row % 2 == 0);
                        row++;
                    }
                    row += 2; // Espacio entre secciones
                }
                
                // SECCIÓN 5: Tendencia Mensual de Ingresos
                if (data.TendenciaMensualIngresos != null && data.TendenciaMensualIngresos.Any())
                {
                    worksheet.Cells[$"A{row}"].Value = "TENDENCIA MENSUAL DE INGRESOS";
                    worksheet.Cells[$"A{row}:H{row}"].Merge = true;
                    ApplySectionHeaderStyle(worksheet.Cells[$"A{row}:H{row}"]);
                    row++;
                    
                    worksheet.Cells[$"A{row}"].Value = "Año";
                    worksheet.Cells[$"B{row}"].Value = "Mes";
                    worksheet.Cells[$"C{row}"].Value = "Nombre Mes";
                    worksheet.Cells[$"D{row}"].Value = "Total Ingresos";
                    worksheet.Cells[$"E{row}"].Value = "Número Pagos";
                    worksheet.Cells[$"F{row}"].Value = "Promedio por Pago";
                    ApplyTableHeaderStyle(worksheet.Cells[$"A{row}:F{row}"]);
                    row++;
                    
                    foreach (var ingreso in data.TendenciaMensualIngresos.OrderBy(x => x.Año).ThenBy(x => x.Mes))
                    {
                        worksheet.Cells[$"A{row}"].Value = ingreso.Año;
                        worksheet.Cells[$"B{row}"].Value = ingreso.Mes;
                        worksheet.Cells[$"C{row}"].Value = ingreso.NombreMes;
                        worksheet.Cells[$"D{row}"].Value = ingreso.MontoTotal;
                        worksheet.Cells[$"D{row}"].Style.Numberformat.Format = "\"S/. \"#,##0.00";
                        worksheet.Cells[$"E{row}"].Value = ingreso.CantidadPagos;
                        worksheet.Cells[$"F{row}"].Value = ingreso.MontoPromedio;
                        worksheet.Cells[$"F{row}"].Style.Numberformat.Format = "\"S/. \"#,##0.00";
                        ApplyDataRowStyle(worksheet.Cells[$"A{row}:F{row}"], row % 2 == 0);
                        row++;
                    }
                }
            }
            
            worksheet.Cells.AutoFitColumns();
            worksheet.Column(1).Width = 20; // IDs y nombres
            worksheet.Column(2).Width = 25; // Nombres de eventos
            worksheet.Column(3).Width = 30; // Razón social (puede ser largo)
        }

        private void CreateReservasSheet(ExcelPackage package, ReportesReservasResponse? reservas, DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (reservas == null) return;

            var worksheet = package.Workbook.Worksheets.Add("Reservas");
            int row = 1;

            // Título
            worksheet.Cells[row, 1].Value = "REPORTE DE RESERVAS";
            worksheet.Cells[row, 1, row, 6].Merge = true;
            SetTitleStyle(worksheet.Cells[row, 1, row, 6]);
            row += 2;

            // Período
            var periodoTexto = GetPeriodoTexto(fechaInicio, fechaFin);
            worksheet.Cells[row, 1].Value = $"Período: {periodoTexto}";
            SetSubtitleStyle(worksheet.Cells[row, 1]);
            row += 2;

            // *** SECCIÓN 1: Estados de Reservas (ACTUALIZADA) ***
            worksheet.Cells[row, 1].Value = "ESTADOS DE RESERVAS";
            SetSectionHeaderStyle(worksheet.Cells[row, 1]);
            row++;

            // Headers
            worksheet.Cells[row, 1].Value = "Estado";
            worksheet.Cells[row, 2].Value = "Cantidad";
            worksheet.Cells[row, 3].Value = "Porcentaje";
            SetHeaderStyle(worksheet.Cells[row, 1, row, 3]);
            row++;

            if (reservas.TasaConversionEstado != null)
            {
                var conversion = reservas.TasaConversionEstado;
                var total = conversion.ReservasPendientes + conversion.ReservasConfirmadas + 
                           conversion.ReservasCanceladas + conversion.ReservasFinalizadas;

                // Pendientes
                worksheet.Cells[row, 1].Value = "Pendientes";
                worksheet.Cells[row, 2].Value = conversion.ReservasPendientes;
                worksheet.Cells[row, 3].Value = total > 0 ? (conversion.ReservasPendientes * 100.0 / total).ToString("F1") + "%" : "0%";
                row++;

                // Confirmadas
                worksheet.Cells[row, 1].Value = "Confirmadas";
                worksheet.Cells[row, 2].Value = conversion.ReservasConfirmadas;
                worksheet.Cells[row, 3].Value = total > 0 ? (conversion.ReservasConfirmadas * 100.0 / total).ToString("F1") + "%" : "0%";
                row++;

                // *** NUEVO: Finalizadas ***
                worksheet.Cells[row, 1].Value = "Finalizadas";
                worksheet.Cells[row, 2].Value = conversion.ReservasFinalizadas;
                worksheet.Cells[row, 3].Value = total > 0 ? (conversion.ReservasFinalizadas * 100.0 / total).ToString("F1") + "%" : "0%";
                row++;

                // Canceladas
                worksheet.Cells[row, 1].Value = "Canceladas";
                worksheet.Cells[row, 2].Value = conversion.ReservasCanceladas;
                worksheet.Cells[row, 3].Value = total > 0 ? (conversion.ReservasCanceladas * 100.0 / total).ToString("F1") + "%" : "0%";
                row++;

                // *** NUEVA SECCIÓN: Tasas de conversión ***
                row++;
                worksheet.Cells[row, 1].Value = "TASAS DE CONVERSIÓN";
                SetSectionHeaderStyle(worksheet.Cells[row, 1]);
                row++;

                worksheet.Cells[row, 1].Value = "Tasa Pendiente → Confirmado";
                worksheet.Cells[row, 2].Value = conversion.TasaConversionPendienteConfirmado.ToString("F1") + "%";
                row++;

                worksheet.Cells[row, 1].Value = "Tasa de Cancelación";
                worksheet.Cells[row, 2].Value = conversion.TasaCancelacion.ToString("F1") + "%";
                row++;

                worksheet.Cells[row, 1].Value = "Tasa de Finalización";
                worksheet.Cells[row, 2].Value = conversion.TasaFinalizacion.ToString("F1") + "%";
                row++;
            }

            row += 2;

            // *** SECCIÓN 2: Reservas por Mes ***
            worksheet.Cells[row, 1].Value = "RESERVAS POR MES";
            SetSectionHeaderStyle(worksheet.Cells[row, 1]);
            row++;

            // Headers
            worksheet.Cells[row, 1].Value = "Año";
            worksheet.Cells[row, 2].Value = "Mes";
            worksheet.Cells[row, 3].Value = "Cantidad";
            worksheet.Cells[row, 4].Value = "Monto Total";
            worksheet.Cells[row, 5].Value = "Monto Promedio";
            SetHeaderStyle(worksheet.Cells[row, 1, row, 5]);
            row++;

            foreach (var reservaMes in reservas.ReservasPorMes ?? new List<ReservaPorMes>())
            {
                worksheet.Cells[row, 1].Value = reservaMes.Año;
                worksheet.Cells[row, 2].Value = reservaMes.NombreMes;
                worksheet.Cells[row, 3].Value = reservaMes.CantidadReservas;
                worksheet.Cells[row, 4].Value = reservaMes.MontoTotal;
                worksheet.Cells[row, 5].Value = reservaMes.MontoPromedio;
                
                // Formatear como moneda
                worksheet.Cells[row, 4].Style.Numberformat.Format = "\"S/\"#,##0.00";
                worksheet.Cells[row, 5].Style.Numberformat.Format = "\"S/\"#,##0.00";
                row++;
            }

            row += 2;

            // *** SECCIÓN 3: Ingresos por Tipo de Evento ***
            worksheet.Cells[row, 1].Value = "INGRESOS POR TIPO DE EVENTO";
            SetSectionHeaderStyle(worksheet.Cells[row, 1]);
            row++;

            // Headers
            worksheet.Cells[row, 1].Value = "Tipo de Evento";
            worksheet.Cells[row, 2].Value = "Cantidad Reservas";
            worksheet.Cells[row, 3].Value = "Ingreso Total";
            worksheet.Cells[row, 4].Value = "Ingreso Promedio";
            worksheet.Cells[row, 5].Value = "Ingreso Mínimo";
            worksheet.Cells[row, 6].Value = "Ingreso Máximo";
            SetHeaderStyle(worksheet.Cells[row, 1, row, 6]);
            row++;

            foreach (var evento in reservas.IngresosPromedioPorTipoEvento ?? new List<IngresoPromedioPorTipoEvento>())
            {
                worksheet.Cells[row, 1].Value = evento.TipoEvento;
                worksheet.Cells[row, 2].Value = evento.CantidadReservas;
                worksheet.Cells[row, 3].Value = evento.IngresoTotal;
                worksheet.Cells[row, 4].Value = evento.IngresoPromedio;
                worksheet.Cells[row, 5].Value = evento.IngresoMinimo;
                worksheet.Cells[row, 6].Value = evento.IngresoMaximo;
                
                // Formatear como moneda
                for (int col = 3; col <= 6; col++)
                {
                    worksheet.Cells[row, col].Style.Numberformat.Format = "\"S/\"#,##0.00";
                }
                row++;
            }

            // Ajustar ancho de columnas
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private void CreateServiciosSheet(ExcelPackage package, ReportesServiciosResponse? data)
        {
            var worksheet = package.Workbook.Worksheets.Add("Servicios");
            
            if (data == null)
            {
                worksheet.Cells[1, 1].Value = "No hay datos de servicios disponibles";
                return;
            }
            
            int row = 1;
            
            // SECCIÓN 1: Servicios Más Frecuentes
            worksheet.Cells[row, 1].Value = "SERVICIOS MÁS FRECUENTES";
            SetSectionHeaderStyle(worksheet.Cells[row, 1]);
            row++;
            
            // Headers
            worksheet.Cells[row, 1].Value = "Servicio";
            worksheet.Cells[row, 2].Value = "Cantidad Reservas";
            worksheet.Cells[row, 3].Value = "Porcentaje Uso";
            worksheet.Cells[row, 4].Value = "Ingreso Total";
            worksheet.Cells[row, 5].Value = "Ingreso Promedio";
            SetHeaderStyle(worksheet.Cells[row, 1, row, 5]);
            row++;
            
            foreach (var servicio in data.ServiciosMasFrecuentes ?? new List<ServicioMasFrecuente>())
            {
                worksheet.Cells[row, 1].Value = servicio.NombreServicio;
                worksheet.Cells[row, 2].Value = servicio.CantidadReservas;
                worksheet.Cells[row, 3].Value = servicio.PorcentajeUso / 100; 
                worksheet.Cells[row, 3].Style.Numberformat.Format = "0.00%";
                worksheet.Cells[row, 4].Value = servicio.IngresoTotal;
                worksheet.Cells[row, 4].Style.Numberformat.Format = "\"S/. \"#,##0.00";
                worksheet.Cells[row, 5].Value = servicio.IngresoPromedio;
                worksheet.Cells[row, 5].Style.Numberformat.Format = "\"S/. \"#,##0.00";
                ApplyDataRowStyle(worksheet.Cells[row, 1, row, 5], row % 2 == 0);
                row++;
            }
            
            row += 2;
            
            // SECCIÓN 2: Variación de Ingresos Mensuales por Servicio
            worksheet.Cells[row, 1].Value = "VARIACIÓN DE INGRESOS MENSUALES POR SERVICIO";
            SetSectionHeaderStyle(worksheet.Cells[row, 1]);
            row++;
            
            // Headers
            worksheet.Cells[row, 1].Value = "Servicio";
            worksheet.Cells[row, 2].Value = "Año";
            worksheet.Cells[row, 3].Value = "Mes";
            worksheet.Cells[row, 4].Value = "Monto Mensual";
            worksheet.Cells[row, 5].Value = "Cantidad Reservas";
            worksheet.Cells[row, 6].Value = "Variación %";
            SetHeaderStyle(worksheet.Cells[row, 1, row, 6]);
            row++;
            
            foreach (var variacion in data.VariacionIngresosMensualesServicio ?? new List<VariacionIngresosMensualesServicio>())
            {
                worksheet.Cells[row, 1].Value = variacion.NombreServicio;
                worksheet.Cells[row, 2].Value = variacion.Año;
                worksheet.Cells[row, 3].Value = variacion.NombreMes;
                worksheet.Cells[row, 4].Value = variacion.MontoMensual;
                worksheet.Cells[row, 4].Style.Numberformat.Format = "\"S/. \"#,##0.00";
                worksheet.Cells[row, 5].Value = variacion.CantidadReservas;
                worksheet.Cells[row, 6].Value = variacion.VariacionPorc / 100;
                worksheet.Cells[row, 6].Style.Numberformat.Format = "0.00%";
                ApplyDataRowStyle(worksheet.Cells[row, 1, row, 6], row % 2 == 0);
                row++;
            }
            
            row += 2;
            
            // SECCIÓN 3: Promedio de Items por Servicio
            worksheet.Cells[row, 1].Value = "PROMEDIO DE ITEMS POR SERVICIO";
            SetSectionHeaderStyle(worksheet.Cells[row, 1]);
            row++;
            
            // Headers
            worksheet.Cells[row, 1].Value = "Servicio";
            worksheet.Cells[row, 2].Value = "Promedio Items Usados";
            worksheet.Cells[row, 3].Value = "Total Detalles";
            worksheet.Cells[row, 4].Value = "Cantidad Reservas";
            SetHeaderStyle(worksheet.Cells[row, 1, row, 4]);
            row++;
            
            foreach (var promedio in data.PromedioItemsPorServicio ?? new List<PromedioItemsPorServicio>())
            {
                worksheet.Cells[row, 1].Value = promedio.NombreServicio;
                worksheet.Cells[row, 2].Value = promedio.PromedioItemsUsados;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "0.00";
                worksheet.Cells[row, 3].Value = promedio.TotalDetalles;
                worksheet.Cells[row, 4].Value = promedio.CantidadReservas;
                ApplyDataRowStyle(worksheet.Cells[row, 1, row, 4], row % 2 == 0);
                row++;
            }
            
            row += 2;
            
            // SECCIÓN 4: Servicios Sin Reservas (SIN DÍAS INACTIVOS)
            worksheet.Cells[row, 1].Value = "SERVICIOS SIN RESERVAS";
            SetSectionHeaderStyle(worksheet.Cells[row, 1]);
            row++;
            
            // Headers
            worksheet.Cells[row, 1].Value = "Servicio";
            worksheet.Cells[row, 2].Value = "Descripción";
            worksheet.Cells[row, 3].Value = "Precio Base";
            SetHeaderStyle(worksheet.Cells[row, 1, row, 3]);
            row++;
            
            foreach (var servicio in data.ServiciosSinReservas ?? new List<ServicioSinReserva>())
            {
                worksheet.Cells[row, 1].Value = servicio.NombreServicio;
                worksheet.Cells[row, 2].Value = servicio.Descripcion;
                worksheet.Cells[row, 3].Value = servicio.PrecioBase;
                worksheet.Cells[row, 3].Style.Numberformat.Format = "\"S/. \"#,##0.00";
                ApplyDataRowStyle(worksheet.Cells[row, 1, row, 3], row % 2 == 0);
                row++;
            }
            
            row += 2;
            
            // SECCIÓN 5: Servicios con Eventos Cancelados
            worksheet.Cells[row, 1].Value = "SERVICIOS CON EVENTOS CANCELADOS";
            SetSectionHeaderStyle(worksheet.Cells[row, 1]);
            row++;
            
            // Headers
            worksheet.Cells[row, 1].Value = "Servicio";
            worksheet.Cells[row, 2].Value = "Total Reservas";
            worksheet.Cells[row, 3].Value = "Reservas Canceladas";
            worksheet.Cells[row, 4].Value = "Porcentaje Cancelación";
            worksheet.Cells[row, 5].Value = "Monto Pérdidas";
            SetHeaderStyle(worksheet.Cells[row, 1, row, 5]);
            row++;
            
            foreach (var servicio in data.ServiciosEventosCancelados ?? new List<ServicioEventoCancelado>())
            {
                worksheet.Cells[row, 1].Value = servicio.NombreServicio;
                worksheet.Cells[row, 2].Value = servicio.TotalReservas;
                worksheet.Cells[row, 3].Value = servicio.ReservasCanceladas;
                worksheet.Cells[row, 4].Value = servicio.PorcentajeCancelacion / 100; 
                worksheet.Cells[row, 4].Style.Numberformat.Format = "0.00%";
                worksheet.Cells[row, 5].Value = servicio.MontoPerdidasCancelacion;
                worksheet.Cells[row, 5].Style.Numberformat.Format = "\"S/. \"#,##0.00";
                ApplyDataRowStyle(worksheet.Cells[row, 1, row, 5], row % 2 == 0);
                row++;
            }
            
            // Ajustar ancho de columnas
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private void CreateInventarioSheet(ExcelPackage package, ReportesItemsResponse? data)
        {
            var worksheet = package.Workbook.Worksheets.Add("Inventario");
            
            // Encabezado
            worksheet.Cells["A1"].Value = "REPORTE DE INVENTARIO";
            worksheet.Cells["A1:G1"].Merge = true;
            ApplyHeaderStyle(worksheet.Cells["A1:G1"], Color.FromArgb(220, 20, 60));
            
            int row = 3;
            
            // SECCIÓN 1: Items Más Utilizados
            if (data?.ItemsMasUtilizados != null && data.ItemsMasUtilizados.Any())
            {
                worksheet.Cells[$"A{row}"].Value = "ITEMS MÁS UTILIZADOS";
                worksheet.Cells[$"A{row}:G{row}"].Merge = true;
                ApplySectionHeaderStyle(worksheet.Cells[$"A{row}:G{row}"]);
                row++;
                
                worksheet.Cells[$"A{row}"].Value = "Item";
                worksheet.Cells[$"B{row}"].Value = "Total Cantidad Utilizada";
                worksheet.Cells[$"C{row}"].Value = "Frecuencia de Uso";
                worksheet.Cells[$"D{row}"].Value = "Promedio Uso por Servicio";
                ApplyTableHeaderStyle(worksheet.Cells[$"A{row}:D{row}"]);
                row++;
                
                foreach (var item in data.ItemsMasUtilizados)
                {
                    worksheet.Cells[$"A{row}"].Value = item.NombreItem;
                    worksheet.Cells[$"B{row}"].Value = item.TotalCantidadUtilizada;
                    worksheet.Cells[$"C{row}"].Value = item.FrecuenciaUso;
                    worksheet.Cells[$"D{row}"].Value = item.PromedioUsoPorServicio;
                    ApplyDataRowStyle(worksheet.Cells[$"A{row}:D{row}"], row % 2 == 0);
                    row++;
                }
                row += 2; // Espacio entre secciones
            }
            
            // SECCIÓN 2: Stock Promedio por Tipo de Servicio
            if (data?.StockPromedioPorTipoServicio != null && data.StockPromedioPorTipoServicio.Any())
            {
                worksheet.Cells[$"A{row}"].Value = "STOCK PROMEDIO POR TIPO DE SERVICIO";
                worksheet.Cells[$"A{row}:G{row}"].Merge = true;
                ApplySectionHeaderStyle(worksheet.Cells[$"A{row}:G{row}"]);
                row++;
                
                worksheet.Cells[$"A{row}"].Value = "Servicio";
                worksheet.Cells[$"B{row}"].Value = "Stock Promedio Utilizado";
                worksheet.Cells[$"C{row}"].Value = "Cantidad de Detalles";
                ApplyTableHeaderStyle(worksheet.Cells[$"A{row}:C{row}"]);
                row++;
                
                foreach (var servicio in data.StockPromedioPorTipoServicio)
                {
                    worksheet.Cells[$"A{row}"].Value = servicio.NombreServicio;
                    worksheet.Cells[$"B{row}"].Value = servicio.StockPromedioUtilizado;
                    worksheet.Cells[$"C{row}"].Value = servicio.CantidadDetalles;
                    ApplyDataRowStyle(worksheet.Cells[$"A{row}:C{row}"], row % 2 == 0);
                    row++;
                }
                row += 2; // Espacio entre secciones
            }
            
            // SECCIÓN 3: Tasa de Disponibilidad
            if (data?.TasaDisponibilidad != null && data.TasaDisponibilidad.Any())
            {
                worksheet.Cells[$"A{row}"].Value = "TASA DE DISPONIBILIDAD POR ITEM";
                worksheet.Cells[$"A{row}:G{row}"].Merge = true;
                ApplySectionHeaderStyle(worksheet.Cells[$"A{row}:G{row}"]);
                row++;
                
                worksheet.Cells[$"A{row}"].Value = "Item";
                worksheet.Cells[$"B{row}"].Value = "Stock Total";
                worksheet.Cells[$"C{row}"].Value = "Stock Disponible";
                worksheet.Cells[$"D{row}"].Value = "Stock Ocupado";
                worksheet.Cells[$"E{row}"].Value = "Tasa Disponibilidad";
                ApplyTableHeaderStyle(worksheet.Cells[$"A{row}:E{row}"]);
                row++;
                
                foreach (var item in data.TasaDisponibilidad.OrderBy(x => x.TasaDisponibilidadPorc))
                {
                    worksheet.Cells[$"A{row}"].Value = item.NombreItem;
                    worksheet.Cells[$"B{row}"].Value = item.Stock;
                    worksheet.Cells[$"C{row}"].Value = item.StockDisponible;
                    worksheet.Cells[$"D{row}"].Value = item.Stock - item.StockDisponible; // Stock ocupado
                    worksheet.Cells[$"E{row}"].Value = item.TasaDisponibilidadPorc / 100;
                    worksheet.Cells[$"E{row}"].Style.Numberformat.Format = "0.00%";
                    ApplyDataRowStyle(worksheet.Cells[$"A{row}:E{row}"], row % 2 == 0);
                    row++;
                }
            }
            
            worksheet.Cells.AutoFitColumns();
            worksheet.Column(1).Width = 30; // Item names pueden ser largos
        }

        // Métodos de estilo
        private void ApplyHeaderStyle(ExcelRange range, Color backgroundColor)
        {
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(backgroundColor);
            range.Style.Font.Color.SetColor(Color.White);
            range.Style.Font.Bold = true;
            range.Style.Font.Size = 14;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            range.Style.Border.BorderAround(ExcelBorderStyle.Thick);
        }

        private void ApplySubHeaderStyle(ExcelRange range)
        {
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(242, 242, 242));
            range.Style.Font.Italic = true;
            range.Style.Font.Size = 10;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        private void ApplySectionHeaderStyle(ExcelRange range)
        {
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
            range.Style.Font.Bold = true;
            range.Style.Font.Size = 12;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Style.Border.BorderAround(ExcelBorderStyle.Medium);
        }

        private void ApplyTableHeaderStyle(ExcelRange range)
        {
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
            range.Style.Font.Color.SetColor(Color.White);
            range.Style.Font.Bold = true;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Style.Border.BorderAround(ExcelBorderStyle.Medium);
        }

        private void ApplyDataRowStyle(ExcelRange range, bool isEven)
        {
            if (isEven)
            {
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(248, 248, 248));
            }
            range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        }

        private string GetMonthName(int month)
        {
            return month switch
            {
                1 => "Enero",
                2 => "Febrero",
                3 => "Marzo",
                4 => "Abril",
                5 => "Mayo",
                6 => "Junio",
                7 => "Julio",
                8 => "Agosto",
                9 => "Septiembre",
                10 => "Octubre",
                11 => "Noviembre",
                12 => "Diciembre",
                _ => month.ToString()
            };
        }

        private void SetTitleStyle(ExcelRange range)
        {
            range.Style.Font.Bold = true;
            range.Style.Font.Size = 16;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        }

        private void SetSubtitleStyle(ExcelRange range)
        {
            range.Style.Font.Italic = true;
            range.Style.Font.Size = 12;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        }

        private void SetSectionHeaderStyle(ExcelRange range)
        {
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(191, 191, 191));
            range.Style.Font.Bold = true;
            range.Style.Font.Size = 12;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        }

        private void SetHeaderStyle(ExcelRange range)
        {
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
            range.Style.Font.Color.SetColor(Color.White);
            range.Style.Font.Bold = true;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        }

        private string GetPeriodoTexto(DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (fechaInicio == null && fechaFin == null)
                return "Todo el tiempo";
            else if (fechaInicio != null && fechaFin == null)
                return $"Desde {fechaInicio.Value:dd/MM/yyyy}";
            else if (fechaInicio == null && fechaFin != null)
                return $"Hasta {fechaFin.Value:dd/MM/yyyy}";
            else
                return $"Desde {fechaInicio!.Value:dd/MM/yyyy} hasta {fechaFin!.Value:dd/MM/yyyy}";
        }
    }
}
