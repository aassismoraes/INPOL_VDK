using CrossCutting.Helpers;
using Data.Connections;
using Data.Entities;
using Domain.Enums;
using Domain.Eventos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace WebApp.Pages.Eventos;

[Authorize(Roles = "Administrador, Assessor, Atendimento, CaboEleitoral")]
public class DetalhesModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DetalhesModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public EventoEntity Evento { get; set; } = default!;

    public bool PodeFazerCheckIn { get; set; }
    public bool JaFezCheckIn { get; set; }
    public bool ConfirmouPresencaAntesDoEvento { get; set; }
    public bool PodeDesconfirmarPresenca { get; set; }
    public bool EventoJaIniciou { get; set; }
    public bool EventoCancelado { get; set; }

    // Listas para a visão Administrativa
    public List<EventoCaboEleitoralEntity> CabosConfirmados { get; set; } = new();
    public List<EventoCaboEleitoralEntity> CabosCheckIn { get; set; } = new();
    public List<EventoCaboEleitoralEntity> CabosDesconfirmados { get; set; } = new();

    [BindProperty]
    [Required(ErrorMessage = "Digite o texto da movimentação antes de registrar.")]
    [Display(Name = "Movimentação")]
    public string NovaMovimentacao { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Informe o motivo do cancelamento da presença.")]
    public string MotivoDesconfirmacao { get; set; } = string.Empty;

    [BindProperty]
    public double Latitude { get; set; }

    [BindProperty]
    public double Longitude { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var evento = await _context.Eventos
            .Include(e => e.Movimentacoes.Where(m => m.Ativo).OrderByDescending(m => m.DataMovimento))
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evento == null) return NotFound();

        Evento = evento;

        // Caso a propriedade do modelo de dados se chame Cancelado ou StatusEvento.Cancelado,
        // ajuste conforme a sua enum ou propriedade booleana correspondente:
        // Exemplo: EventoCancelado = evento.Cancelado; ou evento.Status == StatusEvento.Cancelado;
        EventoCancelado = evento.Status == StatusEvento.Cancelado;

        var agoraBrasilia = DateTimeExtensions.ObterDataHoraBrasilia();
        EventoJaIniciou = agoraBrasilia >= evento.DataHoraInicio;

        // Se for Perfil Administrativo, carrega as listas dos modais
        if (!User.IsInRole(nameof(PerfilAcesso.CaboEleitoral)))
        {
            CabosConfirmados = await _context.Set<EventoCaboEleitoralEntity>()
                .Include(p => p.CaboEleitoral)
                .Where(p => p.EventoId == id && p.PresencaConfirmada && !p.Desconfirmado)
                .AsNoTracking()
                .ToListAsync();

            CabosCheckIn = await _context.Set<EventoCaboEleitoralEntity>()
                .Include(p => p.CaboEleitoral)
                .Where(p => p.EventoId == id && p.DataCheckIn != null)
                .AsNoTracking()
                .ToListAsync();

            CabosDesconfirmados = await _context.Set<EventoCaboEleitoralEntity>()
                .Include(p => p.CaboEleitoral)
                .Where(p => p.EventoId == id && p.Desconfirmado)
                .AsNoTracking()
                .ToListAsync();
        }

        // Lógica para o perfil Cabo Eleitoral
        var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
        if (User.IsInRole(nameof(PerfilAcesso.CaboEleitoral)) && !string.IsNullOrEmpty(emailClaim))
        {
            var cabo = await _context.Set<CaboEleitoralEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Email != null && c.Email.ToLower() == emailClaim.ToLower());

            if (cabo != null)
            {
                var vinculo = await _context.Set<EventoCaboEleitoralEntity>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.EventoId == id && v.CaboEleitoralId == cabo.Id);

                JaFezCheckIn = vinculo?.DataCheckIn != null;
                ConfirmouPresencaAntesDoEvento = vinculo != null && vinculo.PresencaConfirmada && !vinculo.Desconfirmado;
                PodeDesconfirmarPresenca = ConfirmouPresencaAntesDoEvento && !EventoJaIniciou && !EventoCancelado;

                var minutosAntecedencia = evento.MinutosAntecedenciaCheckIn > 0 ? evento.MinutosAntecedenciaCheckIn : 20;
                var inicioJanelaCheckIn = evento.DataHoraInicio.AddMinutes(-minutosAntecedencia);

                bool dentroDaJanelaHorario = agoraBrasilia >= inicioJanelaCheckIn &&
                    (!evento.DataHoraFim.HasValue || agoraBrasilia <= evento.DataHoraFim.Value);

                PodeFazerCheckIn = dentroDaJanelaHorario && !JaFezCheckIn && !EventoCancelado;
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostConfirmarPresencaAsync(Guid id)
    {
        LimparValidacaoCampos();

        var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.Id == id);
        if (evento == null) return NotFound();

        if (evento.Status == StatusEvento.Cancelado)
        {
            TempData["ErrorMessage"] = "Não é possível confirmar presença num evento cancelado.";
            return RedirectToPage(new { id });
        }

        var agoraBrasilia = DateTimeExtensions.ObterDataHoraBrasilia();
        if (agoraBrasilia >= evento.DataHoraInicio)
        {
            TempData["ErrorMessage"] = "Não é possível confirmar presença após o início do evento.";
            return RedirectToPage(new { id });
        }

        var cabo = await ObterCaboLogadoAsync();
        if (cabo == null) return RedirectToPage(new { id });

        var vinculo = await _context.Set<EventoCaboEleitoralEntity>()
            .FirstOrDefaultAsync(v => v.EventoId == id && v.CaboEleitoralId == cabo.Id);

        if (vinculo == null)
        {
            vinculo = new EventoCaboEleitoralEntity { EventoId = id, CaboEleitoralId = cabo.Id };
            _context.Set<EventoCaboEleitoralEntity>().Add(vinculo);
        }

        vinculo.PresencaConfirmada = true;
        vinculo.Desconfirmado = false;
        vinculo.MotivoDesconfirmacao = null;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Presença confirmada com sucesso!";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDesconfirmarPresencaAsync(Guid id)
    {
        ModelState.ClearValidationState(nameof(NovaMovimentacao));
        ModelState.MarkFieldValid(nameof(NovaMovimentacao));

        if (string.IsNullOrWhiteSpace(MotivoDesconfirmacao))
        {
            TempData["ErrorMessage"] = "É necessário informar o motivo do cancelamento.";
            return RedirectToPage(new { id });
        }

        var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.Id == id);
        if (evento == null) return NotFound();

        if (evento.Status == StatusEvento.Cancelado)
        {
            TempData["ErrorMessage"] = "Não é possível alterar a presença num evento cancelado.";
            return RedirectToPage(new { id });
        }

        var agoraBrasilia = DateTimeExtensions.ObterDataHoraBrasilia();
        if (agoraBrasilia >= evento.DataHoraInicio)
        {
            TempData["ErrorMessage"] = "Não é mais possível cancelar a presença pois o evento já iniciou.";
            return RedirectToPage(new { id });
        }

        var cabo = await ObterCaboLogadoAsync();
        if (cabo == null) return RedirectToPage(new { id });

        var vinculo = await _context.Set<EventoCaboEleitoralEntity>()
            .FirstOrDefaultAsync(v => v.EventoId == id && v.CaboEleitoralId == cabo.Id);

        if (vinculo != null)
        {
            vinculo.PresencaConfirmada = false;
            vinculo.Desconfirmado = true;
            vinculo.MotivoDesconfirmacao = MotivoDesconfirmacao.Trim();
            vinculo.DataDesconfirmacao = agoraBrasilia;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Sua presença foi desconfirmada.";
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostCheckInAsync(Guid id)
    {
        LimparValidacaoCampos();

        var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.Id == id);
        if (evento == null) return NotFound();

        if (evento.Status == StatusEvento.Cancelado)
        {
            TempData["ErrorMessage"] = "Não é possível realizar check-in num evento cancelado.";
            return RedirectToPage(new { id });
        }

        var agora = DateTimeExtensions.ObterDataHoraBrasilia();

        var minutosAntecedencia = evento.MinutosAntecedenciaCheckIn > 0 ? evento.MinutosAntecedenciaCheckIn : 20;
        var inicioJanelaCheckIn = evento.DataHoraInicio.AddMinutes(-minutosAntecedencia);

        if (agora < inicioJanelaCheckIn)
        {
            TempData["ErrorMessage"] = $"O check-in só é permitido com até {minutosAntecedencia} minutos de antecedência do início do evento.";
            return RedirectToPage(new { id });
        }

        if (evento.DataHoraFim.HasValue && agora > evento.DataHoraFim.Value)
        {
            TempData["ErrorMessage"] = "O check-in não está mais disponível pois o evento já encerrou.";
            return RedirectToPage(new { id });
        }

        var cabo = await ObterCaboLogadoAsync();
        if (cabo == null) return RedirectToPage(new { id });

        double distanciaCalculada = 0;

        if (evento.Coordenadas != null)
        {
            distanciaCalculada = CalcularDistanciaMetros(evento.Coordenadas.Y, evento.Coordenadas.X, Latitude, Longitude);

            if (distanciaCalculada > 100)
            {
                TempData["ErrorMessage"] = $"Você está a {Math.Round(distanciaCalculada)}m do local. É necessário estar no raio de 100m para confirmar presença.";
                return RedirectToPage(new { id });
            }
        }

        var vinculo = await _context.Set<EventoCaboEleitoralEntity>()
            .FirstOrDefaultAsync(v => v.EventoId == id && v.CaboEleitoralId == cabo.Id);

        if (vinculo == null)
        {
            vinculo = new EventoCaboEleitoralEntity { EventoId = id, CaboEleitoralId = cabo.Id };
            _context.Set<EventoCaboEleitoralEntity>().Add(vinculo);
        }

        vinculo.PresencaConfirmada = true;
        vinculo.DataCheckIn = agora;
        vinculo.LatitudeCheckIn = Latitude;
        vinculo.LongitudeCheckIn = Longitude;
        vinculo.DistanciaMetros = distanciaCalculada;

        var usuarioNome = User.FindFirstValue(ClaimTypes.Name) ?? cabo.Nome ?? "Cabo Eleitoral";
        _context.Set<EventoMovimentoEntity>().Add(new EventoMovimentoEntity
        {
            EventoId = id,
            Descricao = $"📍 Check-in de presença efetuado via GPS ({Math.Round(distanciaCalculada)}m do local)",
            UsuarioNome = usuarioNome,
            DataMovimento = agora,
            Ativo = true
        });

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Check-in presencial registrado com sucesso!";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostAdicionarMovimentoAsync(Guid id)
    {
        ModelState.ClearValidationState(nameof(MotivoDesconfirmacao));
        ModelState.MarkFieldValid(nameof(MotivoDesconfirmacao));

        if (string.IsNullOrWhiteSpace(NovaMovimentacao))
        {
            TempData["ErrorMessage"] = "Digite o texto da movimentação antes de registrar.";
            return RedirectToPage(new { id });
        }

        var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.Id == id);
        if (evento == null) return NotFound();

        if (evento.Status == StatusEvento.Cancelado)
        {
            TempData["ErrorMessage"] = "Não é possível adicionar movimentos a um evento cancelado.";
            return RedirectToPage(new { id });
        }

        var usuarioNome = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Usuário do Gabinete";

        _context.Set<EventoMovimentoEntity>().Add(new EventoMovimentoEntity
        {
            EventoId = id,
            Descricao = NovaMovimentacao.Trim(),
            UsuarioNome = usuarioNome,
            DataMovimento = DateTimeExtensions.ObterDataHoraBrasilia(),
            Ativo = true
        });

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Movimentação registrada com sucesso!";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRemoverMovimentoAsync(Guid id, Guid movimentoId)
    {
        if (User.IsInRole(nameof(PerfilAcesso.CaboEleitoral)))
        {
            TempData["ErrorMessage"] = "Você não possui permissão para remover movimentações.";
            return RedirectToPage(new { id });
        }

        var movimento = await _context.Set<EventoMovimentoEntity>()
            .FirstOrDefaultAsync(m => m.Id == movimentoId && m.EventoId == id);

        if (movimento != null)
        {
            movimento.Ativo = false;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Movimentação excluída com sucesso!";
        }

        return RedirectToPage(new { id });
    }

    private void LimparValidacaoCampos()
    {
        ModelState.ClearValidationState(nameof(NovaMovimentacao));
        ModelState.MarkFieldValid(nameof(NovaMovimentacao));

        ModelState.ClearValidationState(nameof(MotivoDesconfirmacao));
        ModelState.MarkFieldValid(nameof(MotivoDesconfirmacao));
    }

    private async Task<CaboEleitoralEntity?> ObterCaboLogadoAsync()
    {
        var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(emailClaim))
        {
            TempData["ErrorMessage"] = "Usuário não identificado.";
            return null;
        }

        var cabo = await _context.Set<CaboEleitoralEntity>()
            .FirstOrDefaultAsync(c => c.Email != null && c.Email.ToLower() == emailClaim.ToLower());

        if (cabo == null)
            TempData["ErrorMessage"] = "Cadastro de cabo eleitoral não localizado.";

        return cabo;
    }

    private static double CalcularDistanciaMetros(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000;
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        return R * (2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)));
    }

    private static double ToRadians(double val) => Math.PI / 180 * val;
}