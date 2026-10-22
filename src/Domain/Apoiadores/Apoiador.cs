using Domain.Atividades;
using Domain.Demandas;
using Domain.Entities;
using Domain.Enums;
using Domain.Politicos;
using Domain.Tags;
using NetTopologySuite.Geometries;

namespace Domain.Apoiadores
{
    public class Apoiador : BaseEntity
    {
        public Guid PoliticoId { get; set; }
        public Politico Politico { get; set; } = null!;

        public string Nome { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public DateOnly? DataNascimento { get; set; }
        public StatusVoto StatusVoto { get; set; } = StatusVoto.Indeciso;
        public NivelLideranca NivelLideranca { get; set; } = NivelLideranca.Eleitor;

        public string Bairro { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public Point? Coordenadas { get; set; }

        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public ICollection<Demanda> Demandas { get; set; } = new List<Demanda>();
        public ICollection<AtividadeCampo> AtividadesRecebidas { get; set; } = new List<AtividadeCampo>();
    }
}
