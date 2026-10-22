using System.ComponentModel.DataAnnotations;

namespace Domain.Enums;

public enum StatusEvento
{
    [Display(Name = "Agendado")]
    Agendado = 1,

    [Display(Name = "Em Andamento")]
    EmAndamento = 2,

    [Display(Name = "Concluído")]
    Concluido = 3,

    [Display(Name = "Cancelado")]
    Cancelado = 4
}