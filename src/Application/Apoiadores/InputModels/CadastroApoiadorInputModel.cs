using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.Apoiadores.InputModels
{
    public class CadastroApoiadorInputModel
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(150, ErrorMessage = "O nome deve ter no máximo 150 caracteres.")]
        [Display(Name = "Nome Completo")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O telefone é obrigatório.")]
        [Phone(ErrorMessage = "Insira um telefone válido.")]
        [Display(Name = "Telefone / WhatsApp")]
        public string Telefone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Insira um e-mail válido.")]
        [Display(Name = "E-mail")]
        public string? Email { get; set; }

        [Display(Name = "Nascimento")]
        [DataType(DataType.Date)]
        public DateOnly? DataNascimento { get; set; }

        [Required(ErrorMessage = "Selecione o status do voto.")]
        [Display(Name = "Intenção de Voto")]
        public StatusVoto StatusVoto { get; set; } = StatusVoto.Indeciso;

        [Required(ErrorMessage = "Selecione o nível de engajamento.")]
        [Display(Name = "Engajamento")]
        public NivelLideranca NivelLideranca { get; set; } = NivelLideranca.Eleitor;

        public EnderecoInputModel Endereco { get; set; } = new();
    }
  
}