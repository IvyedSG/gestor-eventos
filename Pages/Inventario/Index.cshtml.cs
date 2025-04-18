using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace gestor_eventos.Pages.Inventario
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string CategoryFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
        
        public List<InventoryItem> InventoryItems { get; set; }
        public bool HasLowStockItems { get; set; }
        
        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }
        
        public async Task OnGetAsync()
        {
            // Ensure we're at least on page 1
            if (CurrentPage < 1)
            {
                CurrentPage = 1;
            }
            
            // In a real application, this would be retrieved from a database
            // This is mock data for demonstration purposes
            await FetchInventoryItems();
            
            // Count total items for pagination after filtering
            TotalItems = InventoryItems.Count;
            
            // Apply pagination
            InventoryItems = InventoryItems
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
            
            // Check if there are items with low stock
            HasLowStockItems = InventoryItems.Any(i => i.CurrentStock <= 0 || i.CurrentStock < i.MinimumStock);
        }
        
        private Task FetchInventoryItems()
        {
            // This would normally be a database call
            // We're creating mock data for demonstration
            var allItems = GenerateMockInventoryData();
            
            // Apply filters
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                allItems = allItems.Where(i => 
                    i.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) || 
                    i.Code.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    i.Notes?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) == true
                ).ToList();
            }
            
            if (!string.IsNullOrWhiteSpace(CategoryFilter))
            {
                allItems = allItems.Where(i => i.Category == CategoryFilter).ToList();
            }
            
            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                if (StatusFilter == "Normal")
                {
                    allItems = allItems.Where(i => i.CurrentStock >= i.MinimumStock).ToList();
                }
                else if (StatusFilter == "Bajo")
                {
                    allItems = allItems.Where(i => i.CurrentStock > 0 && i.CurrentStock < i.MinimumStock).ToList();
                }
                else if (StatusFilter == "Agotado")
                {
                    allItems = allItems.Where(i => i.CurrentStock <= 0).ToList();
                }
            }
            
            InventoryItems = allItems;
            return Task.CompletedTask;
        }
        
        private List<InventoryItem> GenerateMockInventoryData()
        {
            var items = new List<InventoryItem>
            {
                new InventoryItem 
                { 
                    Id = 1, 
                    Name = "Mesa redonda 1.5m", 
                    Code = "MR-001",
                    Category = "Mobiliario", 
                    CurrentStock = 25, 
                    MinimumStock = 10,
                    DailyPrice = 50.00m, 
                    WeeklyPrice = 300.00m, 
                    MonthlyPrice = 1200.00m,
                    Notes = "Mesas de 1.5m de diámetro para 8-10 personas",
                    Location = "Almacén principal"
                },
                new InventoryItem 
                { 
                    Id = 2, 
                    Name = "Mesa rectangular 2x1m", 
                    Code = "MRE-001",
                    Category = "Mobiliario", 
                    CurrentStock = 15, 
                    MinimumStock = 8,
                    DailyPrice = 60.00m, 
                    WeeklyPrice = 350.00m, 
                    MonthlyPrice = 1400.00m,
                    Notes = "Mesas rectangulares para buffet o presidium",
                    Location = "Almacén principal"
                },
                new InventoryItem 
                { 
                    Id = 3, 
                    Name = "Silla Tiffany dorada", 
                    Code = "ST-001",
                    Category = "Mobiliario", 
                    CurrentStock = 150, 
                    MinimumStock = 50,
                    DailyPrice = 15.00m, 
                    WeeklyPrice = 90.00m, 
                    MonthlyPrice = 350.00m,
                    Notes = "Sillas Tiffany doradas para eventos elegantes",
                    Location = "Almacén A"
                },
                new InventoryItem 
                { 
                    Id = 4, 
                    Name = "Mantel blanco redondo", 
                    Code = "MB-001",
                    Category = "Mantelería", 
                    CurrentStock = 35, 
                    MinimumStock = 20,
                    DailyPrice = 8.00m, 
                    WeeklyPrice = 48.00m, 
                    MonthlyPrice = 180.00m,
                    Notes = "Manteles blancos para mesas redondas",
                    Location = "Almacén textiles"
                },
                new InventoryItem 
                { 
                    Id = 5, 
                    Name = "Mantel rectangular beige", 
                    Code = "MRB-001",
                    Category = "Mantelería", 
                    CurrentStock = 18, 
                    MinimumStock = 15,
                    DailyPrice = 10.00m, 
                    WeeklyPrice = 60.00m, 
                    MonthlyPrice = 220.00m,
                    Notes = "Manteles beige para mesas rectangulares",
                    Location = "Almacén textiles"
                },
                new InventoryItem 
                { 
                    Id = 6, 
                    Name = "Cubre silla blanco", 
                    Code = "CSB-001",
                    Category = "Mantelería", 
                    CurrentStock = 120, 
                    MinimumStock = 80,
                    DailyPrice = 3.50m, 
                    WeeklyPrice = 21.00m, 
                    MonthlyPrice = 80.00m,
                    Location = "Almacén textiles"
                },
                new InventoryItem 
                { 
                    Id = 7, 
                    Name = "Centro de mesa cristal", 
                    Code = "CM-001",
                    Category = "Decoración", 
                    CurrentStock = 20, 
                    MinimumStock = 10,
                    DailyPrice = 18.00m, 
                    WeeklyPrice = 100.00m, 
                    MonthlyPrice = 380.00m,
                    Notes = "Centro de mesa de cristal con portavela",
                    Location = "Almacén B"
                },
                new InventoryItem 
                { 
                    Id = 8, 
                    Name = "Candelabro plateado 5 velas", 
                    Code = "CP-001",
                    Category = "Decoración", 
                    CurrentStock = 8, 
                    MinimumStock = 5,
                    DailyPrice = 25.00m, 
                    WeeklyPrice = 150.00m, 
                    MonthlyPrice = 550.00m,
                    Notes = "Candelabros plateados de 5 brazos para mesas principales",
                    Location = "Almacén B"
                },
                new InventoryItem 
                { 
                    Id = 9, 
                    Name = "Altavoz amplificado 15\"", 
                    Code = "AA-001",
                    Category = "Audio y Video", 
                    CurrentStock = 4, 
                    MinimumStock = 4,
                    DailyPrice = 80.00m, 
                    WeeklyPrice = 480.00m, 
                    MonthlyPrice = 1800.00m,
                    Notes = "Altavoces amplificados de 15 pulgadas con trípode",
                    Location = "Almacén AV"
                },
                new InventoryItem 
                { 
                    Id = 10, 
                    Name = "Micrófono inalámbrico", 
                    Code = "MI-001",
                    Category = "Audio y Video", 
                    CurrentStock = 3, 
                    MinimumStock = 4,
                    DailyPrice = 35.00m, 
                    WeeklyPrice = 200.00m, 
                    MonthlyPrice = 750.00m,
                    Notes = "Micrófonos inalámbricos profesionales",
                    Location = "Almacén AV"
                },
                new InventoryItem 
                { 
                    Id = 11, 
                    Name = "Proyector 5000 lúmenes", 
                    Code = "PR-001",
                    Category = "Audio y Video", 
                    CurrentStock = 2, 
                    MinimumStock = 2,
                    DailyPrice = 120.00m, 
                    WeeklyPrice = 700.00m, 
                    MonthlyPrice = 2500.00m,
                    Notes = "Proyector de alta luminosidad para eventos",
                    Location = "Almacén AV"
                },
                new InventoryItem 
                { 
                    Id = 12, 
                    Name = "Copa vino tinto", 
                    Code = "CVT-001",
                    Category = "Vajilla", 
                    CurrentStock = 180, 
                    MinimumStock = 120,
                    DailyPrice = 1.20m, 
                    WeeklyPrice = 7.00m, 
                    MonthlyPrice = 25.00m,
                    Notes = "Copas de cristal para vino tinto",
                    Location = "Almacén C"
                },
                new InventoryItem 
                { 
                    Id = 13, 
                    Name = "Plato base dorado", 
                    Code = "PB-001",
                    Category = "Vajilla", 
                    CurrentStock = 150, 
                    MinimumStock = 100,
                    DailyPrice = 2.00m, 
                    WeeklyPrice = 12.00m, 
                    MonthlyPrice = 45.00m,
                    Notes = "Platos base dorados decorativos",
                    Location = "Almacén C"
                },
                new InventoryItem 
                { 
                    Id = 14, 
                    Name = "Cubiertos premium (set)", 
                    Code = "CP-001",
                    Category = "Vajilla", 
                    CurrentStock = 80, 
                    MinimumStock = 120,
                    DailyPrice = 3.50m, 
                    WeeklyPrice = 20.00m, 
                    MonthlyPrice = 75.00m,
                    Notes = "Set de cubiertos premium (cuchillo, tenedor, cuchara)",
                    Location = "Almacén C"
                },
                new InventoryItem 
                { 
                    Id = 15, 
                    Name = "Par LED RGBW", 
                    Code = "PL-001",
                    Category = "Iluminación", 
                    CurrentStock = 12, 
                    MinimumStock = 8,
                    DailyPrice = 35.00m, 
                    WeeklyPrice = 200.00m, 
                    MonthlyPrice = 750.00m,
                    Notes = "Luces LED RGBW para ambientación de eventos",
                    Location = "Almacén AV"
                },
                new InventoryItem 
                { 
                    Id = 16, 
                    Name = "Máquina de humo", 
                    Code = "MH-001",
                    Category = "Iluminación", 
                    CurrentStock = 3, 
                    MinimumStock = 2,
                    DailyPrice = 45.00m, 
                    WeeklyPrice = 250.00m, 
                    MonthlyPrice = 900.00m,
                    Notes = "Máquina de humo profesional con control remoto",
                    Location = "Almacén AV"
                },
                new InventoryItem 
                { 
                    Id = 17, 
                    Name = "Columna decorativa romana", 
                    Code = "CDR-001",
                    Category = "Decoración", 
                    CurrentStock = 6, 
                    MinimumStock = 6,
                    DailyPrice = 40.00m, 
                    WeeklyPrice = 240.00m, 
                    MonthlyPrice = 900.00m,
                    Notes = "Columnas decorativas estilo romano para ceremonias",
                    Location = "Almacén D"
                },
                new InventoryItem 
                { 
                    Id = 18, 
                    Name = "Servilleta de tela blanca", 
                    Code = "STB-001",
                    Category = "Mantelería", 
                    CurrentStock = 250, 
                    MinimumStock = 200,
                    DailyPrice = 1.00m, 
                    WeeklyPrice = 6.00m, 
                    MonthlyPrice = 22.00m,
                    Notes = "Servilletas de tela blanca de alta calidad",
                    Location = "Almacén textiles"
                },
                new InventoryItem 
                { 
                    Id = 19, 
                    Name = "Pantalla de proyección 2x2m", 
                    Code = "PP-001",
                    Category = "Audio y Video", 
                    CurrentStock = 0, 
                    MinimumStock = 2,
                    DailyPrice = 50.00m, 
                    WeeklyPrice = 300.00m, 
                    MonthlyPrice = 1100.00m,
                    Notes = "Pantalla de proyección con trípode",
                    Location = "Almacén AV"
                },
                new InventoryItem 
                { 
                    Id = 20, 
                    Name = "Jarrón de cristal alto", 
                    Code = "JCA-001",
                    Category = "Decoración", 
                    CurrentStock = 8, 
                    MinimumStock = 12,
                    DailyPrice = 15.00m, 
                    WeeklyPrice = 90.00m, 
                    MonthlyPrice = 340.00m,
                    Notes = "Jarrones altos de cristal para arreglos florales",
                    Location = "Almacén B"
                }
            };
            
            return items;
        }

        // Actions
        public async Task<IActionResult> OnPostSaveItemAsync([FromBody] InventoryItem item)
        {
            // In a real application, this would save to a database
            _logger.LogInformation($"Saving item: {item.Name}");
            
            // Simulate successful save
            await Task.Delay(100);
            
            return new JsonResult(new { success = true, message = "Ítem guardado correctamente" });
        }
        
        public async Task<IActionResult> OnPostUpdateStockAsync(int id, int newStock)
        {
            // In a real application, this would update the database
            _logger.LogInformation($"Updating stock for item {id} to {newStock}");
            
            // Simulate successful update
            await Task.Delay(100);
            
            return new JsonResult(new { success = true, message = "Stock actualizado correctamente" });
        }
        
        public async Task<IActionResult> OnPostUpdatePriceAsync(int id, string priceType, decimal newPrice)
        {
            // In a real application, this would update the database
            _logger.LogInformation($"Updating {priceType} for item {id} to {newPrice}");
            
            // Simulate successful update
            await Task.Delay(100);
            
            return new JsonResult(new { success = true, message = "Precio actualizado correctamente" });
        }
        
        public async Task<IActionResult> OnPostDeleteItemAsync(int id)
        {
            // In a real application, this would delete from the database
            _logger.LogInformation($"Deleting item {id}");
            
            // Simulate successful delete
            await Task.Delay(100);
            
            return new JsonResult(new { success = true, message = "Ítem eliminado correctamente" });
        }
        
        public async Task<IActionResult> OnPostAssignItemAsync(int itemId, int reservationId, int quantity, 
            string period, DateTime startDate, DateTime endDate, string notes)
        {
            // In a real application, this would create an assignment in the database
            _logger.LogInformation($"Assigning {quantity} units of item {itemId} to reservation {reservationId}");
            
            // Simulate successful assignment
            await Task.Delay(100);
            
            return new JsonResult(new { success = true, message = "Ítem asignado correctamente" });
        }
        
        public async Task<IActionResult> OnGetItemDetailsAsync(int id)
        {
            // In a real application, this would fetch from the database
            _logger.LogInformation($"Fetching details for item {id}");
            
            // For demonstration, find the item in our mock data
            var allItems = GenerateMockInventoryData();
            var item = allItems.FirstOrDefault(i => i.Id == id);
            
            if (item == null)
            {
                return NotFound();
            }
            
            // Simulate successful retrieval
            await Task.Delay(100);
            
            return new JsonResult(item);
        }
    }
    
    public class InventoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Category { get; set; }
        public string Location { get; set; }
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public decimal DailyPrice { get; set; }
        public decimal WeeklyPrice { get; set; }
        public decimal MonthlyPrice { get; set; }
        public string Notes { get; set; }
    }
}