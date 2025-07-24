using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaBiblioteca.Models;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaBiblioteca.Data
{
    public class DataSeeder : IDataSeeder
    {
        private readonly BibliotecaContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(
            BibliotecaContext context,
            UserManager<Usuario> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<DataSeeder> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando a inicialização do banco de dados...");

                await _context.Database.EnsureCreatedAsync();

                await SeedRolesAsync();
                await SeedAdminUserAsync();
                await SeedSampleBooksAsync();

                _logger.LogInformation("Inicialização do banco de dados concluída com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro durante a inicialização do banco de dados");
                throw; // Re-throw para que o aplicativo saiba que houve falha
            }
        }

        private async Task SeedRolesAsync()
        {
            string[] roles = { "Administrador", "Usuario" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    _logger.LogInformation($"Criando role: {role}");
                    var result = await _roleManager.CreateAsync(new IdentityRole(role));

                    if (!result.Succeeded)
                    {
                        _logger.LogError($"Falha ao criar role {role}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }

        private async Task SeedAdminUserAsync()
        {
            const string adminEmail = "admin@biblioteca.com";

            var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                _logger.LogInformation("Criando usuário administrador...");

                var admin = new Usuario
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Nome = "Administrador",
                    EmailConfirmed = true,
                    PhoneNumber = "+5511999999999",
                    PhoneNumberConfirmed = true,
                    Perfil = TipoPerfil.Administrador
                };

                var creationResult = await _userManager.CreateAsync(admin, "Admin@123");

                if (creationResult.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(admin, "Administrador");

                    if (!roleResult.Succeeded)
                    {
                        _logger.LogError($"Falha ao adicionar role ao admin: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    _logger.LogError($"Falha ao criar usuário admin: {string.Join(", ", creationResult.Errors.Select(e => e.Description))}");
                }
            }
        }

        private async Task SeedSampleBooksAsync()
        {
            if (!await _context.Livros.AnyAsync())
            {
                _logger.LogInformation("Adicionando livros de exemplo...");

                var books = new[]
                {
                    new Livro
                    {
                        Titulo = "Dom Casmurro",
                        Autor = "Machado de Assis",
                        Editora = "Editora Martin Claret",
                        AnoPublicacao = 1899,
                        ISBN = "9788572326979",
                        QuantidadeDisponivel = 5
                    },
                    new Livro
                    {
                        Titulo = "O Senhor dos Anéis",
                        Autor = "J.R.R. Tolkien",
                        Editora = "Editora Martins Fontes",
                        AnoPublicacao = 1954,
                        ISBN = "9788533613379",
                        QuantidadeDisponivel = 3
                    }
                };

                await _context.Livros.AddRangeAsync(books);
                await _context.SaveChangesAsync();
            }
        }
    }
}