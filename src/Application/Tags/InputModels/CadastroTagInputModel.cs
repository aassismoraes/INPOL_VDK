using System.ComponentModel.DataAnnotations;

namespace Application.Tags.InputModels
{
    public class CadastroTagInputModel
    {
        [Required(ErrorMessage = "O nome da tag é obrigatório.")]
        [StringLength(50, ErrorMessage = "O nome pode ter no máximo 50 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "A categoria é obrigatória.")]
        [StringLength(50, ErrorMessage = "A categoria pode ter no máximo 50 caracteres.")]
        public string Categoria { get; set; } = string.Empty;
    }
}
