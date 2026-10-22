using Domain.Apoiadores;
using Domain.Entities;
using Domain.Equipes;
using Domain.Eventos;

namespace Domain.Politicos
{
    public class Politico : BaseEntity
    {
        public string NomeEleitoral { get; set; } = string.Empty;
        public string? CargoAtual { get; set; }
        public string Partido { get; set; } = string.Empty;
        public bool PossuiMandato { get; set; }
        public string Cpf { get; set; } = string.Empty;
        public string? FotoUrl { get; set; }

        public ICollection<MembroEquipe> MembrosEquipe { get; set; } = new List<MembroEquipe>();
        public ICollection<Apoiador> Apoiadores { get; set; } = new List<Apoiador>();
        public ICollection<Evento> Eventos { get; set; } = new List<Evento>();
    }
}
