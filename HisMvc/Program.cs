using HisMvc.Data;
using HisMvc.Middlewares;
using HisMvc.Services;
using HisMvc.Services.Workflow;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Bắt buộc: chi SQL Server (localhost), không LocalDB
var sqlConnectionString = DatabaseConnectionGuard.RequireSqlServerConnectionString(builder.Configuration);

// Configure Services
builder.Services.AddControllersWithViews();

// Database Configuration — du lieu chi tồn tại tren SQL Server
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(sqlConnectionString)
       .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

// Register custom services
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient("Gemini", client => client.Timeout = TimeSpan.FromSeconds(45));
builder.Services.AddScoped<DepartmentMaintenanceService>();
builder.Services.AddScoped<IAppointmentCancellationService, AppointmentCancellationService>();
builder.Services.AddHostedService<AppointmentCleanupService>();
builder.Services.AddScoped<IPublicAppointmentService, PublicAppointmentService>();
builder.Services.AddScoped<HisMvc.Services.Chatbot.IChatbotService, HisMvc.Services.Chatbot.ChatbotService>();
builder.Services.AddScoped<HisMvc.Services.Chatbot.IChatbotFlowService, HisMvc.Services.Chatbot.ChatbotFlowService>();
builder.Services.AddScoped<InsuranceService>();
builder.Services.AddScoped<CurrentStaffService>();
builder.Services.AddScoped<Icd10Service>();

// Luong KCB — moi buoc 1 class, facade 1 class
builder.Services.AddScoped<ReceptionWorkflowStep>();
builder.Services.AddScoped<CashierWorkflowStep>();
builder.Services.AddScoped<DoctorWorkflowStep>();
builder.Services.AddScoped<LabWorkflowStep>();
builder.Services.AddScoped<PharmacyWorkflowStep>();
builder.Services.AddScoped<OutpatientWorkflowService>();

// View services (doc du lieu cho tung Area)
builder.Services.AddScoped<HisMvc.Areas.Cashier.Services.CashierViewService>();
builder.Services.AddScoped<HisMvc.Areas.Doctor.Services.DoctorViewService>();
builder.Services.AddScoped<HisMvc.Areas.Pharmacy.Services.PharmacyViewService>();
builder.Services.AddScoped<HisMvc.Areas.Reception.Services.ReceptionViewService>();
builder.Services.AddScoped<HisMvc.Areas.Lab.Services.LabViewService>();
builder.Services.AddScoped<HisMvc.Areas.Admin.Services.AdminViewService>();
builder.Services.AddScoped<HisMvc.Areas.Inpatient.Services.InpatientViewService>();

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
            var db = services.GetRequiredService<AppDbContext>();
            await DatabaseConnectionGuard.EnsureCanConnectAsync(db, logger);
            logger.LogInformation("Initializing database on SQL Server...");
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

// Không có SQL Server => không phuc vu trang có dữ liệu (503)
app.UseMiddleware<SqlServerRequiredMiddleware>();

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
