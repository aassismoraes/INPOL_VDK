using Domain.Enums;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    [Table("demanda")]
    public class DemandaEntity : BaseEntity
    {
        // --- Colunas Mapeadas com a Tabela MySQL ---
        // (Removido [Required] para aceitar NULLs legados do banco)

        [MaxLength(2000)]
        [Column("descricao")]
        public string? Descricao { get; set; }

        [MaxLength(150)]
        [Column("Titulo")]
        public string? Titulo { get; set; }

        [MaxLength(50)]
        [Column("Protocolo")]
        public string? Protocolo { get; set; }

        [Column("StatusDemanda")]
        public StatusDemanda StatusDemanda { get; set; } = StatusDemanda.Aberta;

        [Column("DataTermino")]
        public DateTime? DataConclusaoPrevista { get; set; }

        [Column("DataInicio")]
        public DateTime? DataInicio { get; set; }

        [Column("DataHora")]
        public DateTime? DataHora { get; set; }

        [MaxLength(50)]
        [Column("UsuarioId")]
        public string? UsuarioId { get; set; }

        // --- Chaves Estrangeiras e Mapeamentos Relacionais ---

        [Column("PessoaId")]
        public Guid ApoiadorId { get; set; }

        [ForeignKey(nameof(ApoiadorId))]
        public virtual PessoaEntity Apoiador { get; set; } = null!;

        [Column("Responsavel")]
        public Guid? ResponsavelId { get; set; }

        [ForeignKey(nameof(ResponsavelId))]
        public virtual ResponsavelEquipeEntity? Responsavel { get; set; }

        [Column("EventoOrigemId")]
        public Guid? EventoOrigemId { get; set; }

        [ForeignKey(nameof(EventoOrigemId))]
        public virtual EventoEntity? EventoOrigem { get; set; }

        [Column("TipoDemandaId")]
        public Guid? TipoDemandaId { get; set; }

        [Column("StatusDemandaId")]
        public Guid? StatusDemandaId { get; set; }

        [Column("CategoriaTipoDemanda")]
        public Guid? CategoriaTipoDemandaId { get; set; }

        [Column("MembroEquipeEntityId")]
        public Guid? MembroEquipeEntityId { get; set; }

        // --- Propriedade de Atalho e Campos Utilitários ([NotMapped]) ---

        [NotMapped]
        public StatusDemanda Status
        {
            get => StatusDemanda;
            set => StatusDemanda = value;
        }

        [NotMapped]
        public Prioridade Prioridade { get; set; } = Prioridade.Media;

        [NotMapped] public string? Cep { get; set; }
        [NotMapped] public string? Logradouro { get; set; }
        [NotMapped] public string? Numero { get; set; }
        [NotMapped] public string? Complemento { get; set; }
        [NotMapped] public string? Uf { get; set; }
        [NotMapped] public string? Bairro { get; set; }
        [NotMapped] public string? Cidade { get; set; }
        [NotMapped] public Point? Coordenadas { get; set; }

        public virtual ICollection<DemandaMovimentacaoEntity> Movimentacoes { get; set; } = new List<DemandaMovimentacaoEntity>();
    }
}