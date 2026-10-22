using Domain.Enums;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    [Table("pessoa")]
    public class PessoaEntity
    {
        // =========================================================================
        // CHAVE PRIMÁRIA & CONTROLE DE DATAS
        // =========================================================================

        [Key]
        [Column("Id", TypeName = "char(36)")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("DataCadastro")]
        public DateTime? CriadoEm { get; set; }

        [Column("ModificadoEm")]
        public DateTime? AtualizadoEm { get; set; }

        [Required]
        [MaxLength(45)]
        [Column("Status")]
        public string StatusTexto { get; set; } = "ATIVO";

        [NotMapped]
        public bool Ativo
        {
            get => StatusTexto == "ATIVO";
            set => StatusTexto = value ? "ATIVO" : "INATIVO";
        }


        // =========================================================================
        // TENANT & CABO ELEITORAL (PADRINHO)
        // =========================================================================

        /// <summary>
        /// Identificador do Tenant/Político.
        /// </summary>
        [Column("PoliticoId")]
        public Guid? PoliticoId { get; set; }

        [ForeignKey(nameof(PoliticoId))]
        public virtual PoliticoEntity? Politico { get; set; }

        /// <summary>
        /// FK para a tabela 'padrinho' (NOT NULL na estrutura do banco).
        /// </summary>
        [Required]
        [Column("PadrinhoId", TypeName = "char(36)")]
        public Guid PadrinhoId { get; set; }

        [ForeignKey(nameof(PadrinhoId))]
        public virtual CaboEleitoralEntity Padrinho { get; set; } = null!;


        // =========================================================================
        // DADOS PESSOAIS
        // =========================================================================

        [Required]
        [MaxLength(50)]
        [Column("Nome")]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(20)]
        [Column("Cpf")]
        public string? Cpf { get; set; }

        [MaxLength(50)]
        [Column("Apelido")]
        public string? Apelido { get; set; }

        [Required]
        [MaxLength(15)]
        [Column("Genero")]
        public string Genero { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("Profissao")]
        public string? Profissao { get; set; }

        [Column("DataNascimento")]
        public DateTime? DataNascimento { get; set; }

        [MaxLength(50)]
        [Column("Email")]
        public string? Email { get; set; }

        [Required]
        [MaxLength(11)]
        [Column("Celular")]
        public string Celular { get; set; } = string.Empty;

        [MaxLength(11)]
        [Column("Telefone")]
        public string? Telefone { get; set; }

        [MaxLength(20)]
        [Column("PossuiZap")]
        public string? PossuiZap { get; set; }

        [MaxLength(45)]
        [Column("Usuario")]
        public string? Usuario { get; set; }


        // =========================================================================
        // ENDEREÇO & GEOLOCALIZAÇÃO
        // =========================================================================

        [MaxLength(9)]
        [Column("Cep")]
        public string? Cep { get; set; }

        [MaxLength(100)]
        [Column("Logradouro")]
        public string? Logradouro { get; set; }

        [MaxLength(20)]
        [Column("Numero")]
        public string? Numero { get; set; }

        [MaxLength(50)]
        [Column("Complemento")]
        public string? Complemento { get; set; }

        [MaxLength(100)]
        [Column("Bairro")]
        public string? Bairro { get; set; }

        [MaxLength(100)]
        [Column("Distrito")]
        public string? Distrito { get; set; }

        [MaxLength(40)]
        [Column("Cidade")]
        public string? Cidade { get; set; }

        [MaxLength(2)]
        [Column("Uf")]
        public string? Uf { get; set; }

        [NotMapped]
        public Point? Coordenadas { get; set; }


        // =========================================================================
        // METADADOS E ATRIBUTOS LEGADOS
        // =========================================================================

        [MaxLength(1000)]
        [Column("Tags")]
        public string? TagsTexto { get; set; }

        [MaxLength(2000)]
        [Column("Observacoes")]
        public string? Observacoes { get; set; }

        [MaxLength(45)]
        [Column("GrauSatisfacao")]
        public string? GrauSatisfacao { get; set; }

        [MaxLength(20)]
        [Column("Fidelizado")]
        public string? Fidelizado { get; set; }

        [MaxLength(45)]
        [Column("ResponsavelCadastro")]
        public string? ResponsavelCadastro { get; set; }

        [MaxLength(45)]
        [Column("ResponsavelStatus")]
        public string? ResponsavelStatus { get; set; }

        [MaxLength(1000)]
        [Column("DescricaoStatus")]
        public string? DescricaoStatus { get; set; }

        [Column("DataStatus")]
        public DateTime? DataStatus { get; set; }

        [NotMapped]
        public StatusVoto StatusVoto { get; set; } = StatusVoto.Indeciso;

        [NotMapped]
        public NivelLideranca NivelLideranca { get; set; } = NivelLideranca.Eleitor;


        // =========================================================================
        // RELACIONAMENTOS / COLEÇÕES
        // =========================================================================

        public virtual ICollection<TagEntity> Tags { get; set; } = new List<TagEntity>();
        public virtual ICollection<DemandaEntity> Demandas { get; set; } = new List<DemandaEntity>();
        public virtual ICollection<AtividadeCampoEntity> AtividadesRecebidas { get; set; } = new List<AtividadeCampoEntity>();
    }
}