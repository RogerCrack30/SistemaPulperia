using PulperiaSystem.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();

// IMPORTANT: Forwarded Headers for Render/Linux Proxy compatibility
// This ensures Blazor/SignalR knows it's running on HTTPS even behind a proxy
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


// Inicializar la cadena de conexión estática para los Repositorios Legacy
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// CRITICO PARA RENDER:
// Intentar leer la variable de entorno "DefaultConnection" directamente.
// Esto es necesario porque en Render definimos la variable como raíz, no bajo "ConnectionStrings__".
var envConnectionString = Environment.GetEnvironmentVariable("DefaultConnection");
if (!string.IsNullOrEmpty(envConnectionString))
{
    connectionString = envConnectionString;
}

if (!string.IsNullOrEmpty(connectionString))
{
    PulperiaSystem.DataAccess.SqlHelper.ConnectionString = connectionString;
}

app.Run();
