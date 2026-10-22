using CrossCutting.Helpers;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

[Table("DemandasMovimentacoes")]
public class DemandaMovimentacaoEntity : BaseEntity
{
    [Required]
    public Guid DemandaId { get; set; }

    [ForeignKey(nameof(DemandaId))]
    public virtual DemandaEntity Demanda { get; set; } = null!;

    [Required]
    public DateTime DataHora { get; set; } = DateTimeExtensions.ObterDataHoraBrasilia();

    [Required]
    [MaxLength(1000)]
    public string Observacao { get; set; } = string.Empty;

    public StatusDemanda? NovoStatus { get; set; }

    [MaxLength(100)]
    public string? Responsavel { get; set; }
}