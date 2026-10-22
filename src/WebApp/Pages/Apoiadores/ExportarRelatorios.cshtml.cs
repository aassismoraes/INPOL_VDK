using Data.Connections;
using Data.Entities;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace WebApp.Pages.Apoiadores
{
    public class ExportarRelatoriosModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ExportarRelatoriosModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public FiltroRelatorioInputModel Filtro { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int PaginaAtual { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int TamanhoPagina { get; set; } = 20;

        public int TotalPaginas { get; set; }
        public int LimiteEfetivo { get; set; }

        public List<PessoaEntity> PreviewResultados { get; set; } = new();
        public int TotalRegistrosEncontrados { get; set; }
        public int TotalRegistrosConsiderados { get; set; }

        public class FiltroRelatorioInputModel
        {
            public string? Distrito { get; set; }
            public string? Genero { get; set; }
            public string? Tag { get; set; }
            public Guid? PadrinhoId { get; set; }
            public int? MaximoResultados { get; set; }

            public DateTime? DataNascimentoDe { get; set; }
            public DateTime? DataNascimentoAte { get; set; }
            public bool DataNascimentoNula { get; set; }

            public bool FiltrarContatosNascidosEm { get; set; }
            public int? NascidoDia { get; set; }
            public int? NascidoMes { get; set; }
            public int? NascidoAno { get; set; }

            public bool FiltrarContatosSemWhatsApp { get; set; }
            public int? SemWhatsAppDia { get; set; }
            public int? SemWhatsAppMes { get; set; }

            public bool ContatosSemInteracao { get; set; }
            public bool ContatosComInteracao { get; set; }
            public DateTime? InteracaoDataInicial { get; set; }
            public DateTime? InteracaoDataFinal { get; set; }
        }

        public SelectList DistritosList { get; set; } = null!;
        public SelectList GenerosList { get; set; } = null!;
        public SelectList TagsList { get; set; } = null!;
        public SelectList PadrinhosList { get; set; } = null!;

        public async Task OnGetAsync()
        {
            await CarregarListasAgrupadasAsync();
        }

        // Handler que processa a busca via AJAX e retorna a Partial View
        public async Task<IActionResult> OnGetPreviewPartialAsync()
        {
            var query = ConstruirQueryFiltros();
            TotalRegistrosEncontrados = await query.CountAsync();

            LimiteEfetivo = ObterLimiteEfetivo();
            TotalRegistrosConsiderados = Math.Min(TotalRegistrosEncontrados, LimiteEfetivo);

            if (PaginaAtual < 1) PaginaAtual = 1;
            if (TamanhoPagina <= 0) TamanhoPagina = 20;

            TotalPaginas = (int)Math.Ceiling(TotalRegistrosConsiderados / (double)TamanhoPagina);
            if (TotalPaginas < 1) TotalPaginas = 1;
            if (PaginaAtual > TotalPaginas) PaginaAtual = TotalPaginas;

            int skip = (PaginaAtual - 1) * TamanhoPagina;

            PreviewResultados = await query
                .Take(TotalRegistrosConsiderados)
                .Skip(skip)
                .Take(TamanhoPagina)
                .ToListAsync();

            return new PartialViewResult
            {
                ViewName = "_PreviewResultadosPartial",
                ViewData = new ViewDataDictionary<ExportarRelatoriosModel>(ViewData, this)
            };
        }

        public async Task<IActionResult> OnGetExportarCsvAsync()
        {
            var query = ConstruirQueryFiltros();
            int limite = ObterLimiteEfetivo();
            var resultados = await query.Take(limite).ToListAsync();

            var builder = new StringBuilder();
            builder.AppendLine("Nome;CPF;Apelido;Celular;Telefone;Email;Data Nascimento;Gênero;Profissão;Distrito;Bairro;Cidade;UF;CEP;Tags;Status");

            foreach (var p in resultados)
            {
                var tagsTexto = ObterTagsStringPessoa(p);
                builder.AppendLine($"\"{p.Nome}\";\"{p.Cpf}\";\"{p.Apelido}\";\"{p.Celular}\";\"{p.Telefone}\";\"{p.Email}\";\"{p.DataNascimento?.ToString("dd/MM/yyyy")}\";\"{p.Genero}\";\"{p.Profissao}\";\"{p.Distrito}\";\"{p.Bairro}\";\"{p.Cidade}\";\"{p.Uf}\";\"{p.Cep}\";\"{tagsTexto}\";\"{p.StatusTexto}\"");
            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", $"Relatorio_Pessoas_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        public async Task<IActionResult> OnGetExportarXlsxAsync()
        {
            var query = ConstruirQueryFiltros();
            int limite = ObterLimiteEfetivo();
            var resultados = await query.Take(limite).ToListAsync();

            using var stream = new MemoryStream();
            using (var spreadsheetDocument = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = spreadsheetDocument.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                worksheetPart.Worksheet = new Worksheet(sheetData);

                var sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());
                var sheet = new Sheet()
                {
                    Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Relatório de Pessoas"
                };
                sheets.Append(sheet);

                var headerRow = new Row();
                string[] headers = { "Nome", "CPF", "Apelido", "Celular", "Telefone", "E-mail", "Data Nascimento", "Gênero", "Profissão", "Distrito", "Bairro", "Cidade", "UF", "CEP", "Tags", "Status" };

                foreach (var header in headers)
                {
                    headerRow.AppendChild(CriarCelulaTexto(header));
                }
                sheetData.AppendChild(headerRow);

                foreach (var p in resultados)
                {
                    var tagsTexto = ObterTagsStringPessoa(p);

                    var row = new Row();
                    row.AppendChild(CriarCelulaTexto(p.Nome));
                    row.AppendChild(CriarCelulaTexto(p.Cpf));
                    row.AppendChild(CriarCelulaTexto(p.Apelido));
                    row.AppendChild(CriarCelulaTexto(p.Celular));
                    row.AppendChild(CriarCelulaTexto(p.Telefone));
                    row.AppendChild(CriarCelulaTexto(p.Email));
                    row.AppendChild(CriarCelulaTexto(p.DataNascimento?.ToString("dd/MM/yyyy")));
                    row.AppendChild(CriarCelulaTexto(p.Genero));
                    row.AppendChild(CriarCelulaTexto(p.Profissao));
                    row.AppendChild(CriarCelulaTexto(p.Distrito));
                    row.AppendChild(CriarCelulaTexto(p.Bairro));
                    row.AppendChild(CriarCelulaTexto(p.Cidade));
                    row.AppendChild(CriarCelulaTexto(p.Uf));
                    row.AppendChild(CriarCelulaTexto(p.Cep));
                    row.AppendChild(CriarCelulaTexto(tagsTexto));
                    row.AppendChild(CriarCelulaTexto(p.StatusTexto));

                    sheetData.AppendChild(row);
                }

                workbookPart.Workbook.Save();
            }

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Relatorio_Pessoas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        private int ObterLimiteEfetivo()
        {
            if (!Filtro.MaximoResultados.HasValue || Filtro.MaximoResultados.Value <= 0)
                return 300;

            return Math.Min(Filtro.MaximoResultados.Value, 1000);
        }

        private Cell CriarCelulaTexto(string? texto)
        {
            return new Cell
            {
                DataType = CellValues.String,
                CellValue = new CellValue(texto ?? string.Empty)
            };
        }

        private static string ObterTagsStringPessoa(PessoaEntity p)
        {
            if (!string.IsNullOrWhiteSpace(p.TagsTexto))
                return p.TagsTexto;

            if (p.Tags != null && p.Tags.Any())
                return string.Join(", ", p.Tags.Select(t => t.Nome));

            return string.Empty;
        }

        private IQueryable<PessoaEntity> ConstruirQueryFiltros()
        {
            var query = _context.Pessoas
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Filtro.Distrito))
                query = query.Where(p => p.Distrito == Filtro.Distrito);

            if (!string.IsNullOrWhiteSpace(Filtro.Genero))
                query = query.Where(p => p.Genero == Filtro.Genero);

            if (!string.IsNullOrWhiteSpace(Filtro.Tag))
            {
                string tagBuscada = Filtro.Tag.Trim();
                var palavras = tagBuscada
                    .Split(new[] { ' ', ',', ';', '-' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length > 2)
                    .ToList();

                string termoPrincipal = palavras.LastOrDefault() ?? tagBuscada;

                query = query.Where(p =>
                    (p.TagsTexto != null && EF.Functions.Like(p.TagsTexto, $"%{termoPrincipal}%")) ||
                    (p.TagsTexto != null && EF.Functions.Like(p.TagsTexto, $"%{tagBuscada}%")) ||
                    p.Tags.Any(t => EF.Functions.Like(t.Nome, $"%{tagBuscada}%") || EF.Functions.Like(t.Nome, $"%{termoPrincipal}%"))
                );
            }

            if (Filtro.PadrinhoId.HasValue && Filtro.PadrinhoId.Value != Guid.Empty)
            {
                Guid padrinhoGuid = Filtro.PadrinhoId.Value;
                query = query.Where(p => p.PadrinhoId == padrinhoGuid);
            }

            if (Filtro.DataNascimentoNula)
            {
                query = query.Where(p => p.DataNascimento == null);
            }
            else
            {
                if (Filtro.DataNascimentoDe.HasValue)
                    query = query.Where(p => p.DataNascimento >= Filtro.DataNascimentoDe.Value);

                if (Filtro.DataNascimentoAte.HasValue)
                    query = query.Where(p => p.DataNascimento <= Filtro.DataNascimentoAte.Value);
            }

            if (Filtro.FiltrarContatosNascidosEm)
            {
                if (Filtro.NascidoDia.HasValue)
                    query = query.Where(p => p.DataNascimento.HasValue && p.DataNascimento.Value.Day == Filtro.NascidoDia.Value);

                if (Filtro.NascidoMes.HasValue)
                    query = query.Where(p => p.DataNascimento.HasValue && p.DataNascimento.Value.Month == Filtro.NascidoMes.Value);

                if (Filtro.NascidoAno.HasValue)
                    query = query.Where(p => p.DataNascimento.HasValue && p.DataNascimento.Value.Year == Filtro.NascidoAno.Value);
            }

            if (Filtro.FiltrarContatosSemWhatsApp)
            {
                query = query.Where(p => p.PossuiZap == "NAO" || p.PossuiZap == "NÃO" || (p.Celular != null && p.Celular == ""));

                if (Filtro.SemWhatsAppDia.HasValue)
                    query = query.Where(p => p.DataNascimento.HasValue && p.DataNascimento.Value.Day == Filtro.SemWhatsAppDia.Value);

                if (Filtro.SemWhatsAppMes.HasValue)
                    query = query.Where(p => p.DataNascimento.HasValue && p.DataNascimento.Value.Month == Filtro.SemWhatsAppMes.Value);
            }

            if (Filtro.ContatosSemInteracao)
            {
                var pessoasComInteracaoIds = _context.InteracoesPessoa.Select(i => i.PessoaId);
                query = query.Where(p => !pessoasComInteracaoIds.Contains(p.Id));
            }
            else if (Filtro.ContatosComInteracao)
            {
                var queryInteracoes = _context.InteracoesPessoa.AsNoTracking().AsQueryable();

                if (Filtro.InteracaoDataInicial.HasValue)
                    queryInteracoes = queryInteracoes.Where(i => i.DataHora >= Filtro.InteracaoDataInicial.Value);

                if (Filtro.InteracaoDataFinal.HasValue)
                    queryInteracoes = queryInteracoes.Where(i => i.DataHora <= Filtro.InteracaoDataFinal.Value);

                var pessoasComInteracaoIds = queryInteracoes.Select(i => i.PessoaId).Distinct();
                query = query.Where(p => pessoasComInteracaoIds.Contains(p.Id));
            }

            query = query.OrderBy(p => p.Nome);

            return query;
        }

        private async Task CarregarListasAgrupadasAsync()
        {
            var distritos = await _context.Pessoas
                .AsNoTracking()
                .Where(p => !string.IsNullOrEmpty(p.Distrito))
                .Select(p => p.Distrito!)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();
            DistritosList = new SelectList(distritos);

            var tags = await _context.Tags
                .AsNoTracking()
                .Where(t => t.Ativo)
                .Select(t => new { Valor = t.Nome, Nome = t.Nome })
                .Distinct()
                .OrderBy(t => t.Nome)
                .ToListAsync();
            TagsList = new SelectList(tags, "Valor", "Nome");

            var padrinhos = await _context.CabosEleitorais
                .AsNoTracking()
                .Select(c => new { Id = c.Id, Nome = c.Nome ?? "[Sem Nome]" })
                .OrderBy(c => c.Nome)
                .ToListAsync();
            PadrinhosList = new SelectList(padrinhos, "Id", "Nome");

            var generos = await _context.Pessoas
                .AsNoTracking()
                .Where(p => !string.IsNullOrEmpty(p.Genero))
                .Select(p => p.Genero!)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();
            GenerosList = new SelectList(generos);
        }
    }
}