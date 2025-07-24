using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaBiblioteca.Models
{
    public class Livro
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório")]
        [StringLength(100, ErrorMessage = "O título não pode exceder 100 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O autor é obrigatório")]
        public string Autor { get; set; } = string.Empty;

        [Required(ErrorMessage = "A editora é obrigatória")]
        public string Editora { get; set; } = string.Empty;

        [Required(ErrorMessage = "O ano de publicação é obrigatório")]
        [Range(1000, 9999, ErrorMessage = "Ano de publicação inválido")]
        public int AnoPublicacao { get; set; }

        [Required(ErrorMessage = "O ISBN é obrigatório")]
        [RegularExpression(@"^(\d{10}|\d{13})$", ErrorMessage = "ISBN deve ter 10 ou 13 dígitos")]
        public string ISBN { get; set; } = string.Empty;

        [Required(ErrorMessage = "A quantidade disponível é obrigatória")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantidade inválida")]
        public int QuantidadeDisponivel { get; set; }

        public ICollection<Locacao>? Locacoes { get; set; }
    }
}