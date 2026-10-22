using CrossCutting.Helpers;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.Eventos.InputModels
{
    public class CadastroEventoInputModel
    {
        [Required(ErrorMessage = "O título do evento é obrigatório.")]
        [StringLength(150, ErrorMessage = "O título deve ter no máximo 150 caracteres.")]
        [Display(Name = "Título do Evento")]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "A descrição pode ter no máximo 1000 caracteres.")]
        [Display(Name = "Descrição / Pauta")]
        public string? Descricao { get; set; }

        [Required(ErrorMessage = "A data e hora de início são obrigatórias.")]
        [Display(Name = "Data e Hora de Início")]
        public DateTime DataHoraInicio { get; set; } = DateTimeExtensions.ObterDataHoraBrasilia();

        [Required(ErrorMessage = "A data e hora de término são obrigatórias.")]
        [Display(Name = "Data e Hora de Término")]
        public DateTime? DataHoraFim { get; set; } = DateTimeExtensions.ObterDataHoraBrasilia().AddHours(1);

        [Required(ErrorMessage = "Informe o local do evento.")]
        [StringLength(200, ErrorMessage = "O local deve ter no máximo 200 caracteres.")]
        [Display(Name = "Local")]
        public string Local { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecione o tipo de evento.")]
        [Display(Name = "Tipo de Evento")]
        public TipoEvento Tipo { get; set; }

        // --- Atributos de Endereço Detalhado ---

        [Display(Name = "CEP")]
        [StringLength(9, ErrorMessage = "O CEP deve ter no máximo 9 caracteres.")]
        [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "Informe um CEP válido (ex: 00000-000).")]
        public string? Cep { get; set; }

        [Display(Name = "Logradouro / Rua")]
        [StringLength(150, ErrorMessage = "O logradouro deve ter no máximo 150 caracteres.")]
        public string? Logradouro { get; set; }

        [Display(Name = "Número")]
        [StringLength(20, ErrorMessage = "O número deve ter no máximo 20 caracteres.")]
        public string? Numero { get; set; }

        [Display(Name = "Complemento")]
        [StringLength(100, ErrorMessage = "O complemento deve ter no máximo 100 caracteres.")]
        public string? Complemento { get; set; }

        [Display(Name = "Bairro")]
        [StringLength(100, ErrorMessage = "O bairro deve ter no máximo 100 caracteres.")]
        public string? Bairro { get; set; }

        [Display(Name = "Cidade")]
        [StringLength(100, ErrorMessage = "A cidade deve ter no máximo 100 caracteres.")]
        public string? Cidade { get; set; }

        [Display(Name = "UF")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "A UF deve conter exatamente 2 caracteres.")]
        [RegularExpression(@"^[A-Za-z]{2}$", ErrorMessage = "UF inválida.")]
        public string? Uf { get; set; }

        // --- Atributos para Geocodificação / NTS ---

        [Display(Name = "Latitude")]
        public double? Latitude { get; set; }

        [Display(Name = "Longitude")]
        public double? Longitude { get; set; }

        /// <summary>
        /// Lista de IDs dos Cabos Eleitorais convidados para o evento
        /// </summary>
        public List<Guid> CabosEleitoraisIds { get; set; } = new();

        [Display(Name = "Minutos de Antecedência para Check-in")]
        [Range(0, 1440, ErrorMessage = "Informe um valor entre 0 e 1440 minutos.")]
        public int MinutosAntecedenciaCheckIn { get; set; } = 20;
    }
}