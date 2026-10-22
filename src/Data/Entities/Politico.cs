using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    [Table("Politicos")]
    public class PoliticoEntity : BaseEntity
    {
        [Column("Nome")]
        [Required]
        [MaxLength(150)]
        public string Nome { get; set; } = string.Empty;

        [Column("LimiteUsuarios")]
        public int LimiteUsuarios { get; set; } = 5;

        [Column("NomeEleitoral")]
        [Required]
        [MaxLength(150)]
        public string NomeEleitoral { get; set; } = string.Empty;

        [Column("CargoAtual")]
        [MaxLength(100)]
        public string? CargoAtual { get; set; }

        [Column("Partido")]
        [Required]
        [MaxLength(50)]
        public string Partido { get; set; } = string.Empty;

        [Column("PossuiMandato")]
        [Required]
        public bool PossuiMandato { get; set; }

        [Column("Cpf")]
        [Required]
        [MaxLength(11)]
        public string Cpf { get; set; } = string.Empty;

        [Column("FotoUrl")]
        [MaxLength(500)]
        public string? FotoUrl { get; set; }

        public virtual ICollection<MembroEquipeEntity> MembrosEquipe { get; set; } = new List<MembroEquipeEntity>();
        public virtual ICollection<PessoaEntity> Apoiadores { get; set; } = new List<PessoaEntity>();
        public virtual ICollection<EventoEntity> Eventos { get; set; } = new List<EventoEntity>();
    }
}