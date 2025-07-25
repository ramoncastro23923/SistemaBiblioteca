using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaBiblioteca.Data;
using SistemaBiblioteca.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DbContext Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BibliotecaContext>(options =>
    options.UseSqlServer(connectionString));

// Identity Configuration
builder.Services.AddIdentity<Usuario, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<BibliotecaContext>()
.AddDefaultTokenProviders();

// JWT Authentication - Updated to match your appsettings.json
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
if (string.IsNullOrEmpty(jwtSettings["Secret"]))
{
    throw new ApplicationException("JWT Secret is missing in configuration.");
}

var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole(TipoPerfil.Administrador.ToString()));
    options.AddPolicy("RequireBibliotecario", policy =>
        policy.RequireRole(TipoPerfil.Bibliotecario.ToString()));
    options.AddPolicy("RequireUser", policy =>
        policy.RequireRole(TipoPerfil.UsuarioPadrao.ToString())); // Fixed from UsuarioPadrao to Usuario
});

// Additional Services
builder.Services.AddControllersWithViews();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddRazorPages();

var app = builder.Build();

// Pipeline Configuration
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Seed Initial Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<Usuario>>();

        // Create Roles
        foreach (var role in Enum.GetNames(typeof(TipoPerfil)))
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Create Admin User
        const string adminEmail = "admin@biblioteca.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new Usuario
            {
                UserName = adminEmail,
                Email = adminEmail,
                Nome = "Administrador",
                PhoneNumber = "11999999999",
                Perfil = TipoPerfil.Administrador,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, admin.Perfil.ToString());
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error seeding database");
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();