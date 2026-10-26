using Data.Connections;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using WebApp._0___Utils;

namespace WebApp.Pages.Apoiadores
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public SelectList CabosEleitorais { get; set; } = null!;

        [BindProperty(SupportsGet = true)]
        public string? TermoBusca { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid? PadrinhoIdFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PaginaAtual { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int TamanhoPagina { get; set; } = 10;

        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }

        // Propriedades para o alerta de pendências
        public int PendentesAprovacaoCount { get; set; }
        public bool PodeAprovar { get; set; }

        public IList<ApoiadorItemDto> Apoiadores { get; set; } = new List<ApoiadorItemDto>();

        public async Task OnGetAsync()
        {
            await CarregarDadosApoiadoresAsync();
        }

        public async Task<IActionResult> OnGetApoiadoresPartialAsync()
        {
            await CarregarDadosApoiadoresAsync();

            return new PartialViewResult
            {
                ViewName = "_ApoiadoresTabelaPartial",
                ViewData = new ViewDataDictionary<IndexModel>(ViewData, this)
            };
        }

        private async Task CarregarDadosApoiadoresAsync()
        {
            if (TamanhoPagina != 5 && TamanhoPagina != 10 && TamanhoPagina != 20 && TamanhoPagina != 50)
            {
                TamanhoPagina = 10;
            }

            // Define se o usuário logado tem permissão para aprovar (Administrador ou Assessor)
            PodeAprovar = User.IsInRole(nameof(PerfilAcesso.Administrador)) || User.IsInRole(nameof(PerfilAcesso.Assessor));

            if (PodeAprovar)
            {
                PendentesAprovacaoCount = await _context.Pessoas
                    .Where(p => p.StatusTexto == "AGUARDANDO APROVAÇÃO")
                    .CountAsync();
            }

            var query = _context.Pessoas.AsNoTracking();

            // ==========================================================
            // REGRA DE SEGURANÇA MÁXIMA: RESTRIÇÃO PARA CABO ELEITORAL
            // ==========================================================
            if (User.IsInRole(nameof(PerfilAcesso.CaboEleitoral)))
            {
                var caboLogado = await Util.GetCaboEleitoralLogado(_context, User);
                if (caboLogado != null)
                {
                    // Força a query a buscar SOMENTE os apoiadores deste cabo, ignorando qualquer manipulação de URL
                    query = query.Where(a => a.PadrinhoId == caboLogado.Id);
                    PadrinhoIdFiltro = caboLogado.Id; // Trava o select na interface
                }
                else
                {
                    // Se houver algum erro e não achar o cabo logado, trava a query para retornar ZERO resultados
                    query = query.Where(a => false);
                    PadrinhoIdFiltro = Guid.Empty;
                }
            }
            else
            {
                // Se NÃO for Cabo Eleitoral (é Admin/Assessor), aplica o filtro do dropdown, caso selecionado
                if (PadrinhoIdFiltro.HasValue && PadrinhoIdFiltro.Value != Guid.Empty)
                {
                    query = query.Where(a => a.PadrinhoId == PadrinhoIdFiltro.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(TermoBusca))
            {
                var pattern = $"%{TermoBusca.Trim()}%";
                query = query.Where(a =>
                    (a.Nome != null && EF.Functions.Like(a.Nome, pattern)) ||
                    (a.Apelido != null && EF.Functions.Like(a.Apelido, pattern)) ||
                    (a.Cpf != null && EF.Functions.Like(a.Cpf, pattern)) ||
                    (a.Celular != null && EF.Functions.Like(a.Celular, pattern)) ||
                    (a.Bairro != null && EF.Functions.Like(a.Bairro, pattern)) ||
                    (a.Cidade != null && EF.Functions.Like(a.Cidade, pattern))
                );
            }

            if (!string.IsNullOrWhiteSpace(StatusFiltro))
            {
                query = query.Where(a => a.StatusTexto == StatusFiltro);
            }

            TotalRegistros = await query.CountAsync();
            TotalPaginas = TotalRegistros > 0 ? (int)Math.Ceiling(TotalRegistros / (double)TamanhoPagina) : 1;

            if (PaginaAtual < 1) PaginaAtual = 1;
            if (PaginaAtual > TotalPaginas) PaginaAtual = TotalPaginas;

            Apoiadores = await (from a in query
                                join p in _context.CabosEleitorais on a.PadrinhoId equals p.Id into padrinhosGroup
                                from padrinho in padrinhosGroup.DefaultIfEmpty()
                                orderby a.AtualizadoEm descending, a.CriadoEm descending
                                select new ApoiadorItemDto
                                {
                                    Id = a.Id,
                                    Nome = a.Nome ?? "[Sem Nome]",
                                    Apelido = a.Apelido ?? string.Empty,
                                    Telefone = a.Celular ?? string.Empty,
                                    Email = a.Email ?? string.Empty,
                                    Cpf = a.Cpf ?? string.Empty,
                                    Bairro = a.Bairro ?? string.Empty,
                                    Cidade = a.Cidade ?? string.Empty,
                                    Uf = a.Uf ?? string.Empty,
                                    StatusTexto = a.StatusTexto ?? "ATIVO",
                                    CriadoEm = a.CriadoEm,
                                    PadrinhoNome = padrinho != null ? (padrinho.Nome ?? "-") : "-"
                                })
                                .Skip((PaginaAtual - 1) * TamanhoPagina)
                                .Take(TamanhoPagina)
                                .ToListAsync();

            // Carrega lista de padrinhos para o select
            var queryPadrinhos = _context.CabosEleitorais.AsNoTracking().Where(c => c.Ativo);

            if (User.IsInRole(nameof(PerfilAcesso.CaboEleitoral)))
            {
                var caboLogado = await Util.GetCaboEleitoralLogado(_context, User);
                if (caboLogado != null)
                {
                    // Cabo eleitoral só vê o próprio nome no dropdown
                    queryPadrinhos = queryPadrinhos.Where(c => c.Id == caboLogado.Id);
                }
            }

            var padrinhos = await queryPadrinhos
                .Select(c => new { c.Id, Nome = c.Nome ?? "[Sem Nome]" })
                .OrderBy(c => c.Nome)
                .ToListAsync();

            CabosEleitorais = new SelectList(padrinhos, "Id", "Nome");
        }

        public async Task<IActionResult> OnPostDeletarAsync(Guid id)
        {
            var apoiador = await _context.Pessoas.FindAsync(id);

            if (apoiador != null)
            {
                // Se for cabo eleitoral, verifica se o apoiador pertence a ele e se o status permite exclusão[cite: 17]
                if (User.IsInRole(nameof(PerfilAcesso.CaboEleitoral)))
                {
                    var caboLogado = await Util.GetCaboEleitoralLogado(_context, User);

                    // Impede se não for o dono[cite: 17]
                    if (caboLogado == null || apoiador.PadrinhoId != caboLogado.Id)
                    {
                        return Forbid();
                    }

                    // Impede se já foi aprovado[cite: 17]
                    if (apoiador.StatusTexto != "AGUARDANDO APROVAÇÃO")
                    {
                        TempData["ErrorMessage"] = "Você só pode excluir apoiadores que ainda estão aguardando aprovação.";
                        return RedirectToPage("./Index", new { TermoBusca, StatusFiltro, PadrinhoIdFiltro, PaginaAtual, TamanhoPagina });
                    }
                }

                apoiador.StatusTexto = "INATIVO";
                apoiador.AtualizadoEm = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Apoiador excluído/inativado com sucesso!";
            }

            return RedirectToPage("./Index", new { TermoBusca, StatusFiltro, PadrinhoIdFiltro, PaginaAtual, TamanhoPagina });
        }
    }
}