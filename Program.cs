using Microsoft.AspNetCore.Authentication.Cookies;
using GestorEventos.Services;
using gestor_eventos.Services; 

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddRazorPages();
builder.Services.AddControllers(); 


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpClient();

builder.Services.AddHttpContextAccessor();

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

builder.Services.AddSingleton(new ApiSettings { BaseUrl = "http://localhost:5280" });

builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<ServicioService>();
builder.Services.AddHttpClient<InventoryService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<ReservacionService>(); 
builder.Services.AddScoped<PagoService>(); 
builder.Services.AddHttpClient<ReportesService>();
builder.Services.AddScoped<ExcelExportService>();

builder.Services.AddAuthentication(options => 
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

builder.Services.AddAntiforgery(options => {
    options.HeaderName = "RequestVerificationToken";
});

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");

    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
