using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

[Table("CabosEleitorais")]
public class CaboEleitoralEntity : BaseEntity
{
    
    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    public string? Email { get; set; }
    public string? Telefone { get; set; }

    [MaxLength(100)]
    public string? Profissao { get; set; }

    [MaxLength(150)]
    public string? LocalTrabalho { get; set; }

    [MaxLength(150)]
    public string? AreaInfluencia { get; set; } // Ex: Bairro Flexeiras, Região do 1º Distrito, etc.

    // Vínculo com a conta de usuário/Identity
    public Guid? UsuarioId { get; set; }

    // Dados de Endereço
    [MaxLength(9)]
    public string? Cep { get; set; }

    [MaxLength(150)]
    public string? Logradouro { get; set; }

    [MaxLength(20)]
    public string? Numero { get; set; }

    [MaxLength(50)]
    public string? Complemento { get; set; }

    [MaxLength(100)]
    public string? Bairro { get; set; }

    [MaxLength(100)]
    public string? Cidade { get; set; }

    [MaxLength(2)]
    public string? Uf { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
}