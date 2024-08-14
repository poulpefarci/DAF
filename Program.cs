using DinkToPdf;
using Microsoft.EntityFrameworkCore;
using SiteDaf.Models;

var builder = WebApplication.CreateBuilder(args);
// ajouter dink to pdf
builder.Services.AddSingleton(typeof(DinkToPdf.Contracts.IConverter), new SynchronizedConverter(new PdfTools()));

// Ajouter la configuration pour DAFContext
builder.Services.AddDbContext<DAFContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Ajouter les services MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configurer le pipeline des requêtes HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
