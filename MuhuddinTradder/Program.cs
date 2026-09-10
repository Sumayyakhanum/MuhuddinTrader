using Microsoft.EntityFrameworkCore;
using MTDBMVC.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<MTDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MTConnection")
    ));

var app = builder.Build();

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
    pattern: "{controller=MT}/{action=Dashboard}/{id?}");

app.Run();