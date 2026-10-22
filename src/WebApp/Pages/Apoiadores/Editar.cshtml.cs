using CrossCutting.Helpers;
using Data.Connections;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using WebApp._0___Utils;

namespace WebApp.Pages.Apoiadores
{
    public class EditarModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EditarModel> _logger;

        public EditarModel(ApplicationDbContext context, ILogger<EditarModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public Guid Id { get; set; }

        [BindProperty]
        public string StatusTexto { get; set; } = "ATIVO";

        [BindProperty]
        public string? DescricaoStatus { get; set; }

        [BindProperty]
        public ApoiadorInputModel Input { get; set; } = new();

        public SelectList CabosEleitorais { get; set; } = null!;
        public bool EhCaboEleitoral { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var apoiador = await _context.Pessoas.FirstOrDefaultAsync(a => a.Id == id);
            if (apoiador == null)
            {
                return NotFound();
            }

            // SEGURANÇA: Se for Cabo Eleitoral, verifica vínculo e status
            if (User.IsInRole(nameof(PerfilAcesso.CaboEleitoral)))
            {
                var caboLogado = await Util.GetCaboEleitoralLogado(_context, User);
                if (caboLogado == null || apoiador.PadrinhoId != caboLogado.Id)
                {
                    return Forbid();
                }

                // BLOQUEIO: Não permite abrir a tela de edição se já foi aprovado[cite: 12]
                if (apoiador.StatusTexto != "AGUARDANDO APROVAÇÃO")
                {
                    TempData["ErrorMessage"] = "Você só pode editar apoiadores que ainda estão aguardando aprovação.";
                    return RedirectToPage("./Detalhes", new { id = apoiador.Id });
                }
            }

            Id = apoiador.Id;
            StatusTexto = apoiador.StatusTexto;
            DescricaoStatus = apoiador.DescricaoStatus;

            Input = new ApoiadorInputModel
            {
                Nome = apoiador.Nome,
                Cpf = apoiador.Cpf,
                Apelido = apoiador.Apelido,
                Telefone = apoiador.Celular,
                TelefoneFixo = apoiador.Telefone,
                Email = apoiador.Email,
                DataNascimento = apoiador.DataNascimento,
                Genero = apoiador.Genero,
                Profissao = apoiador.Profissao,
                Cep = apoiador.Cep,
                Logradouro = apoiador.Logradouro,
                Numero = apoiador.Numero,
                Bairro = apoiador.Bairro,
                Cidade = apoiador.Cidade,
                Uf = apoiador.Uf,
                PadrinhoId = apoiador.PadrinhoId,
                Observacoes = apoiador.Observacoes
            };

            await CarregarListasAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            Id = id;
            EhCaboEleitoral = User.IsInRole(nameof(PerfilAcesso.CaboEleitoral));

            var apoiadorExistente = await _context.Pessoas.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if (apoiadorExistente == null)
            {
                return NotFound();
            }

            if (EhCaboEleitoral)
            {
                var caboLogado = await Util.GetCaboEleitoralLogado(_context, User);
                if (caboLogado == null || apoiadorExistente.PadrinhoId != caboLogado.Id)
                {
                    return Forbid();
                }

                // BLOQUEIO BACKEND: Impede que a edição seja salva caso o status já tenha mudado[cite: 12]
                if (apoiadorExistente.StatusTexto != "AGUARDANDO APROVAÇÃO")
                {
                    TempData["ErrorMessage"] = "Apoiadores já aprovados ou inativos não podem ser editados.";
                    return RedirectToPage("./Detalhes", new { id = apoiadorExistente.Id });
                }

                // Mantém obrigatoriamente vinculado a ele mesmo
                Input.PadrinhoId = caboLogado.Id;

                // FORÇA O STATUS: Garante que o cabo não mude o status para "ATIVO" via formulário html[cite: 12]
                StatusTexto = "AGUARDANDO APROVAÇÃO";
            }

            bool statusMudou = apoiadorExistente.StatusTexto != StatusTexto;
            bool isInativo = StatusTexto == "INATIVO";

            if ((statusMudou || isInativo) && string.IsNullOrWhiteSpace(DescricaoStatus))
            {
                ModelState.AddModelError("DescricaoStatus", "Informe a justificativa/motivo para a alteração de status.");
            }

            var telefoneNumeros = ApenasNumeros(Input.Telefone);

            if (!string.IsNullOrWhiteSpace(telefoneNumeros))
            {
                var pessoa = await _context.Pessoas.Where(x => x.Celular == telefoneNumeros).FirstOrDefaultAsync();
                if (pessoa != null && pessoa.Id != id)
                {
                    ModelState.AddModelError("Input.Telefone", "Este número de telefone já pertence a outro apoiador.");
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
                var apoiador = await _context.Pessoas.FirstOrDefaultAsync(a => a.Id == id);
                if (apoiador == null)
                {
                    return NotFound();
                }

                apoiador.Nome = Input.Nome;
                apoiador.Cpf = Input.Cpf;
                apoiador.Apelido = Input.Apelido;
                apoiador.Celular = Input.Telefone;
                apoiador.Telefone = Input.TelefoneFixo;
                apoiador.Email = Input.Email;
                apoiador.DataNascimento = Input.DataNascimento;
                apoiador.Genero = Input.Genero ?? string.Empty;
                apoiador.Profissao = Input.Profissao;
                apoiador.Cep = Input.Cep;
                apoiador.Logradouro = Input.Logradouro;
                apoiador.Numero = Input.Numero;
                apoiador.Bairro = Input.Bairro;
                apoiador.Cidade = Input.Cidade;
                apoiador.Uf = Input.Uf;
                apoiador.PadrinhoId = Input.PadrinhoId ?? Guid.Empty;
                apoiador.Observacoes = Input.Observacoes;

                if (statusMudou)
                {
                    apoiador.DataStatus = DateTimeExtensions.ObterDataHoraBrasilia();
                }
                apoiador.StatusTexto = StatusTexto;
                apoiador.DescricaoStatus = DescricaoStatus;
                apoiador.AtualizadoEm = DateTimeExtensions.ObterDataHoraBrasilia();

                _context.Pessoas.Update(apoiador);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Apoiador atualizado com sucesso!";
                return RedirectToPage("./Detalhes", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar apoiador {Id}", id);
                TempData["SystemErrorMessage"] = "Não foi possível atualizar as informações do apoiador neste momento.";

                await CarregarListasAsync();
                return Page();
            }
        }

        private static string ApenasNumeros(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
            return Regex.Replace(texto, @"[^\d]", "");
        }

        private async Task CarregarListasAsync()
        {
            EhCaboEleitoral = User.IsInRole(nameof(PerfilAcesso.CaboEleitoral));
            var query = _context.CabosEleitorais.AsNoTracking().Where(c => c.Ativo);

            if (EhCaboEleitoral)
            {
                var caboLogado = await Util.GetCaboEleitoralLogado(_context, User);
                if (caboLogado != null)
                {
                    query = query.Where(c => c.Id == caboLogado.Id);
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