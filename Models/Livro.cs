using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;

namespace SistemaBiblioteca.Models
{
    public class Livro
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório")]
        [StringLength(100, ErrorMessage = "O título não pode exceder 100 caracteres")]
        [Display(Name = "Título")]
        [Remote("IsTituloUnique", "Livros", AdditionalFields = "Id", ErrorMessage = "Já existe um livro com este título")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O autor é obrigatório")]
        [StringLength(50, ErrorMessage = "O autor não pode exceder 50 caracteres")]
        [Display(Name = "Autor")]
        public string Autor { get; set; } = string.Empty;

        [Required(ErrorMessage = "A editora é obrigatória")]
        [StringLength(50, ErrorMessage = "A editora não pode exceder 50 caracteres")]
        [Display(Name = "Editora")]
        [Remote("IsEditoraUnique", "Livros", AdditionalFields = "Id", ErrorMessage = "Esta editora já está cadastrada")]
        public string Editora { get; set; } = string.Empty;

        [Required(ErrorMessage = "O ano de publicação é obrigatório")]
        [Range(1000, 9999, ErrorMessage = "Ano de publicação inválido")]
        [Display(Name = "Ano de Publicação")]
        public int AnoPublicacao { get; set; }

        [Required(ErrorMessage = "O ISBN é obrigatório")]
        [RegularExpression(@"^(\d{10}|\d{13})$", ErrorMessage = "ISBN deve ter 10 ou 13 dígitos")]
        [Display(Name = "ISBN")]
        [Remote("IsIsbnUnique", "Livros", AdditionalFields = "Id", ErrorMessage = "Este ISBN já está cadastrado")]
        public string ISBN { get; set; } = string.Empty;

        [Required(ErrorMessage = "A quantidade disponível é obrigatória")]
        [Range(0, 1000, ErrorMessage = "Quantidade deve estar entre 0 e 1000")]
        [Display(Name = "Quantidade Disponível")]
        public int QuantidadeDisponivel { get; set; }

        // Relacionamentos
        public virtual ICollection<Locacao>? Locacoes { get; set; }

        // Propriedades calculadas
        [NotMapped]
        [Display(Name = "Disponível?")]
        public bool Disponivel => QuantidadeDisponivel > 0 &&
                                !(Locacoes?.Any(l => l.DataDevolucaoReal == null) ?? false); 
        [NotMapped]
        [Display(Name = "Total Exemplares")]
        public int TotalExemplares => QuantidadeDisponivel +
                                    (Locacoes?.Count(l => l.DataDevolucaoReal == null) ?? 0);
    }
}