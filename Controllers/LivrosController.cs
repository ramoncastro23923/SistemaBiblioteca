using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaBiblioteca.Data;
using SistemaBiblioteca.Models;
using Microsoft.AspNetCore.Authorization;
using SistemaBiblioteca.Utils;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SistemaBiblioteca.Controllers
{
    [Authorize(Roles = "Administrador,Bibliotecario")]
    [AutoValidateAntiforgeryToken]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class LivrosController : Controller
    {
        private readonly BibliotecaContext _context;
        private readonly ILogger<LivrosController> _logger;

        public LivrosController(BibliotecaContext context, ILogger<LivrosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Livros
        [AllowAnonymous]
        public async Task<IActionResult> Index(
            string searchString,
            string filterBy = "titulo",
            string currentFilter = "",
            int? pageNumber = 1)
        {
            ViewData["CurrentFilter"] = searchString ?? currentFilter;
            ViewData["FilterBy"] = filterBy;

            var livros = _context.Livros
                .Include(l => l.Locacoes)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                livros = filterBy switch
                {
                    "autor" => livros.Where(l => l.Autor.ToLower().Contains(searchString)),
                    "isbn" => livros.Where(l => l.ISBN.Contains(searchString)),
                    "editora" => livros.Where(l => l.Editora.ToLower().Contains(searchString)),
                    _ => livros.Where(l => l.Titulo.ToLower().Contains(searchString))
                };
            }

            int pageSize = 10;
            return View(await PaginatedList<Livro>.CreateAsync(livros, pageNumber ?? 1, pageSize));
        }

        // GET: Livros/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var livro = await _context.Livros
                .Include(l => l.Locacoes)
                .ThenInclude(l => l.Usuario)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (livro == null)
            {
                return NotFound();
            }

            return View(livro);
        }

        // GET: Livros/Create
        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Livros/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create([Bind("Id,Titulo,Autor,Editora,AnoPublicacao,ISBN,QuantidadeDisponivel")] Livro livro)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(livro);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Livro '{livro.Titulo}' cadastrado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao criar livro");
                ModelState.AddModelError("", "Não foi possível salvar as alterações. " +
                    "Tente novamente, e se o problema persistir, " +
                    "consulte o administrador do sistema.");
            }

            return View(livro);
        }

        // GET: Livros/Edit/5
        [Authorize(Roles = "Administrador,Bibliotecario")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var livro = await _context.Livros.FindAsync(id);
            if (livro == null)
            {
                return NotFound();
            }
            return View(livro);
        }

        // POST: Livros/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Bibliotecario")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,Autor,Editora,AnoPublicacao,ISBN,QuantidadeDisponivel")] Livro livro)
        {
            if (id != livro.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(livro);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Livro '{livro.Titulo}' atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!LivroExists(livro.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Erro de concorrência ao editar livro");
                        ModelState.AddModelError("", "O registro foi modificado por outro usuário. " +
                            "As alterações não foram salvas.");
                        return View(livro);
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(livro);
        }

        // GET: Livros/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var livro = await _context.Livros
                .Include(l => l.Locacoes)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (livro == null)
            {
                return NotFound();
            }

            if (livro.Locacoes?.Any() == true)
            {
                TempData["ErrorMessage"] = "Não é possível excluir um livro com locações associadas";
                return RedirectToAction(nameof(Index));
            }

            return View(livro);
        }

        // POST: Livros/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var livro = await _context.Livros.FindAsync(id);
            if (livro == null)
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Livros.Remove(livro);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Livro '{livro.Titulo}' removido com sucesso!";
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao deletar livro");
                TempData["ErrorMessage"] = $"Não foi possível deletar o livro '{livro.Titulo}'. " +
                                          "Ele pode ter registros associados.";
            }

            return RedirectToAction(nameof(Index));
        }

        // AJAX Validation Methods
        [AllowAnonymous]
        public async Task<JsonResult> IsTituloUnique(string titulo, int? id)
        {
            var exists = await _context.Livros
                .AnyAsync(l => l.Titulo == titulo && (!id.HasValue || l.Id != id.Value));

            return Json(!exists);
        }

        [AllowAnonymous]
        public async Task<JsonResult> IsEditoraUnique(string editora, int? id)
        {
            var exists = await _context.Livros
                .AnyAsync(l => l.Editora == editora && (!id.HasValue || l.Id != id.Value));

            return Json(!exists);
        }

        [AllowAnonymous]
        public async Task<JsonResult> IsIsbnUnique(string isbn, int? id)
        {
            var exists = await _context.Livros
                .AnyAsync(l => l.ISBN == isbn && (!id.HasValue || l.Id != id.Value));

            return Json(!exists);
        }

        private bool LivroExists(int id)
        {
            return _context.Livros.Any(e => e.Id == id);
        }
    }
}