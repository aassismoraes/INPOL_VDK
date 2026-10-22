using Domain.Enums;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    [Table("Eventos")]
    public class EventoEntity : BaseEntity
    {
        [Required]
        public Guid PoliticoId { get; set; }

        [ForeignKey(nameof(PoliticoId))]
        public virtual PoliticoEntity Politico { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string Titulo { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Descricao { get; set; }

        [Required]
        public DateTime DataHoraInicio { get; set; }

        public DateTime? DataHoraFim { get; set; }

        [Required]
        public TipoEvento Tipo { get; set; }

        [Required]
        public StatusEvento Status { get; set; } = StatusEvento.Agendado;

        // --- Endereço Completo (Opcionais) ---
        [MaxLength(10)]
        public string? Cep { get; set; }

        [MaxLength(200)]
        public string? Logradouro { get; set; }

        [MaxLength(20)]
        public string? Numero { get; set; }

        [MaxLength(100)]
        public string? Complemento { get; set; }

        [MaxLength(100)]
        public string? Bairro { get; set; }

        [MaxLength(100)]
        public string? Cidade { get; set; }

        [MaxLength(2)]
        public string? Uf { get; set; }

        [Required]
        [MaxLength(200)]
        public string Local { get; set; } = string.Empty;

        public Point? Coordenadas { get; set; }

        // --- Relacionamentos ---
        public virtual ICollection<MembroEquipeEntity> ParticipantesEquipe { get; set; } = new List<MembroEquipeEntity>();
        public virtual ICollection<DemandaEntity> DemandasGeradas { get; set; } = new List<DemandaEntity>();
        public virtual ICollection<EventoMovimentoEntity> Movimentacoes { get; set; } = new List<EventoMovimentoEntity>();

        /// <summary>
        /// Lista de relacionamento com os cabos eleitorais convidados
        /// </summary>
        public virtual ICollection<EventoCaboEleitoralEntity> CabosEleitoraisConvidados { get; set; } = new List<EventoCaboEleitoralEntity>();

        /// <summary>
        /// Define a partir de quantos minutos antes do evento o check-in estará liberado.
        /// </summary>
        [Display(Name = "Minutos de Antecedência para Check-in")]
        public int MinutosAntecedenciaCheckIn { get; set; } = 40;
    }
}