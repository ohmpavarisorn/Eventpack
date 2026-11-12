using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using EventPack.Models.Db;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddScoped<IPasswordHasher<Userinformation>, PasswordHasher<Userinformation>>();

// Add Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    if (!builder.Environment.IsDevelopment()) options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

var provider = builder.Configuration["Database:Provider"] ?? "MySQL";
var connection = builder.Configuration.GetConnectionString("DefaultConnection") ?? builder.Configuration["DATABASE_URL"];
if (string.IsNullOrWhiteSpace(connection))
    throw new InvalidOperationException("Set ConnectionStrings__DefaultConnection for EventPack.");

if (provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
{
    if (Uri.TryCreate(connection, UriKind.Absolute, out var uri) && uri.Scheme is "postgres" or "postgresql")
    {
        var credentials = uri.UserInfo.Split(':', 2);
        if (credentials.Length != 2) throw new InvalidOperationException("DATABASE_URL requires a username and password.");
        var postgres = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
            Username = Uri.UnescapeDataString(credentials[0]),
            Password = Uri.UnescapeDataString(credentials[1]),
            SslMode = SslMode.Require,
            MaxPoolSize = 8
        };
        if (builder.Configuration["Database:RootCertificate"] is { Length: > 0 } certificate)
        {
            postgres.RootCertificate = certificate;
            postgres.SslMode = SslMode.VerifyFull;
        }
        connection = postgres.ConnectionString;
    }
    builder.Services.AddDbContext<Csi402projectContext>(options => options.UseNpgsql(connection, pg => pg.EnableRetryOnFailure()));
}
else if (provider.Equals("MySQL", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<Csi402projectContext>(options =>
        options.UseMySql(connection, ServerVersion.AutoDetect(connection), mysql => mysql.EnableRetryOnFailure()));
}
else
{
    throw new InvalidOperationException("Set Database__Provider to MySQL or PostgreSQL.");
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Render terminates HTTPS before proxying requests to Kestrel.
if (!app.Configuration.GetValue<bool>("RENDER")) app.UseHttpsRedirection();
app.UseStaticFiles();

// Add Session Middleware
app.UseSession();

app.UseRouting();

app.UseAuthorization();

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Csi402projectContext>();
    
    await context.Database.EnsureCreatedAsync();
    var adminId = app.Configuration["Admin:UserId"];
    var adminPassword = app.Configuration["Admin:InitialPassword"];
    if (!string.IsNullOrWhiteSpace(adminId) && !string.IsNullOrEmpty(adminPassword))
    {
        if (adminId.Length > 10 || adminPassword.Length < 12)
            throw new InvalidOperationException("Set an admin ID of at most 10 characters and a password of at least 12 characters.");
        var admin = await context.Userinformations.FindAsync(adminId);
        if (admin is null)
        {
            admin = new Userinformation { UserId = adminId, FullName = "EventPack Admin", Role = "admin" };
            admin.UserPassword = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Userinformation>>().HashPassword(admin, adminPassword);
            context.Userinformations.Add(admin);
        }
        else if (admin.Role != "admin")
        {
            throw new InvalidOperationException("Admin ID is already in use by a customer.");
        }
    }
    
    // Seed MenuItems if empty
    if (!await context.MenuItems.AnyAsync())
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
    }
    await context.SaveChangesAsync();
}

app.MapGet("/healthz", () => Results.Ok());

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
