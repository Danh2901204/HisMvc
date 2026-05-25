using HisMvc.Data;
using HisMvc.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure Services
builder.Services.AddControllersWithViews();

// Database Configuration
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Register custom services
builder.Services.AddScoped<InsuranceService>();

// Identity Configuration
builder.Services
    .AddIdentity<AppUser, IdentityRole>(opt =>
    {
        opt.Password.RequiredLength = 6;
        opt.Password.RequireNonAlphanumeric = false;
        opt.Password.RequireUppercase = false;
        opt.Password.RequireLowercase = false;
        opt.Password.RequireDigit = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Cookie Configuration
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Account/Login";
    opt.AccessDeniedPath = "/Account/Login";
});

// CORS Configuration for API Portal
var portalOrigin = builder.Configuration["Portal:AllowedOrigin"] ?? "http://localhost:3000";
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("Portal", p =>
        p.WithOrigins(portalOrigin).AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// Initialize Database with Seed Data
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        try
        {
            logger.LogInformation("Initializing database...");
            await SeedData.InitializeAsync(services);
            logger.LogInformation("Database initialized successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "Application failed to start due to database initialization error.");
    throw;
}

// Configure Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// CORS must be placed after UseRouting and before UseAuthentication
app.UseCors("Portal");

app.UseAuthentication();
app.UseAuthorization();

// Configure Routing
app.MapControllers();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
