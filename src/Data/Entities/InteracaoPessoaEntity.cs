using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

[Table("interacaopessoa")]
public class InteracaoPessoaEntity
{
    [Key]
    [Column("Id", TypeName = "char(36)")]
    public Guid Id { get; set; }

    [Required]
    [Column("PessoaId", TypeName = "char(36)")]
    public Guid PessoaId { get; set; }

    [Column("Mensagem", TypeName = "varchar(1000)")]
    public string? Mensagem { get; set; }

    [Column("TipoMensagem", TypeName = "varchar(20)")]
    public string? TipoMensagem { get; set; }

    [Column("TipoInteracao", TypeName = "varchar(20)")]
    public string? TipoInteracao { get; set; }

    [Column("StatusInteracao", TypeName = "varchar(30)")]
    public string? StatusInteracao { get; set; }

    [Column("DataHora", TypeName = "timestamp(6)")]
    public DateTime? DataHora { get; set; }

    [Column("Remetente", TypeName = "varchar(50)")]
    public string? Remetente { get; set; }

    [Column("Situacao", TypeName = "varchar(50)")]
    public string? Situacao { get; set; }

    [Column("AlteradoEm", TypeName = "datetime")]
    public DateTime? AlteradoEm { get; set; }

    [Column("Midia", TypeName = "longtext")]
    public string? Midia { get; set; }

    // Propriedade de Navegação (Opcional)
    [ForeignKey(nameof(PessoaId))]
    public virtual PessoaEntity? Pessoa { get; set; }
}