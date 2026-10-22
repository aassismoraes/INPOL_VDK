using Application.Eventos.InputModels;
using CrossCutting.Helpers;
using Data.Connections;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using WebApp._0___Utils;

namespace WebApp.Pages.Eventos;

[Authorize(Policy = "EquipeGabinete")]
public class CadastrarModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CadastrarModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public CadastroEventoInputModel EventoInput { get; set; } = new();

    // Propriedades para receber as coordenadas geográficas capturadas pelo JavaScript no formulário
    [BindProperty]
    public double? Latitude { get; set; }

    [BindProperty]
    public double? Longitude { get; set; }

    public List<CaboEleitoralOptionDto> CabosEleitoraisDisponiveis { get; set; } = new();

    public class CaboEleitoralOptionDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? AreaInfluencia { get; set; }
        public string? Bairro { get; set; }
    }

    public async Task OnGetAsync()
    {
        await CarregarCabosEleitoraisAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await CarregarCabosEleitoraisAsync();
            return Page();
        }

        var politico = await Util.GetPolitico(_context);

        if (politico == null)
        {
            ModelState.AddModelError(string.Empty, "Nenhum político cadastrado no sistema para vincular ao evento.");
            await CarregarCabosEleitoraisAsync();
            return Page();
        }

        var novoEvento = new EventoEntity
        {
            PoliticoId = politico.Id,
            Titulo = EventoInput.Titulo,
            Descricao = EventoInput.Descricao,
            Tipo = EventoInput.Tipo,
            Local = EventoInput.Local,
            DataHoraInicio = EventoInput.DataHoraInicio.ParaHorarioBrasilia(),
            DataHoraFim = EventoInput.DataHoraFim.Value.ParaHorarioBrasilia(),

            MinutosAntecedenciaCheckIn = EventoInput.MinutosAntecedenciaCheckIn > 0
                ? EventoInput.MinutosAntecedenciaCheckIn
                : 20,

            Cep = EventoInput.Cep,
            Logradouro = EventoInput.Logradouro,
            Numero = EventoInput.Numero,
            Complemento = EventoInput.Complemento,
            Bairro = EventoInput.Bairro,
            Cidade = EventoInput.Cidade,
            Uf = EventoInput.Uf,

            CabosEleitoraisConvidados = new List<EventoCaboEleitoralEntity>()
        };

        // Regista o ponto geográfico com SRID 4326 (Padrão Global GPS - WGS 84)
        if (Latitude.HasValue && Longitude.HasValue)
        {
            novoEvento.Coordenadas = new Point(Longitude.Value, Latitude.Value) { SRID = 4326 };
        }

        if (EventoInput.CabosEleitoraisIds != null && EventoInput.CabosEleitoraisIds.Any())
        {
            foreach (var caboId in EventoInput.CabosEleitoraisIds)
            {
                novoEvento.CabosEleitoraisConvidados.Add(new EventoCaboEleitoralEntity
                {
                    CaboEleitoralId = caboId
                });
            }
        }

        _context.Eventos.Add(novoEvento);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Evento agendado com sucesso e GPS registado!";
        return RedirectToPage("/Eventos/Index");
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
    }
}