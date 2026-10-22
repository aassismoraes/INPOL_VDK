using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public abstract class BaseEntity
    {
        public virtual Guid Id { get; set; }
        public virtual DateTime CriadoEm { get; set; }
        public virtual DateTime? AtualizadoEm { get; set; }
        public virtual DateTime? DeletadoEm { get; set; }
        public virtual bool Ativo { get; set; } = true; 
    }
}