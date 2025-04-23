using Microsoft.AspNetCore.Authentication.Cookies;
using GestorEventos.Services;
using gestor_eventos.Services; 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers(); // Agregar esto para soporte de API controllers

// Agregar soporte para sesiones
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Agregar HttpClient a los servicios
builder.Services.AddHttpClient();

// Agregar HttpContextAccessor para acceder al contexto en los servicios
builder.Services.AddHttpContextAccessor();

// Registrar la configuración de la API
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
// Si no hay configuración en appsettings.json, registramos los valores por defecto
builder.Services.AddSingleton(new ApiSettings { BaseUrl = "http://localhost:5280" });

// Registrar el servicio de clientes
builder.Services.AddScoped<ClienteService>();

// Registrar el servicio de reservaciones
builder.Services.AddHttpClient<ReservacionService>();



// Configura la autenticación por cookies
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers(); // Agregar esta línea para mapear los controllers

app.Run();
