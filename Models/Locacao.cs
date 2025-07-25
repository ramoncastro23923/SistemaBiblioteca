using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemaBiblioteca.Models.Enums;

namespace SistemaBiblioteca.Models
{
    public class Locacao
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O livro é obrigatório")]
        [Display(Name = "Livro")]
        public int LivroId { get; set; }

        [ForeignKey("LivroId")]
        public virtual Livro? Livro { get; set; }

        [Required(ErrorMessage = "O usuário é obrigatório")]
        [Display(Name = "Usuário")]
        public string UsuarioId { get; set; } = string.Empty;

        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Data de Retirada")]
        public DateTime DataRetirada { get; set; } = DateTime.Now;

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Devolução Prevista")]
        public DateTime DataDevolucaoPrevista { get; set; } = DateTime.Now.AddDays(14);

        [DataType(DataType.DateTime)]
        [Display(Name = "Devolução Real")]
        public DateTime? DataDevolucaoReal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Multa (R$)")]
        public decimal Multa { get; set; }

        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = StatusLocacao.Pendente.ToString();
    }
}