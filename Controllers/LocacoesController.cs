using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaBiblioteca.Data;
using SistemaBiblioteca.Models;
using SistemaBiblioteca.Models.Enums;
using SistemaBiblioteca.Models.ViewModels;
using SistemaBiblioteca.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Authorize]
public class LocacoesController : Controller
{
    private readonly BibliotecaContext _context;
    private readonly UserManager<Usuario> _userManager;
    private const decimal ValorMultaDiaria = 2.50m;
    private const int DiasLocacao = 14;
    private const int DiasRenovacao = 7;

    public LocacoesController(BibliotecaContext context, UserManager<Usuario> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Locacoes
    public async Task<IActionResult> Index(string filtroStatus, string searchString)
    {
        var query = _context.Locacoes
            .Include(l => l.Livro)
            .Include(l => l.Usuario)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filtroStatus) && filtroStatus != "Todos")
        {
            query = query.Where(l => l.Status == filtroStatus);
        }

        if (!string.IsNullOrEmpty(searchString))
        {
            query = query.Where(l =>
                l.Livro.Titulo.Contains(searchString) ||
                l.Usuario.Nome.Contains(searchString));
        }

        ViewBag.StatusList = new SelectList(new List<string> { "Todos", "Pendente", "Devolvido", "Atrasado" });
        ViewBag.CurrentFilter = searchString;

        return View(await query.OrderByDescending(l => l.DataRetirada).ToListAsync());
    }

    // GET: Locacoes/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var locacao = await _context.Locacoes
            .Include(l => l.Livro)
            .Include(l => l.Usuario)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (locacao == null)
        {
            return NotFound();
        }

        return View(locacao);
    }

    // GET: Locacoes/Create
    [Authorize(Roles = "Administrador,Bibliotecario")]
    public async Task<IActionResult> Create()
    {
        await CarregarSelectLists();
        return View(new LocacaoViewModel());
    }

    // POST: Locacoes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Bibliotecario")]
    public async Task<IActionResult> Create(LocacaoViewModel model)
    {
        if (ModelState.IsValid)
        {
            var livro = await _context.Livros.FindAsync(model.LivroId);
            var usuario = await _userManager.FindByIdAsync(model.UsuarioId);

            if (livro == null || livro.QuantidadeDisponivel <= 0)
            {
                ModelState.AddModelError("LivroId", "Livro não disponível para locação");
                await CarregarSelectLists();
                return View(model);
            }

            if (usuario == null)
            {
                ModelState.AddModelError("UsuarioId", "Usuário não encontrado");
                await CarregarSelectLists();
                return View(model);
            }

            var locacao = new Locacao
            {
                LivroId = model.LivroId,
                UsuarioId = model.UsuarioId,
                DataRetirada = DateTime.Now,
                DataDevolucaoPrevista = DateTime.Now.AddDays(DiasLocacao),
                Status = StatusLocacao.Pendente.ToString(),
                Multa = 0
            };

            livro.QuantidadeDisponivel--;
            _context.Add(locacao);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Locação registrada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        await CarregarSelectLists();
        return View(model);
    }

    // POST: Locacoes/Devolver/5
    [HttpPost]
    [Authorize(Roles = "Administrador,Bibliotecario")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Devolver(int id)
    {
        var locacao = await _context.Locacoes
            .Include(l => l.Livro)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (locacao == null)
        {
            TempData["ErrorMessage"] = "Locação não encontrada";
            return RedirectToAction(nameof(Index));
        }

        if (locacao.Status == StatusLocacao.Devolvido.ToString())
        {
            TempData["WarningMessage"] = "Este livro já foi devolvido";
            return RedirectToAction(nameof(Index));
        }

        // Calcular multa se houver atraso
        if (DateTime.Now > locacao.DataDevolucaoPrevista)
        {
            var diasAtraso = (DateTime.Now - locacao.DataDevolucaoPrevista).Days;
            locacao.Multa = diasAtraso * ValorMultaDiaria;
            locacao.Status = StatusLocacao.Atrasado.ToString();
        }
        else
        {
            locacao.Status = StatusLocacao.Devolvido.ToString();
        }

        locacao.DataDevolucaoReal = DateTime.Now;
        locacao.Livro.QuantidadeDisponivel++;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Devolução registrada com sucesso{(locacao.Multa > 0 ? $". Multa aplicada: R$ {locacao.Multa:0.00}" : "")}";
        return RedirectToAction(nameof(Index));
    }

    // POST: Locacoes/Renovar/5
    [HttpPost]
    [Authorize(Roles = "Administrador,Bibliotecario,Usuario")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Renovar(int id)
    {
        var locacao = await _context.Locacoes.FindAsync(id);
        if (locacao == null)
        {
            TempData["ErrorMessage"] = "Locação não encontrada";
            return RedirectToAction(nameof(Index));
        }

        if (locacao.Status == StatusLocacao.Atrasado.ToString())
        {
            TempData["ErrorMessage"] = "Não é possível renovar um empréstimo atrasado";
            return RedirectToAction(nameof(Index));
        }

        locacao.DataDevolucaoPrevista = locacao.DataDevolucaoPrevista.AddDays(DiasRenovacao);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Empréstimo renovado com sucesso. Nova data de devolução: {locacao.DataDevolucaoPrevista:dd/MM/yyyy}";
        return RedirectToAction(nameof(Index));
    }

    // GET: Locacoes/RelatorioAtrasados
    [Authorize(Roles = "Administrador,Bibliotecario")]
    public async Task<IActionResult> RelatorioAtrasados()
    {
        var locacoesAtrasadas = await _context.Locacoes
            .Include(l => l.Livro)
            .Include(l => l.Usuario)
            .Where(l => l.Status == StatusLocacao.Atrasado.ToString() &&
                   l.DataDevolucaoReal == null)
            .OrderByDescending(l => l.DataDevolucaoPrevista)
            .ToListAsync();

        ViewBag.TotalMulta = locacoesAtrasadas.Sum(l => l.Multa);
        return View(locacoesAtrasadas);
    }

    private async Task CarregarSelectLists()
    {
        var livrosDisponiveis = await _context.Livros
            .Where(l => l.QuantidadeDisponivel > 0)
            .ToListAsync();

        var usuarios = await _context.Users
            .Select(u => new { u.Id, NomeCompleto = u.Nome })
            .ToListAsync();

        ViewData["LivroId"] = new SelectList(livrosDisponiveis, "Id", "Titulo");
        ViewData["UsuarioId"] = new SelectList(usuarios, "Id", "NomeCompleto");
    }
}