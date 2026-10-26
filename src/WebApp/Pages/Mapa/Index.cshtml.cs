using Data.Connections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

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
                try
                {
                    var apoiadores = await _context.Pessoas
                        .AsNoTracking()
                        .Where(a => a.StatusTexto != null && a.StatusTexto.ToUpper() == "ATIVO" && !string.IsNullOrEmpty(a.Bairro))
                        .GroupBy(a => a.Bairro)
                        .Select(g => new
                        {
                            bairro = g.Key,
                            total = g.Count()
                        })
                        .ToListAsync();

                    return new JsonResult(apoiadores);
                }
                catch (Exception ex)
                {
                    return new JsonResult(new { erro = ex.Message });
                }

            case "demandas":
            default:
                var demandas = await _context.Demandas
                    .AsNoTracking()
                    .Where(d => !string.IsNullOrEmpty(d.Bairro))
                    .GroupBy(d => d.Bairro)
                    .Select(g => new
                    {
                        bairro = g.Key,
                        total = g.Count()
                    })
                    .ToListAsync();

                return new JsonResult(demandas);
        }
    }
}