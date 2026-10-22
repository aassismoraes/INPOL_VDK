using Data.Connections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages.Mapa;

[Authorize(Policy = "EquipeGabinete")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public void OnGet()
    {
    }

    public async Task<JsonResult> OnGetDadosAsync(string tipo = "demandas", string ibge = "3302502")
    {
        // Limpa e padroniza string para evitar incompatibilidade
        string NormalizarTexto(string? input) =>
            string.IsNullOrWhiteSpace(input) ? string.Empty : input.Trim().ToUpper();

        switch (tipo.ToLower())
        {
            case "eventos":
                var eventos = await _context.Eventos
                    .AsNoTracking()
                    .Where(e => e.Ativo && !string.IsNullOrEmpty(e.Bairro))
                    .GroupBy(e => e.Bairro)
                    .Select(g => new
                    {
                        bairro = g.Key,
                        total = g.Count()
                    })
                    .ToListAsync();

                return new JsonResult(eventos);

            case "apoiadores":
                var apoiadores = await _context.Pessoas
                    .AsNoTracking()
                    .Where(a => a.Ativo == true && !string.IsNullOrEmpty(a.Bairro))
                    .GroupBy(a => a.Bairro)
                    .Select(g => new
                    {
                        bairro = g.Key,
                        total = g.Count()
                    })
                    .ToListAsync();

                return new JsonResult(apoiadores);

            case "demandas":
            default:
                var demandas = "";
                

                return new JsonResult(demandas);
        }
    }
}