using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaBiblioteca.Data;
using SistemaBiblioteca.Models;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class LocacoesController : Controller
{
    private readonly BibliotecaContext _context;

    public LocacoesController(BibliotecaContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var bibliotecaContext = _context.Locacoes
            .Include(l => l.Livro)
            .Include(l => l.Usuario);
        return View(await bibliotecaContext.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var locacao = await _context.Locacoes
            .Include(l => l.Livro)
            .Include(l => l.Usuario)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (locacao == null) return NotFound();

        return View(locacao);
    }

    [Authorize(Roles = "Administrador,Bibliotecario")]
    public IActionResult Create()
    {
        ViewData["LivroId"] = new SelectList(_context.Livros, "Id", "Titulo");
        ViewData["UsuarioId"] = new SelectList(_context.Users, "Id", "Nome"); // Alterado para Users
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Bibliotecario")]
    public async Task<IActionResult> Create([Bind("Id,LivroId,UsuarioId,DataRetirada,DataDevolucaoPrevista")] Locacao locacao)
    {
        if (ModelState.IsValid)
        {
            var livro = await _context.Livros.FindAsync(locacao.LivroId);
            if (livro == null || livro.QuantidadeDisponivel <= 0)
            {
                ModelState.AddModelError("", "Livro não disponível para locação");
                ViewData["LivroId"] = new SelectList(_context.Livros, "Id", "Titulo", locacao.LivroId);
                ViewData["UsuarioId"] = new SelectList(_context.Users, "Id", "Nome", locacao.UsuarioId); // Alterado para Users
                return View(locacao);
            }

            locacao.DataRetirada = DateTime.Now;
            locacao.DataDevolucaoPrevista = DateTime.Now.AddDays(14);
            locacao.Status = "Pendente";
            locacao.Multa = 0;

            livro.QuantidadeDisponivel--;

            _context.Add(locacao);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["LivroId"] = new SelectList(_context.Livros, "Id", "Titulo", locacao.LivroId);
        ViewData["UsuarioId"] = new SelectList(_context.Users, "Id", "Nome", locacao.UsuarioId); // Alterado para Users
        return View(locacao);
    }

    private bool LocacaoExists(int id)
    {
        return _context.Locacoes.Any(e => e.Id == id);
    }
}