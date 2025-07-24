using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaBiblioteca.Models
{
    public class Usuario : IdentityUser
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Telefone inválido")]
        public override string? PhoneNumber { get; set; }

        public virtual ICollection<Locacao>? Locacoes { get; set; }

        public TipoPerfil Perfil { get; set; } = TipoPerfil.UsuarioPadrao;
    }

    public enum TipoPerfil
    {
        UsuarioPadrao,
        Bibliotecario,
        Administrador
    }
}