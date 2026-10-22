using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.Apoiadores.InputModels;

public class EditarApoiadorInputModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(150, ErrorMessage = "O nome não pode exceder 150 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Telefone inválido.")]
    public string? Telefone { get; set; }

    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string? Email { get; set; }

    public DateOnly? DataNascimento { get; set; }

    [Required(ErrorMessage = "Selecione a intenção de voto.")]
    public StatusVoto StatusVoto { get; set; }

    [Required(ErrorMessage = "Selecione o nível de liderança.")]
    public NivelLideranca NivelLideranca { get; set; }

    // Campos de Endereço Opcionais
    public string? Cep { get; set; }
    public string? Logradouro { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Uf { get; set; }

    // Campos de Endereço Obrigatórios no Form
    [Required(ErrorMessage = "O bairro é obrigatório.")]
    public string Bairro { get; set; } = string.Empty;

    [Required(ErrorMessage = "A cidade é obrigatória.")]
    public string Cidade { get; set; } = string.Empty;

    // Coordenadas
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}