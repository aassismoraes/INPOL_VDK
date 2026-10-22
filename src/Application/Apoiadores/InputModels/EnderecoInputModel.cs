using System.ComponentModel.DataAnnotations;

namespace Application.Apoiadores.InputModels;

public class EnderecoInputModel
{
    [Display(Name = "CEP")]
    [StringLength(9, ErrorMessage = "O CEP deve ter no máximo 9 caracteres.")]
    public string? Cep { get; set; }

    [Display(Name = "Rua / Avenida")]
    [StringLength(150, ErrorMessage = "O logradouro deve ter no máximo 150 caracteres.")]
    public string? Logradouro { get; set; }

    [Display(Name = "Número")]
    [StringLength(20, ErrorMessage = "O número deve ter no máximo 20 caracteres.")]
    public string? Numero { get; set; }

    [Display(Name = "Complemento")]
    [StringLength(100, ErrorMessage = "O complemento deve ter no máximo 100 caracteres.")]
    public string? Complemento { get; set; }

    [Required(ErrorMessage = "O bairro é obrigatório.")]
    [Display(Name = "Bairro")]
    [StringLength(100, ErrorMessage = "O bairro deve ter no máximo 100 caracteres.")]
    public string Bairro { get; set; } = string.Empty;

    [Required(ErrorMessage = "A cidade é obrigatória.")]
    [Display(Name = "Cidade")]
    [StringLength(100, ErrorMessage = "A cidade deve ter no máximo 100 caracteres.")]
    public string Cidade { get; set; } = string.Empty;

    [Display(Name = "UF")]
    [StringLength(2, ErrorMessage = "A UF deve ter 2 caracteres.")]
    public string? Uf { get; set; }

    // --- Coordenadas Geográficas ---
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}