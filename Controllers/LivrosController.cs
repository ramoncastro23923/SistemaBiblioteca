using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaBiblioteca.Data;
using SistemaBiblioteca.Models;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "Administrador,Bibliotecario")] // Somente administradores e bibliotecários
public class LivrosController : Controller
{
    private readonly BibliotecaContext _context;

    public LivrosController(BibliotecaContext context)
    {
        _context = context;
    }

    [AllowAnonymous] // Permite acesso sem autenticação
    public async Task<IActionResult> Index(string searchString)
    {
        var livros = from l in _context.Livros select l;

        if (!string.IsNullOrEmpty(searchString))
        {
            livros = livros.Where(s => s.Titulo.Contains(searchString)
                           || s.Autor.Contains(searchString));
        }

        return View(await livros.ToListAsync());
    }

    [Authorize(Roles = "Administrador")] // Apenas administradores
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
            return RedirectToAction(nameof(Index));
        }
        return View(livro);
    }

    [Authorize(Roles = "Administrador,Bibliotecario")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var livro = await _context.Livros.FindAsync(id);
        if (livro == null) return NotFound();

        return View(livro);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Bibliotecario")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,Autor,Editora,AnoPublicacao,ISBN,QuantidadeDisponivel")] Livro livro)
    {
        if (id != livro.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(livro);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LivroExists(livro.Id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(livro);
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var livro = await _context.Livros
            .FirstOrDefaultAsync(m => m.Id == id);
        if (livro == null) return NotFound();

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
        }
        return RedirectToAction(nameof(Index));
    }

    private bool LivroExists(int id)
    {
        return _context.Livros.Any(e => e.Id == id);
    }
}