using Application.Eventos.InputModels;
using CrossCutting.Helpers;
using Data.Connections;
using Data.Entities;
using Domain.Eventos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using System.Security.Claims;

namespace WebApp.Pages.Eventos;

[Authorize(Policy = "EquipeGabinete")]
public class EditarModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditarModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public CadastroEventoInputModel EventoInput { get; set; } = new();

    [BindProperty]
    public Guid Id { get; set; }

    [BindProperty]
    public bool Ativo { get; set; } = true;

    public List<CaboEleitoralOptionDto> CabosEleitoraisDisponiveis { get; set; } = new();
    public List<CaboEleitoralOptionDto> CabosEleitoraisSelecionados { get; set; } = new();

    public class CaboEleitoralOptionDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? AreaInfluencia { get; set; }
        public string? Bairro { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var evento = await _context.Eventos
            .AsNoTracking()
            .Include(e => e.CabosEleitoraisConvidados)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evento == null)
        {
            return NotFound();
        }

        Id = evento.Id;
        Ativo = evento.Ativo;

        EventoInput = new CadastroEventoInputModel
        {
            Titulo = evento.Titulo,
            Tipo = evento.Tipo,
            Local = evento.Local,
            Descricao = evento.Descricao,
            DataHoraInicio = new DateTime(
                evento.DataHoraInicio.Year, evento.DataHoraInicio.Month, evento.DataHoraInicio.Day,
                evento.DataHoraInicio.Hour, evento.DataHoraInicio.Minute, 0),
            DataHoraFim = evento.DataHoraFim.HasValue && evento.DataHoraFim.Value != DateTime.MinValue
                ? new DateTime(
                    evento.DataHoraFim.Value.Year, evento.DataHoraFim.Value.Month, evento.DataHoraFim.Value.Day,
                    evento.DataHoraFim.Value.Hour, evento.DataHoraFim.Value.Minute, 0)
                : null,
            MinutosAntecedenciaCheckIn = evento.MinutosAntecedenciaCheckIn > 0 ? evento.MinutosAntecedenciaCheckIn : 20,
            Cep = evento.Cep,
            Logradouro = evento.Logradouro,
            Numero = evento.Numero,
            Bairro = evento.Bairro,
            Cidade = evento.Cidade,
            Uf = evento.Uf,
            Complemento = evento.Complemento,
            Latitude = evento.Coordenadas?.Y,
            Longitude = evento.Coordenadas?.X,
            CabosEleitoraisIds = evento.CabosEleitoraisConvidados.Select(c => c.CaboEleitoralId).ToList()
        };

        await CarregarCabosEleitoraisAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (EventoInput.DataHoraFim.HasValue && EventoInput.DataHoraFim.Value < EventoInput.DataHoraInicio)
        {
            ModelState.AddModelError("EventoInput.DataHoraFim", "A data/hora de término não pode ser anterior à de início.");
        }

        if (!ModelState.IsValid)
        {
            var erros = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .ToList();

            TempData["ErrorMessage"] = "Não foi possível salvar. Verifique os campos: " + string.Join(" | ", erros);

            await CarregarCabosEleitoraisAsync();
            return Page();
        }

        var evento = await _context.Eventos
            .FirstOrDefaultAsync(e => e.Id == Id);

        if (evento == null)
        {
            return NotFound();
        }

        if (evento.Ativo && !Ativo)
        {
            var usuarioNome = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Administrador";
            _context.Set<EventoMovimentoEntity>().Add(new EventoMovimentoEntity
            {
                EventoId = evento.Id,
                Descricao = "🚫 Evento cancelado/inativado através da edição do cadastro.",
                UsuarioNome = usuarioNome,
                DataMovimento = DateTimeExtensions.ObterDataHoraBrasilia(),
                Ativo = true
            });
        }

        evento.Ativo = Ativo;
        evento.Titulo = EventoInput.Titulo;
        evento.Tipo = EventoInput.Tipo;
        evento.Local = EventoInput.Local;
        evento.Descricao = EventoInput.Descricao;
        evento.DataHoraInicio = EventoInput.DataHoraInicio.ParaHorarioBrasilia();
        evento.DataHoraFim = EventoInput.DataHoraFim?.ParaHorarioBrasilia();
        evento.MinutosAntecedenciaCheckIn = EventoInput.MinutosAntecedenciaCheckIn > 0 ? EventoInput.MinutosAntecedenciaCheckIn : 20;

        evento.Cep = EventoInput.Cep;
        evento.Logradouro = EventoInput.Logradouro;
        evento.Numero = EventoInput.Numero;
        evento.Bairro = EventoInput.Bairro;
        evento.Cidade = EventoInput.Cidade;
        evento.Uf = EventoInput.Uf;
        evento.Complemento = EventoInput.Complemento;

        if (EventoInput.Latitude.HasValue && EventoInput.Longitude.HasValue)
        {
            evento.Coordenadas = new Point(EventoInput.Longitude.Value, EventoInput.Latitude.Value) { SRID = 4326 };
        }
        else
        {
            evento.Coordenadas = null;
        }

        await SincronizarCabosEleitoraisAsync(evento.Id, EventoInput.CabosEleitoraisIds ?? new List<Guid>());

        _context.Update(evento);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Evento atualizado com sucesso e GPS sincronizado!";
        return RedirectToPage("/Eventos/Index");
    }

    private async Task SincronizarCabosEleitoraisAsync(Guid eventoId, List<Guid> idsSelecionados)
    {
        var vinculosAtuais = await _context.Set<EventoCaboEleitoralEntity>()
            .Where(x => x.EventoId == eventoId)
            .ToListAsync();

        var idsAtuais = vinculosAtuais.Select(v => v.CaboEleitoralId).ToList();

        var vinculosParaRemover = vinculosAtuais
            .Where(v => !idsSelecionados.Contains(v.CaboEleitoralId))
            .ToList();

        if (vinculosParaRemover.Any())
        {
            _context.Set<EventoCaboEleitoralEntity>().RemoveRange(vinculosParaRemover);
        }

        var idsParaAdicionar = idsSelecionados
            .Except(idsAtuais)
            .ToList();

        if (idsParaAdicionar.Any())
        {
            var novosVinculos = idsParaAdicionar.Select(caboId => new EventoCaboEleitoralEntity
            {
                EventoId = eventoId,
                CaboEleitoralId = caboId
            });

            await _context.Set<EventoCaboEleitoralEntity>().AddRangeAsync(novosVinculos);
        }
    }

    private async Task CarregarCabosEleitoraisAsync()
    {
        CabosEleitoraisDisponiveis = await _context.Set<CaboEleitoralEntity>()
            .AsNoTracking()
            .Where(c => c.Ativo)
            .OrderBy(c => c.Nome)
            .Select(c => new CaboEleitoralOptionDto
            {
                Id = c.Id,
                Nome = c.Nome,
                AreaInfluencia = c.AreaInfluencia,
                Bairro = c.Bairro
            })
            .ToListAsync();

        if (EventoInput.CabosEleitoraisIds != null && EventoInput.CabosEleitoraisIds.Any())
        {
            CabosEleitoraisSelecionados = CabosEleitoraisDisponiveis
                .Where(c => EventoInput.CabosEleitoraisIds.Contains(c.Id))
                .ToList();
        }
    }
}