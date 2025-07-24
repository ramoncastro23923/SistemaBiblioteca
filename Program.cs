using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaBiblioteca.Data;
using SistemaBiblioteca.Models;

var builder = WebApplication.CreateBuilder(args);

// Configura��o do DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BibliotecaContext>(options =>
    options.UseSqlServer(connectionString));

// Configura��o do Identity com pol�ticas personalizadas
builder.Services.AddIdentity<Usuario, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddEntityFrameworkStores<BibliotecaContext>()
.AddDefaultTokenProviders();

// Configura��o de autoriza��o
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole(TipoPerfil.Administrador.ToString()));
    options.AddPolicy("RequireBibliotecario", policy =>
        policy.RequireRole(TipoPerfil.Bibliotecario.ToString()));
});

// Servi�os adicionais
builder.Services.AddScoped<UserManager<Usuario>>();
builder.Services.AddScoped<SignInManager<Usuario>>();
builder.Services.AddScoped<RoleManager<IdentityRole>>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Pipeline de requisi��es
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

// Seed inicial de roles e admin
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<Usuario>>();

        // Cria roles
        foreach (var role in Enum.GetNames(typeof(TipoPerfil)))
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Cria admin padr�o
        const string adminEmail = "admin@biblioteca.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new Usuario
            {
                UserName = adminEmail,
                Email = adminEmail,
                Nome = "Administrador",
                PhoneNumber = "11999999999",
                Perfil = TipoPerfil.Administrador
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
        logger.LogError(ex, "Erro ao inicializar roles e usu�rio admin");
    }
}

// Rotas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();