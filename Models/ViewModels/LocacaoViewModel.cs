using System.ComponentModel.DataAnnotations;

namespace SistemaBiblioteca.Models.ViewModels
{
    public class LocacaoViewModel
    {
        [Required(ErrorMessage = "Selecione um livro")]
        [Display(Name = "Livro")]
        public int LivroId { get; set; }

        [Required(ErrorMessage = "Selecione um usuário")]
        [Display(Name = "Usuário")]
        public string UsuarioId { get; set; }
    }
}