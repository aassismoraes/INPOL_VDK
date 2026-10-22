using Domain.Enums;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    [Table("AtividadesCampo")]
    public class AtividadeCampoEntity : BaseEntity
    {
        [Required]
        public DateTime DataHora { get; set; } = DateTime.Now;

        [Required]
        public TipoAtividade TipoAtividade { get; set; }

        [MaxLength(1000)]
        public string? Observacoes { get; set; }

        public Point? GeolocalizacaoCheckin { get; set; }

        [Required]
        public Guid MembroEquipeId { get; set; }

        [ForeignKey(nameof(MembroEquipeId))]
        public virtual MembroEquipeEntity MembroEquipe { get; set; } = null!;

        public Guid? ApoiadorId { get; set; }

        [ForeignKey(nameof(ApoiadorId))]
        public virtual PessoaEntity? Apoiador { get; set; }
    }
}
