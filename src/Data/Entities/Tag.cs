using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    [Table("Tags")]
    public class TagEntity : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Categoria { get; set; } = string.Empty;

        public virtual ICollection<PessoaEntity> Apoiadores { get; set; } = new List<PessoaEntity>();
    }
}
