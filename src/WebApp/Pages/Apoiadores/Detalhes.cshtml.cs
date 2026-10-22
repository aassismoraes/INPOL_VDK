using Data.Connections;
using Data.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApp._0___Utils;

namespace WebApp.Pages.Apoiadores
{
    public class DetalhesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private const int TAMANHO_PAGINA = 3;

        public DetalhesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public PessoaEntity Apoiador { get; set; } = null!;
        public List<InteracaoPessoaEntity> Interacoes { get; set; } = new();

        public int PaginaAtual { get; set; } = 1;
        public int TotalPaginas { get; set; }

        [BindProperty]
        public InteracaoInputModel InteracaoEdicao { get; set; } = new();

        [BindProperty]
        public InteracaoCriacaoInputModel InteracaoCriacao { get; set; } = new();

        public class InteracaoInputModel
        {
            public Guid Id { get; set; }
            public Guid PessoaId { get; set; }
            public TipoInteracao TipoInteracao { get; set; } = TipoInteracao.AVULSO;
            public TipoMensagem TipoMensagem { get; set; } = TipoMensagem.MANUAL;
            public StatusInteracao StatusInteracao { get; set; } = StatusInteracao.ENVIADO;
            public SituacaoInteracao Situacao { get; set; } = SituacaoInteracao.SEM_RESPOSTA;
            public string? Mensagem { get; set; }
        }

        public class InteracaoCriacaoInputModel
        {
            public Guid PessoaId { get; set; }
            public TipoInteracao TipoInteracao { get; set; } = TipoInteracao.AVULSO;
            public TipoMensagem TipoMensagem { get; set; } = TipoMensagem.MANUAL;
            public StatusInteracao StatusInteracao { get; set; } = StatusInteracao.ENVIADO;
            public SituacaoInteracao Situacao { get; set; } = SituacaoInteracao.SEM_RESPOSTA;
            public string? Mensagem { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(Guid id, int pagina = 1)
        {
            PaginaAtual = pagina < 1 ? 1 : pagina;

            var apoiador = await _context.Pessoas
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (apoiador == null)
            {
                return NotFound();
            }

            // NOVA VALIDAÇÃO DE ACESSO DIRETO VIA URL
            if (User.IsInRole(nameof(PerfilAcesso.CaboEleitoral)))
            {
                var caboLogado = await Util.GetCaboEleitoralLogado(_context, User);
                if (caboLogado == null || apoiador.PadrinhoId != caboLogado.Id)
                {
                    return Forbid();
                }
            }

            if (apoiador.PadrinhoId != Guid.Empty)
            {
                apoiador.Padrinho = await _context.CabosEleitorais
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == apoiador.PadrinhoId);
            }

            Apoiador = apoiador;
            await CarregarInteracoesAsync(id, PaginaAtual);

            return Page();
        }

        // Handler assíncrono chamado via AJAX para trocar de página
        public async Task<IActionResult> OnGetInteracoesPartialAsync(Guid id, int pagina = 1)
        {
            PaginaAtual = pagina < 1 ? 1 : pagina;
            await CarregarInteracoesAsync(id, PaginaAtual);

            return Partial("_InteracoesTablePartial", this);
        }

        private async Task CarregarInteracoesAsync(Guid pessoaId, int pagina)
        {
            var query = _context.InteracoesPessoa
                .AsNoTracking()
                .Where(i => i.PessoaId == pessoaId)
                .OrderByDescending(i => i.DataHora);

            var totalItens = await query.CountAsync();
            TotalPaginas = (int)Math.Ceiling(totalItens / (double)TAMANHO_PAGINA);

            Interacoes = await query
                .Skip((pagina - 1) * TAMANHO_PAGINA)
                .Take(TAMANHO_PAGINA)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAdicionarInteracaoAsync()
        {
            var usuarioLogado = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Usuário do Sistema";
            var agora = DateTime.Now;

            var novaInteracao = new InteracaoPessoaEntity
            {
                Id = Guid.NewGuid(),
                PessoaId = InteracaoCriacao.PessoaId,
                TipoInteracao = InteracaoCriacao.TipoInteracao.ToString(),
                TipoMensagem = InteracaoCriacao.TipoMensagem.ToString(),
                StatusInteracao = InteracaoCriacao.StatusInteracao.ToString(),
                Situacao = InteracaoCriacao.Situacao.ToString(),
                Mensagem = InteracaoCriacao.Mensagem,
                DataHora = agora,
                Remetente = usuarioLogado,
                AlteradoEm = agora
            };

            await _context.InteracoesPessoa.AddAsync(novaInteracao);

            await _context.InteracoesPessoaLogs.AddAsync(new InteracaoPessoaLogEntity
            {
                InteracaoId = novaInteracao.Id,
                Usuario = usuarioLogado,
                DataAlteracao = agora,
                CampoAlterado = "Criação",
                ValorAntigo = null,
                ValorNovo = $"Nova interação registrada via painel por {usuarioLogado}"
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Nova interação adicionada com sucesso!";
            return RedirectToPage(new { id = InteracaoCriacao.PessoaId, pagina = 1 });
        }

        public async Task<IActionResult> OnPostSalvarInteracaoAsync(int pagina = 1)
        {
            var interacao = await _context.InteracoesPessoa
                .FirstOrDefaultAsync(i => i.Id == InteracaoEdicao.Id);

            if (interacao == null)
            {
                return NotFound();
            }

            var usuarioLogado = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Usuário do Sistema";
            var agora = DateTime.Now;
            var logs = new List<InteracaoPessoaLogEntity>();

            void RegistrarLogSeAlterado(string nomeCampo, string? antigo, string? novo)
            {
                if ((antigo ?? string.Empty) != (novo ?? string.Empty))
                {
                    logs.Add(new InteracaoPessoaLogEntity
                    {
                        InteracaoId = interacao.Id,
                        Usuario = usuarioLogado,
                        DataAlteracao = agora,
                        CampoAlterado = nomeCampo,
                        ValorAntigo = antigo,
                        ValorNovo = novo
                    });
                }
            }

            var novoTipoInteracaoStr = InteracaoEdicao.TipoInteracao.ToString();
            var novoTipoMensagemStr = InteracaoEdicao.TipoMensagem.ToString();
            var novoStatusInteracaoStr = InteracaoEdicao.StatusInteracao.ToString();
            var novaSituacaoStr = InteracaoEdicao.Situacao.ToString();

            RegistrarLogSeAlterado("TipoInteracao", interacao.TipoInteracao, novoTipoInteracaoStr);
            RegistrarLogSeAlterado("TipoMensagem", interacao.TipoMensagem, novoTipoMensagemStr);
            RegistrarLogSeAlterado("StatusInteracao", interacao.StatusInteracao, novoStatusInteracaoStr);
            RegistrarLogSeAlterado("Situacao", interacao.Situacao, novaSituacaoStr);
            RegistrarLogSeAlterado("Mensagem", interacao.Mensagem, InteracaoEdicao.Mensagem);

            if (logs.Any())
            {
                interacao.TipoInteracao = novoTipoInteracaoStr;
                interacao.TipoMensagem = novoTipoMensagemStr;
                interacao.StatusInteracao = novoStatusInteracaoStr;
                interacao.Situacao = novaSituacaoStr;
                interacao.Mensagem = InteracaoEdicao.Mensagem;
                interacao.AlteradoEm = agora;

                _context.InteracoesPessoa.Update(interacao);
                await _context.InteracoesPessoaLogs.AddRangeAsync(logs);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Interação atualizada e alterações registradas em log com sucesso!";
            }

            return RedirectToPage(new { id = InteracaoEdicao.PessoaId, pagina });
        }

        public async Task<IActionResult> OnPostAprovarAsync(Guid id)
        {
            // Bloqueia a ação se for Cabo Eleitoral
            if (User.IsInRole(nameof(PerfilAcesso.CaboEleitoral)))
            {
                return Forbid();
            }

            var apoiador = await _context.Pessoas.FindAsync(id);

            if (apoiador == null)
            {
                return NotFound();
            }

            if (apoiador.StatusTexto != "AGUARDANDO APROVAÇÃO")
            {
                TempData["ErrorMessage"] = "Este apoiador já foi aprovado ou inativado.";
                return RedirectToPage(new { id });
            }

            var usuarioLogado = User.Identity?.Name ?? "Administrador/Assessor";

            // Atualiza o status para ATIVO e regista a aprovação
            apoiador.StatusTexto = "ATIVO";
            apoiador.DataStatus = DateTime.Now;
            apoiador.DescricaoStatus = $"Cadastro aprovado e ativado por {usuarioLogado}.";
            apoiador.AtualizadoEm = DateTime.Now;

            _context.Pessoas.Update(apoiador);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cadastro aprovado com sucesso! O apoiador agora está ATIVO na base.";
            return RedirectToPage(new { id });
        }
    }
}