using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

[Table("interacaopessoalog")]
public class InteracaoPessoaLogEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column(TypeName = "char(36)")]
    public Guid InteracaoId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Usuario { get; set; } = string.Empty;

    public DateTime DataAlteracao { get; set; } = DateTime.Now;

    [MaxLength(100)]
    public string CampoAlterado { get; set; } = string.Empty;

    [Column(TypeName = "longtext")]
    public string? ValorAntigo { get; set; }

    [Column(TypeName = "longtext")]
    public string? ValorNovo { get; set; }

    [ForeignKey(nameof(InteracaoId))]
    public virtual InteracaoPessoaEntity? Interacao { get; set; }
}