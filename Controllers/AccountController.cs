using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SistemaBiblioteca.Models;
using SistemaBiblioteca.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

public class AccountController : Controller
{
    private readonly SignInManager<Usuario> _signInManager;
    private readonly UserManager<Usuario> _userManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<Usuario> signInManager,
        UserManager<Usuario> userManager,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: /Account/Login
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("Usuário logado.");
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("Conta bloqueada.");
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Tentativa de login inválida.");
                return View(model);
            }
        }
        return View(model);
    }

    // GET: /Account/Register
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View();
    }

    // POST: /Account/Register
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new Usuario
            {
                UserName = model.Email,
                Email = model.Email,
                Nome = model.Nome,
                PhoneNumber = model.Telefone,
                Perfil = TipoPerfil.UsuarioPadrao
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, user.Perfil.ToString());

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("Usuário deslogado.");
        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/Manage
    [HttpGet]
    public async Task<IActionResult> Manage()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var model = new ManageViewModel
        {
            Nome = user.Nome,
            Email = user.Email,
            Telefone = user.PhoneNumber
        };

        return View(model);
    }

    // Métodos auxiliares
    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Home");
    }
}
