using CrossCutting.Helpers;
using Data.Connections;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp._0___Utils;

namespace WebApp.Pages.Apoiadores
{
    public class CadastrarModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CadastrarModel> _logger;

        public CadastrarModel(ApplicationDbContext context, ILogger<CadastrarModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public ApoiadorInputModel Input { get; set; } = new();

        public SelectList CabosEleitorais { get; set; } = null!;
        public bool EhCaboEleitoral { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await CarregarListasAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            EhCaboEleitoral = User.IsInRole(nameof(PerfilAcesso.CaboEleitoral));

            if (EhCaboEleitoral)
            {
                var caboLogado = await Util.GetCaboEleitoralLogado(_context, User);
                if (caboLogado != null)
                {
                    Input.PadrinhoId = caboLogado.Id;
                }
            }

            if (!string.IsNullOrWhiteSpace(Input.Telefone))
            {
                var telefoneExiste = await _context.Pessoas
                    .AnyAsync(p => p.Celular == Input.Telefone || p.Telefone == Input.Telefone);

                if (telefoneExiste)
                {
                    ModelState.AddModelError("Input.Telefone", "Este número de telefone já está cadastrado.");
                }
            }

            if (!ModelState.IsValid)
            {
                var erros = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .ToList();

                if (erros.Count > 0)
                {
                    var listaHtml = string.Join("", erros.Select(e => $"<li>{e}</li>"));
                    TempData["ErrorMessage"] = $"<ul class=\"text-start mb-0 ps-3\">{listaHtml}</ul>";
                }
                else
                {
                    TempData["ErrorMessage"] = "Preencha todos os campos obrigatórios corretamente.";
                }

                await CarregarListasAsync();
                return Page();
            }

            try
            {
                var politico = await _context.Politicos.FirstOrDefaultAsync();

                var novoApoiador = new PessoaEntity
                {
                    Id = Guid.NewGuid(),
                    Nome = Input.Nome,
                    Cpf = Input.Cpf,
                    Apelido = Input.Apelido,
                    Celular = Input.Telefone,
                    Telefone = Input.TelefoneFixo,
                    Email = Input.Email,
                    DataNascimento = Input.DataNascimento,
                    Genero = Input.Genero,
                    Profissao = Input.Profissao,
                    Cep = Input.Cep,
                    Logradouro = Input.Logradouro,
                    Numero = Input.Numero,
                    Bairro = Input.Bairro,
                    Cidade = Input.Cidade,
                    Uf = Input.Uf,
                    PadrinhoId = Input.PadrinhoId ?? Guid.Empty,
                    Observacoes = Input.Observacoes,
                    CriadoEm = DateTimeExtensions.ObterDataHoraBrasilia(),
                    StatusTexto = EhCaboEleitoral ? "AGUARDANDO APROVAÇÃO" : "ATIVO",
                    PoliticoId = politico?.Id ?? Guid.Empty
                };

                _context.Pessoas.Add(novoApoiador);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Apoiador cadastrado com sucesso!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao tentar cadastrar apoiador {Nome}", Input.Nome);
                TempData["SystemErrorMessage"] = "Não foi possível concluir o cadastro no momento. Tente novamente em instantes.";

                await CarregarListasAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnGetVerificarTelefoneAsync(string telefone)
        {
            if (string.IsNullOrWhiteSpace(telefone))
            {
                return new JsonResult(new { existe = false });
            }

            var existe = await _context.Pessoas
                .AnyAsync(p => p.Celular == telefone || p.Telefone == telefone);

            return new JsonResult(new { existe });
        }

        private async Task CarregarListasAsync()
        {
            EhCaboEleitoral = User.IsInRole(nameof(PerfilAcesso.CaboEleitoral));
            var query = _context.CabosEleitorais.AsNoTracking()
                .Where(c => c.Ativo);

            if (EhCaboEleitoral)
            {
                var caboLogado = await Util.GetCaboEleitoralLogado(_context, User);
                if (caboLogado != null)
                {
                    query = query.Where(c => c.Id == caboLogado.Id);
                    Input.PadrinhoId = caboLogado.Id;
                }
            }

            var padrinhos = await query
                .Select(c => new { c.Id, c.Nome })
                .OrderBy(c => c.Nome)
                .ToListAsync();

            CabosEleitorais = new SelectList(padrinhos, "Id", "Nome");
        }
    }
}