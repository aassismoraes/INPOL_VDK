using System.ComponentModel.DataAnnotations;

namespace Domain.Enums;

public enum TipoInteracao
{
    [Display(Name = "Avulso")]
    AVULSO = 1,

    [Display(Name = "Parabéns")]
    PARABENS = 2,

    [Display(Name = "Boas-Vindas 01")]
    BOAS_VINDAS_01 = 3,

    [Display(Name = "Boas-Vindas 02")]
    BOAS_VINDAS_02 = 4
}


public enum TipoMensagem
{
    [Display(Name = "Manual")]
    MANUAL = 1,

    [Display(Name = "Automática")]
    AUTOMATICA = 2
}


public enum StatusInteracao
{
    [Display(Name = "Enviado")]
    ENVIADO = 1,

    [Display(Name = "Enfileirado")]
    enfileirado = 2,

    [Display(Name = "Rejeitado (Sem WhatsApp)")]
    Rejeitado_Sem_WhatsApp = 3
}

public enum SituacaoInteracao
{
    [Display(Name = "Sem Resposta")]
    SEM_RESPOSTA = 1,

    [Display(Name = "Resposta Positiva")]
    RESPOSTA_POSITIVA = 2,

    [Display(Name = "Outros")]
    OUTROS = 3
}