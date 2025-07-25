using System.ComponentModel.DataAnnotations;

namespace SistemaBiblioteca.Models.Account
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Nome Completo")]
        public string Nome { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Telefone")]
        public string Telefone { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "A senha deve ter no mínimo {2} caracteres.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Senha")]
        [Compare("Password", ErrorMessage = "As senhas não coincidem.")]
        public string ConfirmPassword { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "Você deve aceitar os termos.")]
        public bool AcceptTerms { get; set; }
    }
}