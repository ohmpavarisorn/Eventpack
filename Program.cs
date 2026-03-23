using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using EventPack.Models.Db;

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
    context.Database.EnsureCreated();
    
    // Seed Foodpackages if empty
    if (!context.Foodpackages.Any())
    {
        context.Foodpackages.AddRange(
            new Foodpackage 
            { 
                PackageId = "PKG001", 
                PackageName = "Signature Sushi", 
                Description = "ซูชิรวม (Nigiri), โรลแซลมอน, ซุปมิโซะ, สลัดผัก - ราคาประหยัดที่สุด จัดการง่าย",
                PricePerPerson = 890m,
                MinGuests = 10
            },
            new Foodpackage 
            { 
                PackageId = "PKG002", 
                PackageName = "Healthy Ocean", 
                Description = "สลัดอโวคาโดกุ้งสด, ปลาหิมะย่างเกลือ, ข้าวญี่ปุ่นธัญพืช, ผลไม้สด - เจาะกลุ่มคนรักสุขภาพ",
                PricePerPerson = 950m,
                MinGuests = 8
            },
            new Foodpackage 
            { 
                PackageId = "PKG003", 
                PackageName = "Grand Sashimi", 
                Description = "เซตซาซิมิรวมพรีเมียม (แซลมอน, ฮามาจิ, ทูน่า, หอยเชลล์), หัวปลาต้มซีอิ๊ว, เทมปุระรวม - ดูหรูหรา ถ่ายรูปสวย",
                PricePerPerson = 1500m,
                MinGuests = 10
            },
            new Foodpackage 
            { 
                PackageId = "PKG004", 
                PackageName = "Wagyu & Sushi Party", 
                Description = "ซูชิเนื้อวากิวเบิร์นไฟ, สเต็กเนื้อวากิว A4, ยำสาหร่าย, ข้าวหน้าเนื้อ - สำหรับคนรักเนื้อ",
                PricePerPerson = 1890m,
                MinGuests = 15
            },
            new Foodpackage 
            { 
                PackageId = "PKG005", 
                PackageName = "Ultimate Omakase Home", 
                Description = "จัดตามใจเชฟ โดยมีวัตถุดิบตามฤดูกาล 12-15 รายการ - พรีเมียมที่สุด เชฟจะไปปั้นให้ดูต่อหน้าทีละคำ",
                PricePerPerson = 2500m,
                MinGuests = 20
            }
        );
        context.SaveChanges();
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();