using CrossCutting.Helpers;
using Data.Connections;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json.Serialization;

namespace WebApp.Pages.Configuracoes.CabosEleitorais;

[Authorize(Policy = "EquipeEstrategica")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    public List<CaboEleitoralDto> CabosEleitorais { get; set; } = new();

    // Propriedades para Pesquisa, Filtro e Paginação
    [BindProperty(SupportsGet = true)]
    public string? TermoBusca { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFiltro { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PaginaAtual { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int TamanhoPagina { get; set; } = 10;

    public int TotalPaginas { get; set; }
    public int TotalRegistros { get; set; }

    [BindProperty]
    public CaboEleitoralInputModel Input { get; set; } = new();

    public class CaboEleitoralDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? Profissao { get; set; }
        public string? LocalTrabalho { get; set; }
        public string? AreaInfluencia { get; set; }
        public string? Cep { get; set; }
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Complemento { get; set; }
        public string? Bairro { get; set; }
        public string? Cidade { get; set; }
        public string? Uf { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool Ativo { get; set; }
    }

    public class CaboEleitoralInputModel
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        public string? Email { get; set; }

        public string? Telefone { get; set; }

        [StringLength(100, ErrorMessage = "A profissão deve ter no máximo 100 caracteres.")]
        public string? Profissao { get; set; }

        [StringLength(150, ErrorMessage = "O local de trabalho deve ter no máximo 150 caracteres.")]
        public string? LocalTrabalho { get; set; }

        [StringLength(150, ErrorMessage = "A área de influência deve ter no máximo 150 caracteres.")]
        public string? AreaInfluencia { get; set; }

        public string? Cep { get; set; }
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Complemento { get; set; }
        public string? Bairro { get; set; }
        public string? Cidade { get; set; } = "Magé";
        public string? Uf { get; set; } = "RJ";
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public async Task OnGetAsync()
    {
        await CarregarCabosEleitoraisAsync();
    }

    public async Task<IActionResult> OnPostSalvarAsync()
    {
        if (!ModelState.IsValid)
        {
            await CarregarCabosEleitoraisAsync();
            return Page();
        }

        if (Input.Id.HasValue && Input.Id.Value != Guid.Empty)
        {
            var item = await _context.Set<CaboEleitoralEntity>().FindAsync(Input.Id.Value);
            if (item != null)
            {
                MapearInputParaEntidade(Input, item);
                item.AtualizadoEm = DateTimeExtensions.ObterDataHoraBrasilia();

                _context.Set<CaboEleitoralEntity>().Update(item);
                TempData["SuccessMessage"] = "Cabo eleitoral atualizado com sucesso!";
            }
        }
        else
        {
            var novo = new CaboEleitoralEntity
            {
                Id = Guid.NewGuid(),
                Ativo = true,
                CriadoEm = DateTimeExtensions.ObterDataHoraBrasilia()
            };
            MapearInputParaEntidade(Input, novo);

            await _context.Set<CaboEleitoralEntity>().AddAsync(novo);
            TempData["SuccessMessage"] = "Cabo eleitoral cadastrado com sucesso!";
        }

        await _context.SaveChangesAsync();
        return RedirectToPage(new { TermoBusca, StatusFiltro, PaginaAtual, TamanhoPagina });
    }

    public async Task<IActionResult> OnPostAlternarStatusAsync(Guid id)
    {
        var item = await _context.Set<CaboEleitoralEntity>().FindAsync(id);
        if (item != null)
        {
            item.Ativo = !item.Ativo;
            item.AtualizadoEm = DateTimeExtensions.ObterDataHoraBrasilia();

            await _context.SaveChangesAsync();
            var statusStr = item.Ativo ? "ativado" : "desativado";
            TempData["SuccessMessage"] = $"Cabo eleitoral {item.Nome} foi {statusStr} com sucesso!";
        }

        return RedirectToPage(new { TermoBusca, StatusFiltro, PaginaAtual, TamanhoPagina });
    }

    public async Task<IActionResult> OnPostRemoverAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Identificador inválido para remoção.";
            return RedirectToPage(new { TermoBusca, StatusFiltro, PaginaAtual, TamanhoPagina });
        }

        var item = await _context.Set<CaboEleitoralEntity>().FindAsync(id);
        if (item == null)
        {
            TempData["ErrorMessage"] = "Cabo eleitoral não foi encontrado.";
            return RedirectToPage(new { TermoBusca, StatusFiltro, PaginaAtual, TamanhoPagina });
        }

        try
        {
            _context.Set<CaboEleitoralEntity>().Remove(item);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Cabo eleitoral {item.Nome} foi removido com sucesso!";
        }
        catch (DbUpdateException)
        {
            // Trata erro de integridade referencial (quando existem vínculos no banco)
            TempData["ErrorMessage"] = $"Não é possível remover {item.Nome} pois existem registros vinculados a ele. Considere desativá-lo em vez de excluir.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Ocorreu um erro ao tentar remover: {ex.Message}";
        }

        return RedirectToPage(new { TermoBusca, StatusFiltro, PaginaAtual, TamanhoPagina });
    }

    public async Task<JsonResult> OnGetBuscarCoordenadasAsync(string? cep, string? logradouro, string? cidade, string? uf)
    {
        if (!string.IsNullOrWhiteSpace(cep))
        {
            var cepLimpo = System.Text.RegularExpressions.Regex.Replace(cep, @"[^\d]", "");
            if (cepLimpo.Length == 8)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient();
                    var res = await client.GetFromJsonAsync<BrasilApiCepResult>($"https://brasilapi.com.br/api/cep/v2/{cepLimpo}");

                    if (res != null)
                    {
                        double? lat = null, lng = null;
                        if (res.Location?.Coordinates != null &&
                            double.TryParse(res.Location.Coordinates.Latitude, CultureInfo.InvariantCulture, out var parsedLat) &&
                            double.TryParse(res.Location.Coordinates.Longitude, CultureInfo.InvariantCulture, out var parsedLng))
                        {
                            lat = parsedLat;
                            lng = parsedLng;
                        }

                        return new JsonResult(new
                        {
                            success = true,
                            cep = res.Cep,
                            logradouro = res.Street,
                            bairro = res.Neighborhood,
                            cidade = res.City,
                            uf = res.State,
                            lat,
                            lng,
                            fonte = "CEP"
                        });
                    }
                }
                catch { }
            }
        }

        return new JsonResult(new { success = false, message = "Endereço não localizado." });
    }

    private void MapearInputParaEntidade(CaboEleitoralInputModel input, CaboEleitoralEntity entity)
    {
        entity.Nome = input.Nome.Trim();
        entity.Email = string.IsNullOrWhiteSpace(input.Email) ? null : input.Email.Trim().ToLower();
        entity.Telefone = input.Telefone;
        entity.Profissao = input.Profissao?.Trim();
        entity.LocalTrabalho = input.LocalTrabalho?.Trim();
        entity.AreaInfluencia = input.AreaInfluencia?.Trim();
        entity.Cep = input.Cep;
        entity.Logradouro = input.Logradouro?.Trim();
        entity.Numero = input.Numero?.Trim();
        entity.Complemento = input.Complemento?.Trim();
        entity.Bairro = input.Bairro?.Trim();
        entity.Cidade = input.Cidade?.Trim();
        entity.Uf = input.Uf?.Trim();
        entity.Latitude = input.Latitude;
        entity.Longitude = input.Longitude;
    }

    private async Task CarregarCabosEleitoraisAsync()
    {
        // Adiciona o filtro para ignorar registros com Soft Delete
        var query = _context.Set<CaboEleitoralEntity>()
            .AsNoTracking()
            .Where(c => c.DeletadoEm == null);

        // Filtro de Busca Por Texto
        if (!string.IsNullOrWhiteSpace(TermoBusca))
        {
            var termo = TermoBusca.Trim().ToLower();
            query = query.Where(c => EF.Functions.Like(c.Nome.ToLower(), $"%{termo}%") ||
                                     (c.Email != null && EF.Functions.Like(c.Email.ToLower(), $"%{termo}%")) ||
                                     (c.AreaInfluencia != null && EF.Functions.Like(c.AreaInfluencia.ToLower(), $"%{termo}%")) ||
                                     (c.Bairro != null && EF.Functions.Like(c.Bairro.ToLower(), $"%{termo}%")));
        }

        // Filtro por Status
        if (!string.IsNullOrWhiteSpace(StatusFiltro))
        {
            if (StatusFiltro.Equals("ativos", StringComparison.OrdinalIgnoreCase))
                query = query.Where(c => c.Ativo);
            else if (StatusFiltro.Equals("inativos", StringComparison.OrdinalIgnoreCase))
                query = query.Where(c => !c.Ativo);
        }

        TotalRegistros = await query.CountAsync();
        TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)TamanhoPagina);
        if (TotalPaginas < 1) TotalPaginas = 1;

        if (PaginaAtual < 1) PaginaAtual = 1;
        if (PaginaAtual > TotalPaginas) PaginaAtual = TotalPaginas;

        CabosEleitorais = await query
            .OrderByDescending(c => c.Ativo)
            .ThenBy(c => c.Nome)
            .Skip((PaginaAtual - 1) * TamanhoPagina)
            .Take(TamanhoPagina)
            .Select(c => new CaboEleitoralDto
            {
                Id = c.Id,
                Nome = c.Nome,
                Email = c.Email,
                Telefone = c.Telefone,
                Profissao = c.Profissao,
                LocalTrabalho = c.LocalTrabalho,
                AreaInfluencia = c.AreaInfluencia,
                Cep = c.Cep,
                Logradouro = c.Logradouro,
                Numero = c.Numero,
                Complemento = c.Complemento,
                Bairro = c.Bairro,
                Cidade = c.Cidade,
                Uf = c.Uf,
                Latitude = c.Latitude,
                Longitude = c.Longitude,
                Ativo = c.Ativo
            })
            .ToListAsync();
    }

    private class BrasilApiCepResult
    {
        [JsonPropertyName("cep")] public string? Cep { get; set; }
        [JsonPropertyName("state")] public string? State { get; set; }
        [JsonPropertyName("city")] public string? City { get; set; }
        [JsonPropertyName("neighborhood")] public string? Neighborhood { get; set; }
        [JsonPropertyName("street")] public string? Street { get; set; }
        [JsonPropertyName("location")] public BrasilApiLocation? Location { get; set; }
    }

    private class BrasilApiLocation
    {
        [JsonPropertyName("coordinates")] public BrasilApiCoordinates? Coordinates { get; set; }
    }

    private class BrasilApiCoordinates
    {
        [JsonPropertyName("longitude")] public string? Longitude { get; set; }
        [JsonPropertyName("latitude")] public string? Latitude { get; set; }
    }
}