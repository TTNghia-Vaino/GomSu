using Microsoft.EntityFrameworkCore;
using GomSu.Models;
using GomSu.Services;

var builder = WebApplication.CreateBuilder(args);

// Load Configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Get Connection String from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("GomsuContext");
Console.WriteLine($"Connection String: {connectionString ?? "Not found"}");

// Configure DbContext with correct connection string
builder.Services.AddDbContext<GomsuContext>(options =>
    options.UseSqlServer(connectionString));

// Configure Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});

// Add Controllers and Views
builder.Services.AddControllersWithViews();

// Configure Email Service
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<EmailService>();

var app = builder.Build();

// Configure Middleware Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Ensure Session Middleware is added before Authorization

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
