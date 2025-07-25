using System.ComponentModel.DataAnnotations;

namespace SistemaBiblioteca.Models.Account
{
    public class ManageViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [Display(Name = "Nome Completo")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Telefone inválido")]
        [Display(Name = "Telefone")]
        public string Telefone { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Nova Senha (deixe em branco para não alterar)")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Nova Senha")]
        [Compare("NewPassword", ErrorMessage = "As senhas não coincidem")]
        public string? ConfirmPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Senha Atual (necessária para alterações)")]
        public string? CurrentPassword { get; set; }
    }
}