using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaBiblioteca.Data;
using SistemaBiblioteca.Models;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "Administrador")] // Somente administradores podem gerenciar usuários
public class UsuariosController : Controller
{
    private readonly UserManager<Usuario> _userManager;
    private readonly BibliotecaContext _context;

    public UsuariosController(UserManager<Usuario> userManager, BibliotecaContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public IActionResult Index()
    {
        var usuarios = _userManager.Users.ToList();
        return View(usuarios);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(UsuarioViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new Usuario
            {
                UserName = model.Email,
                Email = model.Email,
                Nome = model.Nome,
                PhoneNumber = model.Telefone,
                Perfil = model.Perfil
            };

            var result = await _userManager.CreateAsync(user, model.Senha);

            if (result.Succeeded)
            {
                var role = model.Perfil == TipoPerfil.Administrador ? "Administrador" : "Usuario";
                await _userManager.AddToRoleAsync(user, role);

                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        return View(model);
    }

    // Adicione métodos para Editar/Excluir usuários conforme necessário
}