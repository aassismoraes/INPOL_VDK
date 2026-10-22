using CrossCutting.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public enum PerfilAcesso
{
    Administrador = 1,
    Assessor = 2,
    Atendimento = 3,
    CaboEleitoral = 4
}

[Table("Usuarios")]
public class UsuarioEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? GoogleId { get; set; }

    public string? AvatarUrl { get; set; }

    [MaxLength(20)]
    public string? Telefone { get; set; }

    [Required]
    public PerfilAcesso Perfil { get; set; } = PerfilAcesso.Assessor;

    public bool Ativo { get; set; } = true;

    public DateTime CriadoEm { get; set; } = DateTimeExtensions.ObterDataHoraBrasilia();

    // Propriedade que estava faltando no seu modelo:
    public DateTime? AtualizadoEm { get; set; }

    public DateTime? UltimoAcesso { get; set; }
}