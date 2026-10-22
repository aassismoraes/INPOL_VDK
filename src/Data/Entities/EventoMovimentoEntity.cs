using CrossCutting.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class EventoMovimentoEntity : BaseEntity
{
    public Guid EventoId { get; set; }
    public virtual EventoEntity Evento { get; set; } = null!;

    [Required]
    public string Descricao { get; set; } = string.Empty;

    public string? UsuarioNome { get; set; }

    public DateTime DataMovimento { get; set; } 
}