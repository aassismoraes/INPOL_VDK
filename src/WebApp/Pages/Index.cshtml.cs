using Data.Connections;
using Data.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    // Métricas de Eventos
    public int TotalEventosHoje { get; set; }
    public int TotalEventosSemana { get; set; }
    public List<EventoEntity> ProximosEventos { get; set; } = new();

    // Métricas de Demandas
    public int TotalDemandas { get; set; }
    public int DemandasPendentes { get; set; }
    public int DemandasConcluidas { get; set; }

    // Métrica de Apoiadores
    public int TotalApoiadores { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // Redireciona Cabo Eleitoral para a Agenda/Eventos
        if (User.IsInRole(nameof(PerfilAcesso.CaboEleitoral)))
        {
            return RedirectToPage("/Eventos/Index");
        }

        var hoje = DateTimeOffset.Now.Date;
        var inicioHoje = new DateTimeOffset(hoje, TimeSpan.FromHours(-3));
        var fimHoje = inicioHoje.AddDays(1).AddTicks(-1);
        var fimSemana = inicioHoje.AddDays(7);

        // 1. Métricas de Eventos
        TotalEventosHoje = await _context.Eventos
            .AsNoTracking()
            .CountAsync(e => e.Ativo && e.DataHoraInicio >= inicioHoje && e.DataHoraInicio <= fimHoje);

        TotalEventosSemana = await _context.Eventos
            .AsNoTracking()
            .CountAsync(e => e.Ativo && e.DataHoraInicio >= inicioHoje && e.DataHoraInicio <= fimSemana);

        ProximosEventos = await _context.Eventos
            .AsNoTracking()
            .Where(e => e.Ativo && e.DataHoraInicio >= inicioHoje)
            .OrderBy(e => e.DataHoraInicio)
            .Take(5)
            .ToListAsync();

        // 2. Métricas de Demandas (Consultando colunas físicas mapeadas)
        //TotalDemandas = await _context.Demandas
        //    .AsNoTracking()
        //    .CountAsync(d => d.Ativo);

        //// Demandas concluídas filtram pela presença de DataConclusaoPrevista/DataTermino ou agrupamento por StatusDemandaId
        //DemandasConcluidas = await _context.Demandas
        //    .AsNoTracking()
        //    .CountAsync(d => d.Ativo && d.DataConclusaoPrevista.HasValue && d.DataConclusaoPrevista <= DateTime.Now);

        DemandasPendentes = TotalDemandas - DemandasConcluidas;

        // 3. Métrica de Apoiadores
        TotalApoiadores = await _context.Pessoas
            .AsNoTracking()
            .CountAsync(a => a.StatusTexto == "ATIVO");

        return Page();
    }
}