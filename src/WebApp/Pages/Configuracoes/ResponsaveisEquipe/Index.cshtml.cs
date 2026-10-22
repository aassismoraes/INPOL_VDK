using CrossCutting.Helpers;
using Data.Connections;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Pages.Configuracoes.ResponsaveisEquipe;

[Authorize(Policy = "EquipeEstrategica")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<ResponsavelDto> Responsaveis { get; set; } = new();

    [BindProperty]
    public ResponsavelInputModel Input { get; set; } = new();

    public class ResponsavelDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? CargoOuFuncao { get; set; }
        public bool Ativo { get; set; }
    }

    public class ResponsavelInputModel
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        public string? Email { get; set; }

        public string? Telefone { get; set; }

        [StringLength(100, ErrorMessage = "O cargo deve ter no máximo 100 caracteres.")]
        public string? CargoOuFuncao { get; set; }
    }

    public async Task OnGetAsync()
    {
        await CarregarResponsaveisAsync();
    }

    public async Task<IActionResult> OnPostSalvarAsync()
    {
        if (!ModelState.IsValid)
        {
            await CarregarResponsaveisAsync();
            return Page();
        }

        // --- EDIÇÃO ---
        if (Input.Id.HasValue && Input.Id.Value != Guid.Empty)
        {
            var item = await _context.Set<ResponsavelEquipeEntity>().FindAsync(Input.Id.Value);
            if (item != null)
            {
                item.Nome = Input.Nome.Trim();
                item.Email = string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email.Trim().ToLower();
                item.Telefone = Input.Telefone;
                item.FuncaoOuCargo = Input.CargoOuFuncao?.Trim();
                item.AtualizadoEm = DateTimeExtensions.ObterDataHoraBrasilia();

                _context.Set<ResponsavelEquipeEntity>().Update(item);
                TempData["SuccessMessage"] = "Responsável atualizado com sucesso!";
            }
        }
        // --- CRIAÇÃO ---
        else
        {
            var novo = new ResponsavelEquipeEntity
            {
                Id = Guid.NewGuid(),
                Nome = Input.Nome.Trim(),
                Email = string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email.Trim().ToLower(),
                Telefone = Input.Telefone,
                FuncaoOuCargo = Input.CargoOuFuncao?.Trim(),
                Ativo = true,
                CriadoEm = DateTimeExtensions.ObterDataHoraBrasilia()
            };

            await _context.Set<ResponsavelEquipeEntity>().AddAsync(novo);
            TempData["SuccessMessage"] = "Responsável cadastrado com sucesso!";
        }

        await _context.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAlternarStatusAsync(Guid id)
    {
        var item = await _context.Set<ResponsavelEquipeEntity>().FindAsync(id);
        if (item != null)
        {
            item.Ativo = !item.Ativo;
            item.AtualizadoEm = DateTimeExtensions.ObterDataHoraBrasilia();

            await _context.SaveChangesAsync();
            var statusStr = item.Ativo ? "ativado" : "desativado";
            TempData["SuccessMessage"] = $"Responsável {item.Nome} foi {statusStr} com sucesso!";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoverAsync(Guid id)
    {
        var item = await _context.Set<ResponsavelEquipeEntity>().FindAsync(id);
        if (item != null)
        {
            _context.Set<ResponsavelEquipeEntity>().Remove(item);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Responsável {item.Nome} foi removido com sucesso!";
        }

        return RedirectToPage();
    }

    private async Task CarregarResponsaveisAsync()
    {
        Responsaveis = await _context.Set<ResponsavelEquipeEntity>()
            .AsNoTracking()
            .OrderByDescending(r => r.Ativo)
            .ThenBy(r => r.Nome)
            .Select(r => new ResponsavelDto
            {
                Id = r.Id,
                Nome = r.Nome,
                Email = r.Email,
                Telefone = r.Telefone,
                CargoOuFuncao = r.FuncaoOuCargo,
                Ativo = r.Ativo
            })
            .ToListAsync();
    }
}