using Microsoft.Extensions.Options;
using UrlShortener_TechAssessment.DataAccess;
using UrlShortener_TechAssessment.Models;
using UrlShortener_TechAssessment.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<SiteUrlStoreDatabaseSettings>(builder.Configuration.GetSection("SiteUrlStoreDatabase"));

builder.Services.AddSingleton<IDatabaseSettings>(s => s.GetRequiredService<IOptions<SiteUrlStoreDatabaseSettings>>().Value);
builder.Services.AddSingleton<ISiteUrlDBContext, SiteUrlDBContext>();
builder.Services.AddSingleton<ISiteUrlRepository, SiteUrlRepository>();
builder.Services.AddSingleton<IShortUrlsService, ShortUrlsService>();


builder.Services.AddControllersWithViews();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
