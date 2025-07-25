using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using SistemaBiblioteca.Data;
using SistemaBiblioteca.HealthChecks;
using SistemaBiblioteca.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuração do DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<BibliotecaContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

// Configuração do Identity
builder.Services.AddIdentity<Usuario, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
})
.AddEntityFrameworkStores<BibliotecaContext>()
.AddDefaultTokenProviders();

// Configuração do JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtSecret = jwtSettings["Secret"]
    ?? throw new InvalidOperationException("JWT Secret is missing in configuration.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

// Configuração de Autorização
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole(TipoPerfil.Administrador.ToString()));

    options.AddPolicy("RequireBibliotecario", policy =>
        policy.RequireRole(TipoPerfil.Bibliotecario.ToString()));

    options.AddPolicy("RequireUser", policy =>
        policy.RequireRole(TipoPerfil.UsuarioPadrao.ToString()));

    options.AddPolicy("RequireStaff", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole(TipoPerfil.Administrador.ToString()) ||
            context.User.IsInRole(TipoPerfil.Bibliotecario.ToString())));
});

// Configuração de Cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// Configuração de Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configuração de MVC
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// Configuração de Razor Runtime Compilation
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddRazorPages()
        .AddRazorRuntimeCompilation();
}
else
{
    builder.Services.AddRazorPages();
}

// Configuração de Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<BibliotecaContextHealthCheck>(
        "BibliotecaDB",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "database", "sqlserver" });

// Registrar o Health Check customizado
builder.Services.AddSingleton<BibliotecaContextHealthCheck>();

var app = builder.Build();

// Pipeline de requisições HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(
            System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    exception = e.Value.Exception?.Message
                })
            }));
    }
});

// Seed de dados iniciais
await SeedDatabaseAsync(app);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

async Task SeedDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<BibliotecaContext>();
        var userManager = services.GetRequiredService<UserManager<Usuario>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        foreach (var role in Enum.GetNames(typeof(TipoPerfil)))
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminEmail = "admin@biblioteca.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new Usuario
            {
                UserName = adminEmail,
                Email = adminEmail,
                Nome = "Administrador",
                PhoneNumber = "11999999999",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin@1234");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, TipoPerfil.Administrador.ToString());
            }
        }

        if (!context.Livros.Any())
        {
            context.Livros.AddRange(
                new Livro
                {
                    Titulo = "Dom Casmurro",
                    Autor = "Machado de Assis",
                    Editora = "Companhia das Letras",
                    AnoPublicacao = 1899,
                    ISBN = "9788535910663",
                    QuantidadeDisponivel = 5
                },
                new Livro
                {
                    Titulo = "Clean Code",
                    Autor = "Robert C. Martin",
                    Editora = "Alta Books",
                    AnoPublicacao = 2008,
                    ISBN = "9780132350884",
                    QuantidadeDisponivel = 3
                }
            );
            await context.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro ao popular o banco de dados");
    }
}