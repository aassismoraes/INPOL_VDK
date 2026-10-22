using CrossCutting.Helpers;
using Data.Connections;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Pages.Configuracoes.Tags;

[Authorize(Policy = "EquipeGabinete")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<TagGroupDto> GruposDeTags { get; set; } = new();

    [BindProperty]
    public TagInputModel Input { get; set; } = new();

    public class TagGroupDto
    {
        public string Categoria { get; set; } = string.Empty;
        public List<TagEntity> Tags { get; set; } = new();
    }

    public class TagInputModel
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "O nome da tag é obrigatório.")]
        [StringLength(50, ErrorMessage = "O nome deve ter no máximo 50 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "A categoria é obrigatória.")]
        [StringLength(50, ErrorMessage = "A categoria deve ter no máximo 50 caracteres.")]
        public string Categoria { get; set; } = "Geral";
    }

    public async Task OnGetAsync()
    {
        await CarregarTagsAsync();
    }

    public async Task<IActionResult> OnPostSalvarAsync()
    {
        if (!ModelState.IsValid)
        {
            await CarregarTagsAsync();
            return Page();
        }

        if (Input.Id.HasValue && Input.Id.Value != Guid.Empty)
        {
            // Alterar Tag Existente
            var tag = await _context.Set<TagEntity>().FindAsync(Input.Id.Value);
            if (tag != null)
            {
                tag.Nome = Input.Nome.Trim();
                tag.Categoria = Input.Categoria.Trim();
                tag.AtualizadoEm = DateTimeExtensions.ObterDataHoraBrasilia();
                _context.Set<TagEntity>().Update(tag);
            }
        }
        else
        {
            // Criar Nova Tag
            var novaTag = new TagEntity
            {
                Id = Guid.NewGuid(),
                Nome = Input.Nome.Trim(),
                Categoria = Input.Categoria.Trim(),
                Ativo = true,
                CriadoEm = DateTimeExtensions.ObterDataHoraBrasilia()
            };
            await _context.Set<TagEntity>().AddAsync(novaTag);
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Tag salva com sucesso!";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostExcluirAsync(Guid id)
    {
        var tag = await _context.Set<TagEntity>()
            .Include(t => t.Apoiadores)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tag != null)
        {
            _context.Set<TagEntity>().Remove(tag);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Tag removida com sucesso!";
        }

        return RedirectToPage();
    }

    private async Task CarregarTagsAsync()
    {
        var tags = await _context.Set<TagEntity>()
            .Include(t => t.Apoiadores)
            .AsNoTracking()
            .Where(t => t.Ativo)
            .OrderBy(t => t.Categoria)
            .ThenBy(t => t.Nome)
            .ToListAsync();

        GruposDeTags = tags
            .GroupBy(t => t.Categoria)
            .Select(g => new TagGroupDto
            {
                Categoria = g.Key,
                Tags = g.ToList()
            })
            .ToList();
    }
}