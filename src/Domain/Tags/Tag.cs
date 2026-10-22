using Domain.Apoiadores;
using Domain.Entities;

namespace Domain.Tags
{
    public class Tag : BaseEntity
    {
        public string Nome { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;

        public ICollection<Apoiador> Apoiadores { get; set; } = new List<Apoiador>();
    }
}
