using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaBiblioteca.Data;
using SistemaBiblioteca.Models;
using Microsoft.AspNetCore.Authorization;
using SistemaBiblioteca.Utils;

[Authorize(Roles = "Administrador,Bibliotecario")]
public class LivrosController : Controller
{
    private readonly BibliotecaContext _context;

    public LivrosController(BibliotecaContext context)
    {
        _context = context;
    }

    // GET: Livros
    [AllowAnonymous]
    public async Task<IActionResult> Index(string searchString, string currentFilter, int? pageNumber)
    {
        ViewData["CurrentFilter"] = searchString;

        if (searchString != null)
        {
            pageNumber = 1;
        }
        else
        {
            searchString = currentFilter;
        }

        var livros = from l in _context.Livros select l;

        if (!string.IsNullOrEmpty(searchString))
        {
            livros = livros.Where(l => l.Titulo.Contains(searchString)
                           || l.Autor.Contains(searchString)
                           || l.ISBN.Contains(searchString));
        }

        int pageSize = 10;
        return View(await PaginatedList<Livro>.CreateAsync(livros.AsNoTracking(), pageNumber ?? 1, pageSize));
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create([Bind("Id,Titulo,Autor,Editora,AnoPublicacao,ISBN,QuantidadeDisponivel")] Livro livro)
    {
        if (ModelState.IsValid)
        {
            _context.Add(livro);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Livro cadastrado com sucesso!";
            return RedirectToAction(nameof(Index));
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
                TempData["SuccessMessage"] = "Livro atualizado com sucesso!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LivroExists(livro.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
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
            .FirstOrDefaultAsync(m => m.Id == id);

        if (livro == null)
        {
            return NotFound();
        }

        return View(livro);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var livro = await _context.Livros.FindAsync(id);
        if (livro != null)
        {
            _context.Livros.Remove(livro);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Livro removido com sucesso!";
        }
        return RedirectToAction(nameof(Index));
    }

    private bool LivroExists(int id)
    {
        return _context.Livros.Any(e => e.Id == id);
    }
}