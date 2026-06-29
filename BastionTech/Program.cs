using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 🌐 REGISTRAR NUESTRO SERVICIO CENTRALIZADO DE SUPABASE
// .NET le pasará automáticamente la configuración (IConfiguration) al servicio
builder.Services.AddSingleton<BastionTech.Services.SupabaseDataService>();

// 🛡️ CONFIGURAR AUTENTICACIÓN POR COOKIES DE .NET
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // La sesión dura 7 días
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseRouting();

// 🛡️ IMPORTANTE: UseAuthentication debe ir ANTES de UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tienda}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();