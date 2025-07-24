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
    private const decimal ValorMultaDiaria = 2.00m;

    public LocacoesController(BibliotecaContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string filtroStatus)
    {
        var query = _context.Locacoes
            .Include(l => l.Livro)
            .Include(l => l.Usuario)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filtroStatus))
        {
            query = query.Where(l => l.Status == filtroStatus);
        }

        ViewBag.StatusList = new List<string> { "Pendente", "Devolvido", "Atrasado" };
        return View(await query.ToListAsync());
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
        ViewData["LivroId"] = new SelectList(_context.Livros.Where(l => l.QuantidadeDisponivel > 0), "Id", "Titulo");
        ViewData["UsuarioId"] = new SelectList(_context.Users, "Id", "Nome");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Bibliotecario")]
    public async Task<IActionResult> Create([Bind("Id,LivroId,UsuarioId")] Locacao locacao)
    {
        if (ModelState.IsValid)
        {
            var livro = await _context.Livros.FindAsync(locacao.LivroId);
            if (livro == null || livro.QuantidadeDisponivel <= 0)
            {
                ModelState.AddModelError("", "Livro não disponível para locação");
                ViewData["LivroId"] = new SelectList(_context.Livros, "Id", "Titulo", locacao.LivroId);
                ViewData["UsuarioId"] = new SelectList(_context.Users, "Id", "Nome", locacao.UsuarioId);
                return View(locacao);
            }

            locacao.DataRetirada = DateTime.Now;
            locacao.DataDevolucaoPrevista = DateTime.Now.AddDays(14);
            locacao.Status = "Pendente";
            locacao.Multa = 0;

            livro.QuantidadeDisponivel--;
            _context.Update(livro);
            _context.Add(locacao);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["LivroId"] = new SelectList(_context.Livros, "Id", "Titulo", locacao.LivroId);
        ViewData["UsuarioId"] = new SelectList(_context.Users, "Id", "Nome", locacao.UsuarioId);
        return View(locacao);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Bibliotecario")]
    public async Task<IActionResult> Devolver(int id)
    {
        var locacao = await _context.Locacoes
            .Include(l => l.Livro)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (locacao == null) return NotFound();

        // Calcular multa se houver atraso
        if (DateTime.Now > locacao.DataDevolucaoPrevista && locacao.Status != "Devolvido")
        {
            var diasAtraso = (DateTime.Now - locacao.DataDevolucaoPrevista).Days;
            locacao.Multa = diasAtraso * ValorMultaDiaria;
            locacao.Status = "Atrasado";
        }

        locacao.DataDevolucaoReal = DateTime.Now;
        locacao.Status = "Devolvido";
        locacao.Livro.QuantidadeDisponivel++;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Bibliotecario,Usuario")]
    public async Task<IActionResult> Renovar(int id)
    {
        var locacao = await _context.Locacoes.FindAsync(id);

        if (locacao == null) return NotFound();

        // Verificar se já foi renovado antes ou se está atrasado
        if (locacao.Status == "Atrasado" || DateTime.Now > locacao.DataDevolucaoPrevista)
        {
            TempData["ErrorMessage"] = "Não é possível renovar um empréstimo atrasado";
            return RedirectToAction(nameof(Index));
        }

        locacao.DataDevolucaoPrevista = locacao.DataDevolucaoPrevista.AddDays(7); // Renova por 7 dias
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Empréstimo renovado com sucesso";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> RelatorioAtrasados()
    {
        var locacoesAtrasadas = await _context.Locacoes
            .Include(l => l.Livro)
            .Include(l => l.Usuario)
            .Where(l => l.Status == "Atrasado")
            .ToListAsync();

        return View(locacoesAtrasadas);
    }

    private bool LocacaoExists(int id)
    {
        return _context.Locacoes.Any(e => e.Id == id);
    }
}