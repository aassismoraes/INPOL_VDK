using Domain.Apoiadores;
using Domain.Entities;
using Domain.Enums;
using Domain.Equipes;
using NetTopologySuite.Geometries;

namespace Domain.Atividades
{
    public class AtividadeCampo : BaseEntity
    {
        public DateTimeOffset DataHora { get; set; } = DateTimeOffset.UtcNow;
        public TipoAtividade TipoAtividade { get; set; }
        public string? Observacoes { get; set; }
        public Point? GeolocalizacaoCheckin { get; set; }

        public Guid MembroEquipeId { get; set; }
        public MembroEquipe MembroEquipe { get; set; } = null!;

        public Guid? ApoiadorId { get; set; }
        public Apoiador? Apoiador { get; set; }
    }
}
