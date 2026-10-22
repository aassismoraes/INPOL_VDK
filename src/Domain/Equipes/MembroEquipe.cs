using Domain.Atividades;
using Domain.Demandas;
using Domain.Entities;
using Domain.Enums;
using Domain.Eventos;
using Domain.Politicos;

namespace Domain.Equipes
{
    public class MembroEquipe : BaseEntity
    {
        public Guid PoliticoId { get; set; }
        public Politico Politico { get; set; } = null!;

        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public NivelAcesso Permissao { get; set; }

        public ICollection<Demanda> DemandasAtribuidas { get; set; } = new List<Demanda>();
        public ICollection<AtividadeCampo> AtividadesRealizadas { get; set; } = new List<AtividadeCampo>();
        public ICollection<Evento> EventosParticipando { get; set; } = new List<Evento>();
    }
}
