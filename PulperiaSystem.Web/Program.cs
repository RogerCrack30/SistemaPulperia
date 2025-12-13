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
// Inicializar la cadena de conexión estática para los Repositorios Legacy
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    // LOGGING DE DEPURACIÓN (Sanitized)
    // Esto nos mostrará en los logs de Render EXÁCTAMENTE qué está leyendo (sin mostrar la clave real)
    var debugString = System.Text.RegularExpressions.Regex.Replace(connectionString, "Password=.*?;", "Password=HIDDEN;");
    Console.WriteLine($"[DIAGNOSTIC] Loaded Connection String: '{debugString}'");

    PulperiaSystem.DataAccess.SqlHelper.ConnectionString = connectionString;
}
else
{
    Console.WriteLine("[DIAGNOSTIC] CRITICAL: Connection String 'DefaultConnection' is MISSING or EMPTY.");
}

app.Run();
