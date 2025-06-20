using InfertilityApp.Models;
using InfertilityApp.DataAccessLayer.Interfaces;
using InfertilityApp.DataAccessLayer.Repositories;
using InfertilityApp.BusinessLogicLayer.Interfaces;
using InfertilityApp.BusinessLogicLayer.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký Data Access Layer
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Đăng ký Business Logic Layer
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<ITreatmentService, TreatmentService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();
builder.Services.AddScoped<IMedicationService, MedicationService>();
builder.Services.AddScoped<IPartnerService, PartnerService>();
builder.Services.AddScoped<IProcedureService, ProcedureService>();
builder.Services.AddScoped<ITreatmentStageService, TreatmentStageService>();
builder.Services.AddScoped<IUserService, UserService>();

// Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure HTTPS redirection
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status301MovedPermanently;
    options.HttpsPort = 7160;
});

var app = builder.Build();

// Seed default admin user
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
    
    // Ensure database is created
    context.Database.EnsureCreated();
    
    // Check if admin user exists
    var adminUser = await userService.GetUserByUsernameAsync("admin");
    if (adminUser == null)
    {
        // Create default admin user
        var admin = new User
        {
            Username = "admin",
            PasswordHash = "123456", // Will be hashed by service
            FullName = "Quản trị viên hệ thống",
            Email = "admin@infertility.com",
            PhoneNumber = "0123456789",
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.Now
        };
        
        await userService.CreateUserAsync(admin);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
