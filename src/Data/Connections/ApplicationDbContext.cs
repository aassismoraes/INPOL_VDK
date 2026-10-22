using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq.Expressions;
using BaseEntity = Data.Entities.BaseEntity;

namespace Data.Connections
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Conjuntos de Dados (DbSet)
        public DbSet<PoliticoEntity> Politicos => Set<PoliticoEntity>();
        public DbSet<PessoaEntity> Pessoas => Set<PessoaEntity>();
        public DbSet<TagEntity> Tags => Set<TagEntity>();
        public DbSet<DemandaEntity> Demandas => Set<DemandaEntity>();
        public DbSet<EventoEntity> Eventos => Set<EventoEntity>();
        public DbSet<MembroEquipeEntity> MembrosEquipe => Set<MembroEquipeEntity>();
        public DbSet<AtividadeCampoEntity> AtividadesCampo => Set<AtividadeCampoEntity>();
        public DbSet<UsuarioEntity> Usuarios => Set<UsuarioEntity>();
        public DbSet<DemandaMovimentacaoEntity> DemandasMovimentacoes => Set<DemandaMovimentacaoEntity>();
        public DbSet<ResponsavelEquipeEntity> ResponsaveisEquipe => Set<ResponsavelEquipeEntity>();
        public DbSet<EventoMovimentoEntity> EventosMovimentacoes => Set<EventoMovimentoEntity>();
        public DbSet<EventoCaboEleitoralEntity> EventoCaboEleitoral => Set<EventoCaboEleitoralEntity>();
        public DbSet<CaboEleitoralEntity> CabosEleitorais => Set<CaboEleitoralEntity>();
        public DbSet<InteracaoPessoaEntity> InteracoesPessoa => Set<InteracaoPessoaEntity>();
        public DbSet<InteracaoPessoaLogEntity> InteracoesPessoaLogs => Set<InteracaoPessoaLogEntity>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // Silencia os avisos de navegação com filtro de consulta global
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(
                    new Microsoft.Extensions.Logging.EventId(20500),
                    CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. DATA SEEDING (Politico e Usuario Administrador)
            var politicoPadraoId = Guid.Parse("f2c3d4e5-f6a7-8b9c-0d1e-2f3a4b5c6d7e");

            modelBuilder.Entity<PoliticoEntity>().HasData(new PoliticoEntity
            {
                Id = politicoPadraoId,
                Nome = "João da Silva",
                NomeEleitoral = "João do Povo",
                CargoAtual = "Deputado Estadual",
                Partido = "PL",
                PossuiMandato = true,
                Cpf = "12345678901",
                LimiteUsuarios = 10,
                FotoUrl = "https://example.com/fotos/joao-do-povo.jpg",
                Ativo = true,
                CriadoEm = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                AtualizadoEm = null,
                DeletadoEm = null
            });

            modelBuilder.Entity<UsuarioEntity>().HasData(new UsuarioEntity
            {
                Id = Guid.Parse("a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"),
                Nome = "Administrador",
                Email = "moraesalex409@gmail.com",
                GoogleId = null,
                AvatarUrl = null,
                Telefone = null,
                Perfil = PerfilAcesso.Administrador,
                Ativo = true,
                CriadoEm = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                AtualizadoEm = null,
                UltimoAcesso = null
            });

            // 2. CONFIGURAÇÕES GENÉRICAS (BaseEntity + Soft Delete)
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(nameof(BaseEntity.CriadoEm))
                        .IsRequired();

                    modelBuilder.Entity(entityType.ClrType)
                        .Property(nameof(BaseEntity.Ativo))
                        .HasDefaultValue(true);

                    // Expressão do Filtro Global Genérico (Ativo == true para TODAS as entidades BaseEntity)
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var isAtivoProperty = Expression.Property(parameter, nameof(BaseEntity.Ativo));
                    var compareExpression = Expression.Equal(isAtivoProperty, Expression.Constant(true));
                    var lambda = Expression.Lambda(compareExpression, parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

        // 3. SOFT DELETE AUTOMÁTICO
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplySoftDeleteRules();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            ApplySoftDeleteRules();
            return base.SaveChanges();
        }

        private void ApplySoftDeleteRules()
        {
            var agora = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is BaseEntity entity)
                {
                    if (entry.State == EntityState.Deleted)
                    {
                        entry.State = EntityState.Modified;
                        entity.Ativo = false;
                        entity.DeletadoEm = agora;
                        entity.AtualizadoEm = agora;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        entity.AtualizadoEm = agora;
                    }
                }
            }
        }
    }
}