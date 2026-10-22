using Domain.Apoiadores;
using Domain.Entities;
using Domain.Enums;
using Domain.Equipes;
using Domain.Eventos;

namespace Domain.Demandas
{
    public class Demanda : BaseEntity
    {
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Protocolo { get; set; } = string.Empty;
        public StatusDemanda Status { get; set; } = StatusDemanda.Aberta;
        public Prioridade Prioridade { get; set; } = Prioridade.Media;
        public string Bairro { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateOnly? DataConclusaoPrevista { get; set; }

        public Guid ApoiadorId { get; set; }
        public Apoiador Apoiador { get; set; } = null!;

        public Guid? ResponsavelId { get; set; }
        public MembroEquipe? Responsavel { get; set; }

        public Guid? EventoOrigemId { get; set; }
        public Evento? EventoOrigem { get; set; }
    }
}
