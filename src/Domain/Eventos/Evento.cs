using Domain.Demandas;
using Domain.Entities;
using Domain.Enums;
using Domain.Equipes;
using Domain.Politicos;
using NetTopologySuite.Geometries;

namespace Domain.Eventos
{
    public class Evento : BaseEntity
    {
        public Guid PoliticoId { get; set; }
        public Politico Politico { get; set; } = null!;

        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public DateTimeOffset DataHoraInicio { get; set; }
        public DateTimeOffset DataHoraFim { get; set; }
        public string Local { get; set; } = string.Empty;
        public Point? Coordenadas { get; set; }
        public TipoEvento Tipo { get; set; }

        public ICollection<MembroEquipe> ParticipantesEquipe { get; set; } = new List<MembroEquipe>();
        public ICollection<Demanda> DemandasGeradas { get; set; } = new List<Demanda>();
    }
}
