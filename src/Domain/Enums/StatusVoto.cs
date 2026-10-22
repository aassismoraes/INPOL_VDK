namespace Domain.Enums
{
    public enum StatusVoto
    {
        Desconhecido = 0,
        Certo = 1,
        Provavel = 2,
        Indeciso = 3,
        Oposicao = 4
    }

    public enum NivelLideranca
    {
        Desconhecido = 0,
        Eleitor = 1,
        Mobilizador = 2,
        LiderComunitaria = 3,
        CoordenadorRegional = 4
    }

    public enum StatusDemanda
    {
        Aberta = 1,
        EmAndamento = 2,
        EncaminhadaGabinete = 3,
        Atendida = 4,
        Cancelada = 5,
        NaoAtendida = 6,
        Aguardando = 7
    }

    public enum Prioridade
    {
        Baixa = 1,
        Media = 2,
        Alta = 3,
        Urgente = 4
    }

    public enum TipoEvento
    {
        ReuniaoComunitaria = 1,
        Caminhada = 2,
        SessaoGabinete = 3,
        Entrevista = 4,
        EventoPartidario = 5
    }

    public enum TipoAtividade
    {
        VisitaBairro = 1,
        Panfletagem = 2,
        AtendimentoDemanda = 3,
        CadastroApoiador = 4
    }

    public enum NivelAcesso
    {
        Administrador = 1,
        AssessorGabinete = 2,
        LiderCampo = 3,
        Voluntario = 4
    }
}
