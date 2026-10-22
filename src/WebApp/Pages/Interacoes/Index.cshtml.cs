using Data.Connections;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages.Interacoes
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private const int TAMANHO_PAGINA = 10;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<InteracaoDto> Interacoes { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? TermoBusca { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? TipoFiltro { get; set; }

        public int PaginaAtual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }

        public class InteracaoDto
        {
            public Guid Id { get; set; }
            public Guid PessoaId { get; set; }
            public string NomePessoa { get; set; } = string.Empty;
            public string? Mensagem { get; set; }
            public string? TipoMensagem { get; set; }
            public string? TipoInteracao { get; set; }
            public string? StatusInteracao { get; set; }
            public string? Situacao { get; set; }
            public DateTime? DataHora { get; set; }
            public string? Remetente { get; set; }
        }

        public async Task OnGetAsync(int pagina = 1)
        {
            PaginaAtual = pagina < 1 ? 1 : pagina;

            var query = _context.InteracoesPessoa
                .AsNoTracking()
                .Join(_context.Pessoas.AsNoTracking(),
                    interacao => interacao.PessoaId,
                    pessoa => pessoa.Id,
                    (interacao, pessoa) => new InteracaoDto
                    {
                        Id = interacao.Id,
                        PessoaId = interacao.PessoaId,
                        NomePessoa = pessoa.Nome,
                        Mensagem = interacao.Mensagem,
                        TipoMensagem = interacao.TipoMensagem,
                        TipoInteracao = interacao.TipoInteracao,
                        StatusInteracao = interacao.StatusInteracao,
                        Situacao = interacao.Situacao,
                        DataHora = interacao.DataHora,
                        Remetente = interacao.Remetente
                    });

            if (!string.IsNullOrWhiteSpace(TermoBusca))
            {
                query = query.Where(i => i.NomePessoa.Contains(TermoBusca) || (i.Mensagem != null && i.Mensagem.Contains(TermoBusca)));
            }

            if (!string.IsNullOrWhiteSpace(StatusFiltro))
            {
                query = query.Where(i => i.StatusInteracao == StatusFiltro);
            }

            if (!string.IsNullOrWhiteSpace(TipoFiltro))
            {
                query = query.Where(i => i.TipoInteracao == TipoFiltro);
            }

            TotalRegistros = await query.CountAsync();
            TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)TAMANHO_PAGINA);

            Interacoes = await query
                .OrderByDescending(i => i.DataHora)
                .Skip((PaginaAtual - 1) * TAMANHO_PAGINA)
                .Take(TAMANHO_PAGINA)
                .ToListAsync();
        }
    }
}