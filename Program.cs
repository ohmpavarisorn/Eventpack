using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using EventPack.Models.Db;
using EventPack.Utilities;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<Csi402projectContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("DefaultConnection")
        )
    )
);

var app = builder.Build();

// Create restaurant database tables if they don't exist
try
{
    await DatabaseInitializer.ExecuteSqlScriptAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"⚠ Warning: Could not initialize tables: {ex.Message}");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Add Session Middleware
app.UseSession();

app.UseRouting();

app.UseAuthorization();

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Csi402projectContext>();
    
    // Add MenuItemsJson column if it doesn't exist
    try
    {
        context.Database.ExecuteSqlRaw(@"
            ALTER TABLE bookings 
            ADD COLUMN MenuItemsJson longtext CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL
        ");
    }
    catch
    {
        // Column likely already exists, ignore error
    }
    
    // Seed MenuItems if empty
    if (!context.MenuItems.Any())
    {
        context.MenuItems.AddRange(
            new MenuItem
            {
                MenuItemId = "M001",
                ItemName = "ซูชิแซลมอน",
                Category = "ซูชิ",
                BasePrice = 120m
            },
            new MenuItem
            {
                MenuItemId = "M002",
                ItemName = "โรลแซลมอน",
                Category = "ซูชิ",
                BasePrice = 150m
            },
            new MenuItem
            {
                MenuItemId = "M003",
                ItemName = "ซุปมิโซะ",
                Category = "ซุป",
                BasePrice = 50m
            },
            new MenuItem
            {
                MenuItemId = "M004",
                ItemName = "สลัดผัก",
                Category = "สลัด",
                BasePrice = 80m
            }
        );
        context.SaveChanges();
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();