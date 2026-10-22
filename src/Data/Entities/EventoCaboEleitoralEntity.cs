using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    [Table("eventocaboeleitoral")]
    public class EventoCaboEleitoralEntity : BaseEntity
    {
        [Required]
        [Column("EventoId")]
        public Guid EventoId { get; set; }

        [ForeignKey(nameof(EventoId))]
        public virtual EventoEntity? Evento { get; set; }

        [Required]
        [Column("CaboEleitoralId")]
        public Guid CaboEleitoralId { get; set; }

        [ForeignKey(nameof(CaboEleitoralId))]
        public virtual CaboEleitoralEntity? CaboEleitoral { get; set; }

        // Status de Presença / Check-in GPS
        [Column("PresencaConfirmada")]
        public bool PresencaConfirmada { get; set; }

        [Column("DataCheckIn")]
        public DateTime? DataCheckIn { get; set; }

        [Column("LatitudeCheckIn")]
        public double? LatitudeCheckIn { get; set; }

        [Column("LongitudeCheckIn")]
        public double? LongitudeCheckIn { get; set; }

        [Column("DistanciaMetros")]
        public double? DistanciaMetros { get; set; }

        // Desconfirmação de Presença
        [Column("Desconfirmado")]
        public bool Desconfirmado { get; set; }

        [MaxLength(500)]
        [Column("MotivoDesconfirmacao")]
        public string? MotivoDesconfirmacao { get; set; }

        [Column("DataDesconfirmacao")]
        public DateTime? DataDesconfirmacao { get; set; }
    }
}