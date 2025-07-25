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
        [ForeignKey("Livro")]
        public int LivroId { get; set; }

        [Display(Name = "Livro")]
        public virtual Livro Livro { get; set; }

        [Required(ErrorMessage = "O usuário é obrigatório")]
        [ForeignKey("Usuario")]
        public string UsuarioId { get; set; }

        [Display(Name = "Usuário")]
        public virtual Usuario Usuario { get; set; }

        [Required(ErrorMessage = "A data de retirada é obrigatória")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Data de Retirada")]
        public DateTime DataRetirada { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "A data de devolução prevista é obrigatória")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Devolução Prevista")]
        public DateTime DataDevolucaoPrevista { get; set; } = DateTime.Now.AddDays(14);

        [DataType(DataType.DateTime)]
        [Display(Name = "Data de Devolução")]
        public DateTime? DataDevolucaoReal { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Multa")]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 1000, ErrorMessage = "O valor da multa deve estar entre 0 e 1000")]
        public decimal Multa { get; set; } = 0m;

        [Required]
        [Display(Name = "Status")]
        [StringLength(20)]
        public string Status { get; set; } = "Pendente";

        [NotMapped]
        [Display(Name = "Dias de Atraso")]
        public int? DiasAtraso
        {
            get
            {
                if (DataDevolucaoReal == null && DateTime.Now > DataDevolucaoPrevista)
                {
                    return (DateTime.Now - DataDevolucaoPrevista).Days;
                }
                else if (DataDevolucaoReal > DataDevolucaoPrevista)
                {
                    return (DataDevolucaoReal.Value - DataDevolucaoPrevista).Days;
                }
                return null;
            }
        }

        [NotMapped]
        [Display(Name = "Em Atraso?")]
        public bool EmAtraso => DiasAtraso.HasValue && DiasAtraso > 0;
    }
}