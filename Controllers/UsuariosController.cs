using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaBiblioteca.Data;
using SistemaBiblioteca.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SistemaBiblioteca.Models.Account;

[Authorize(Roles = "Administrador")]
public class UsuariosController : Controller
{
    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;
    private readonly BibliotecaContext _context;
    private readonly IConfiguration _configuration;

    public UsuariosController(
        UserManager<Usuario> userManager,
        SignInManager<Usuario> signInManager,
        BibliotecaContext context,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _configuration = configuration;
    }

    // GET: Usuarios
    public async Task<IActionResult> Index()
    {
        var usuarios = await _userManager.Users.ToListAsync();
        return View(usuarios);
    }

    // GET: Usuarios/Register
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View();
    }

    // POST: Usuarios/Register
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

                // Generate email confirmation token
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Action(
                    "ConfirmEmail",
                    "Usuarios",
                    new { userId = user.Id, code = code },
                    protocol: HttpContext.Request.Scheme);

                // TODO: Implement email service
                // await _emailSender.SendEmailAsync(model.Email, "Confirm your email",
                //    $"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a>");

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

    // GET: Usuarios/Login
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // POST: Usuarios/Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var token = GenerateJwtToken(user);

                Response.Cookies.Append(
                    "X-Access-Token",
                    token,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddDays(7)
                    });

                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                return View("Lockout");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }

        return View(model);
    }

    // POST: Usuarios/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        Response.Cookies.Delete("X-Access-Token");
        return RedirectToAction("Index", "Home");
    }

    // GET: Usuarios/ConfirmEmail
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        if (userId == null || code == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.ConfirmEmailAsync(user, code);
        if (result.Succeeded)
        {
            return View("ConfirmEmail");
        }

        return View("Error");
    }

    // Helper method to generate JWT token
    private string GenerateJwtToken(Usuario user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Nome),
            new Claim(ClaimTypes.Role, user.Perfil.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"] ?? "30"));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Helper method for local redirects
    private IActionResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Home");
    }
}