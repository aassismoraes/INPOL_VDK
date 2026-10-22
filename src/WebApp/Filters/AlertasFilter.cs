using Data.Connections;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Filters
{
    public class AlertasFilter : IAsyncPageFilter
    {
        private readonly ApplicationDbContext _context;

        public AlertasFilter(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            // Executa apenas se o usuário estiver autenticado
            if (context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                // Calcula a data atual e o limite dos próximos 3 dias no fuso de Brasília (UTC-3)
                var hoje = DateTime.UtcNow.AddHours(-3).Date;
                var limiteTresDias = hoje.AddDays(3);

                // IDs ou Valores numéricos dos status a serem ignorados (ex: Atendida/Concluída=4, Cancelada=5)
                // Se a propriedade na entidade for StatusDemandaId (int / Guid) ou Status (enum mapeado):
                var statusIgnorados = new[] { (int)StatusDemanda.Atendida, (int)StatusDemanda.NaoAtendida, (int)StatusDemanda.Cancelada };

                // 1. Contagem de Demandas Vencidas (anteriores a hoje), ignorando Concluídas e Canceladas
                //var demandasVencidas = await _context.Demandas
                //    .AsNoTracking()
                //    .CountAsync(d => d.StatusDemandaId.HasValue
                //                  && !statusIgnorados.Contains((int)d.StatusDemanda)
                //                  && d.DataConclusaoPrevista.HasValue
                //                  && d.DataConclusaoPrevista < hoje);

                ////// 2. Contagem de Demandas que vencem de HOJE até os PRÓXIMOS 3 DIAS, ignorando Concluídas e Canceladas
                //var demandasVencendo3Dias = await _context.Demandas
                //    .AsNoTracking()
                //    .CountAsync(d => d.StatusDemandaId.HasValue
                //                  && !statusIgnorados.Contains((int)d.StatusDemanda)
                //                  && d.DataConclusaoPrevista.HasValue
                //                  && d.DataConclusaoPrevista >= hoje
                //                  && d.DataConclusaoPrevista <= limiteTresDias);

                // 3. Contagem de Eventos/Compromissos para Hoje
                var eventosHoje = await _context.Eventos
                    .AsNoTracking()
                    .CountAsync(e => e.DataHoraInicio >= hoje && e.DataHoraInicio < hoje.AddDays(1));

                // =========================================================
                // 4. CONTAGEM DE APOIADORES AGUARDANDO APROVAÇÃO
                // =========================================================
                int pendentesAprovacao = 0;
                var user = context.HttpContext.User;

                // Verifica se o usuário tem perfil para aprovar (Administrador ou Assessor)
                if (user.IsInRole("Administrador") || user.IsInRole("AssessorGabinete") || user.IsInRole("Assessor"))
                {
                    pendentesAprovacao = await _context.Pessoas
                        .AsNoTracking()
                        .CountAsync(p => p.StatusTexto == "AGUARDANDO APROVAÇÃO");
                }

                // Injeta os valores no ViewData lido pelo _Layout.cshtml
                if (context.HandlerInstance is PageModel pageModel)
                {
                    //pageModel.ViewData["DemandasVencidas"] = demandasVencidas;
                    //pageModel.ViewData["DemandasVencendo3Dias"] = demandasVencendo3Dias;

                    pageModel.ViewData["EventosHoje"] = eventosHoje;
                    pageModel.ViewData["PendentesAprovacao"] = pendentesAprovacao;
                }
            }

            await next();
        }
    }
}