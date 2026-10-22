using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{

    [Table("MembrosEquipe")]
    public class MembroEquipeEntity : BaseEntity
    {
        [Required]
        public Guid PoliticoId { get; set; }

        [ForeignKey(nameof(PoliticoId))]
        public virtual PoliticoEntity Politico { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Cargo { get; set; } = string.Empty;

        [Required]
        public NivelAcesso Permissao { get; set; }

        public virtual ICollection<DemandaEntity> DemandasAtribuidas { get; set; } = new List<DemandaEntity>();
        public virtual ICollection<AtividadeCampoEntity> AtividadesRealizadas { get; set; } = new List<AtividadeCampoEntity>();
        public virtual ICollection<EventoEntity> EventosParticipando { get; set; } = new List<EventoEntity>();
    }
}
