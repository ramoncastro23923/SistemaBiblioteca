using System;
using System.ComponentModel.DataAnnotations;

namespace SistemaBiblioteca.ViewModels
{
    public class LocacaoViewModel
    {
        public int LivroId { get; set; }
        public string LivroTitulo { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Usuário")]
        public string UsuarioId { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Data de Retirada")]
        public DateTime DataRetirada { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "Data Prevista Devolução")]
        public DateTime DataDevolucaoPrevista { get; set; } = DateTime.Today.AddDays(14);

        [Display(Name = "Observações")]
        [StringLength(500)]
        public string? Observacoes { get; set; }
    }
}