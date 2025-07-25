using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaBiblioteca.Models
{
    public class Usuario : IdentityUser
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres")]
        [Display(Name = "Nome Completo")]
        public string Nome { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Número de telefone inválido")]
        [Display(Name = "Telefone")]
        public override string? PhoneNumber { get; set; }

        [Display(Name = "Tipo de Perfil")]
        public TipoPerfil Perfil { get; set; } = TipoPerfil.UsuarioPadrao;

        // Relacionamento com Locações
        public virtual ICollection<Locacao>? Locacoes { get; set; }
    }

    public enum TipoPerfil
    {
        [Display(Name = "Usuário Padrão")]
        UsuarioPadrao,

        [Display(Name = "Bibliotecário")]
        Bibliotecario,

        [Display(Name = "Administrador")]
        Administrador
    }
}