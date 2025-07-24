using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaBiblioteca.Data;
using Microsoft.EntityFrameworkCore;

namespace SistemaBiblioteca.Pages
{
    public class IndexModel : PageModel
    {
        private readonly BibliotecaContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(BibliotecaContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public int TotalLivros { get; set; }
        public int TotalUsuarios { get; set; }
        public int LocacoesAtivas { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                TotalLivros = await _context.Livros.CountAsync();
                TotalUsuarios = await _context.Users.CountAsync();
                LocacoesAtivas = await _context.Locacoes
                    .CountAsync(l => l.Status == "Pendente");

                ViewData["TotalLivros"] = TotalLivros;
                ViewData["TotalUsuarios"] = TotalUsuarios;
                ViewData["LocacoesAtivas"] = LocacoesAtivas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dados da página inicial");
                // Define valores padrão em caso de erro
                ViewData["TotalLivros"] = 0;
                ViewData["TotalUsuarios"] = 0;
                ViewData["LocacoesAtivas"] = 0;
            }
        }
    }
}