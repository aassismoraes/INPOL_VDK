using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class ResponsavelEquipeEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? Telefone { get; set; }

    [StringLength(100)]
    public string? FuncaoOuCargo { get; set; } // Ex: Assessor de Campo, Secretário, etc.

    public bool Ativo { get; set; } = true;

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }

    public virtual ICollection<DemandaEntity> DemandasAtribuidas { get; set; } = new List<DemandaEntity>();
}