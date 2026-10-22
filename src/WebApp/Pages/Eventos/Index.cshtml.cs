using System.Security.Claims;
using Data.Connections;
using Data.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages.Eventos;

[Authorize(Roles = "Administrador, Assessor, Atendimento, CaboEleitoral")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<EventoEntity> Eventos { get; set; } = default!;

    [BindProperty(SupportsGet = true)]
    public string? TermoBusca { get; set; }

    [BindProperty(SupportsGet = true, Name = "Periodo")]
    public string? Periodo { get; set; }

    [BindProperty(SupportsGet = true, Name = "filtro")]
    public string? Filtro { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Ordem { get; set; } = "data_asc";

    [BindProperty(SupportsGet = true, Name = "p")]
    public int PaginaAtual { get; set; } = 1;

    public int TotalEventos { get; set; }
    public int ExibindoCount { get; set; }
    public int TotalPaginas { get; set; }
    public const int TamanhoPagina = 6;

    public async Task OnGetAsync()
    {
        if (PaginaAtual < 1) PaginaAtual = 1;

        var filtroAtivo = !string.IsNullOrWhiteSpace(Periodo) ? Periodo : Filtro;

        var hoje = DateTimeOffset.Now.Date;
        var inicioHoje = new DateTimeOffset(hoje, TimeSpan.FromHours(-3));

        var query = _context.Eventos
            .AsNoTracking()
            .Where(e => e.Ativo);

        // ✅ CÓDIGO NOVO (Opção A)
        if (User.IsInRole(nameof(PerfilAcesso.CaboEleitoral)))
        {
            var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;

            if (!string.IsNullOrEmpty(emailClaim))
            {
                // Resolve o ID do Cabo Eleitoral filtrando pelo e-mail do usuário autenticado
                var caboEleitoralId = await _context.Set<CaboEleitoralEntity>()
                    .AsNoTracking()
                    .Where(c => c.Email != null && c.Email.ToLower() == emailClaim.ToLower())
                    .Select(c => (Guid?)c.Id)
                    .FirstOrDefaultAsync();

                if (caboEleitoralId.HasValue)
                {
                    query = query.Where(e => e.CabosEleitoraisConvidados
                        .Any(c => c.CaboEleitoralId == caboEleitoralId.Value));
                }
                else
                {
                    // Se o e-mail não bater com nenhum cabo eleitoral cadastrado, não exibe eventos
                    query = query.Where(e => false);
                }
            }
            else
            {
                query = query.Where(e => false);
            }
        }

        // 1. Filtro Temporal (Hoje ou Próximos 7 dias / Semana)
        if (!string.IsNullOrWhiteSpace(filtroAtivo))
        {
            if (filtroAtivo.Equals("Hoje", StringComparison.OrdinalIgnoreCase))
            {
                var fimHoje = inicioHoje.AddDays(1).AddTicks(-1);
                query = query.Where(e => e.DataHoraInicio >= inicioHoje && e.DataHoraInicio <= fimHoje);
            }
            else if (filtroAtivo.Equals("Semana", StringComparison.OrdinalIgnoreCase))
            {
                var fimSemana = inicioHoje.AddDays(7);
                query = query.Where(e => e.DataHoraInicio >= inicioHoje && e.DataHoraInicio <= fimSemana);
            }
        }

        // 2. Filtro de Busca por Texto
        if (!string.IsNullOrWhiteSpace(TermoBusca))
        {
            var termo = TermoBusca.Trim().ToLower();
            query = query.Where(e => e.Titulo.ToLower().Contains(termo) ||
                                     (e.Local != null && e.Local.ToLower().Contains(termo)) ||
                                     (e.Descricao != null && e.Descricao.ToLower().Contains(termo)));
        }

        TotalEventos = await query.CountAsync();
        TotalPaginas = (int)Math.Ceiling(TotalEventos / (double)TamanhoPagina);

        if (PaginaAtual > TotalPaginas && TotalPaginas > 0)
        {
            PaginaAtual = TotalPaginas;
        }

        // 3. Ordenação Dinâmica
        query = Ordem.ToLower() switch
        {
            "data_desc" => query.OrderByDescending(e => e.DataHoraInicio),
            "titulo_asc" => query.OrderBy(e => e.Titulo),
            "criado_desc" => query.OrderByDescending(e => e.CriadoEm),
            _ => query.OrderBy(e => e.DataHoraInicio)
        };

        Eventos = await query
            .Skip((PaginaAtual - 1) * TamanhoPagina)
            .Take(TamanhoPagina)
            .ToListAsync();

        ExibindoCount = Eventos.Count;
        Filtro = filtroAtivo;
    }
}