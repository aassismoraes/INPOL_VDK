using Application.Tags.InputModels;
using CrossCutting.Helpers;
using Data.Connections;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages.Tags;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public CadastroTagInputModel TagInput { get; set; } = new();

    public List<TagEntity> Tags { get; set; } = new();

    // Sugestões de categorias padrão para a operação política
    public List<string> CategoriasSugeridas { get; set; } = new()
    {
        "Engajamento",       // Ex: Voto Certo, Indeciso, Multiplicador
        "Pauta / Interesse", // Ex: Saúde, Educação, Segurança, Esporte
        "Perfil / Função"    // Ex: Liderança Comunitária, Comerciante, Professor
    };

    public async Task OnGetAsync()
    {
        Tags = await _context.Set<TagEntity>()
            .AsNoTracking()
            .Where(t => t.Ativo)
            .Include(t => t.Apoiadores)
            .OrderBy(t => t.Categoria)
            .ThenBy(t => t.Nome)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var novaTag = new TagEntity
        {
            Nome = TagInput.Nome.Trim(),
            Categoria = TagInput.Categoria.Trim(),
            Ativo = true,
            CriadoEm = DateTimeExtensions.ObterDataHoraBrasilia()
        };

        _context.Set<TagEntity>().Add(novaTag);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Tag criada com sucesso!";
        return RedirectToPage();
    }
}