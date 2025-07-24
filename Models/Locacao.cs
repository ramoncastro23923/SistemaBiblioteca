using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaBiblioteca.Models
{
    public class Locacao
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O livro é obrigatório")]
        public int LivroId { get; set; }

        [ForeignKey("LivroId")]
        public Livro? Livro { get; set; }

        [Required(ErrorMessage = "O usuário é obrigatório")]
        public string UsuarioId { get; set; } = string.Empty;

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

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
        public string Status { get; set; } = "Pendente";
    }
}