using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

[Table("EventoPresencas")]
public class EventoPresencaEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid EventoId { get; set; }

    [Required]
    public Guid CaboEleitoralId { get; set; }

    public DateTime DataCheckIn { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double DistanciaMetros { get; set; }

    // Propriedades de Navegação
    [ForeignKey(nameof(EventoId))]
    public virtual EventoEntity? Evento { get; set; }

    [ForeignKey(nameof(CaboEleitoralId))]
    public virtual CaboEleitoralEntity? CaboEleitoral { get; set; }
}