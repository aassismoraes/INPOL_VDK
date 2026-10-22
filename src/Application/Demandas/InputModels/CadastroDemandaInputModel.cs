using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.Demandas.InputModels
{
    public class CadastroDemandaInputModel
    {
        [Required(ErrorMessage = "O título da demanda é obrigatório.")]
        [StringLength(150, ErrorMessage = "O título deve ter no máximo 150 caracteres.")]
        [Display(Name = "Título da Demanda")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [StringLength(2000, ErrorMessage = "A descrição pode ter no máximo 2000 caracteres.")]
        [Display(Name = "Descrição da Solicitação")]
        public string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecione a prioridade.")]
        [Display(Name = "Prioridade")]
        public Prioridade Prioridade { get; set; } = Prioridade.Media;

        [Required(ErrorMessage = "Informe o bairro.")]
        [Display(Name = "Bairro")]
        public string Bairro { get; set; } = string.Empty;

        [Display(Name = "Previsão de Conclusão")]
        [DataType(DataType.Date)]
        public DateTime? DataConclusaoPrevista { get; set; }

        [Required(ErrorMessage = "Selecione o apoiador solicitante.")]
        [Display(Name = "Apoiador Solicitante")]
        public Guid ApoiadorId { get; set; }
    }

}
