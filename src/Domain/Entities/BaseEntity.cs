using Notification;

namespace Domain.Entities
{
    public abstract class BaseEntity : Notifiable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? AtualizadoEm { get; set; }
        public bool Ativo { get; set; } = true;
    }
}
