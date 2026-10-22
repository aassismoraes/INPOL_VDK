using CrossCutting.Helpers;
using Data.Connections;
using Data.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Pages.Configuracoes.Usuarios;

[Authorize(Policy = "EquipeEstrategica")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<UsuarioDto> Usuarios { get; set; } = new();
    public List<CaboEleitoralDto> CabosEleitoraisDisponiveis { get; set; } = new();

    [BindProperty]
    public UsuarioInputModel Input { get; set; } = new();

    public int TotalUsuarios { get; set; }
    public int LimiteUsuarios { get; set; }

    public class UsuarioDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefone { get; set; }
        public string Perfil { get; set; } = "Assessor";
        public bool Ativo { get; set; }
        public DateTime CriadoEm { get; set; }
        public DateTime? UltimoAcesso { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class CaboEleitoralDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? Bairro { get; set; }
        public string? AreaInfluencia { get; set; }
    }

    public class UsuarioInputModel
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        public string Email { get; set; } = string.Empty;

        public string? Telefone { get; set; }

        [Required(ErrorMessage = "Selecione o perfil de acesso.")]
        public string Perfil { get; set; } = "Assessor";
    }

    public async Task OnGetAsync()
    {
        // 1. Busca o político cadastrado (usa IgnoreQueryFilters caso ele esteja inativo por engano)
        var politico = await _context.Set<PoliticoEntity>()
            .AsNoTracking()
            .IgnoreQueryFilters()
            .OrderByDescending(p => p.CriadoEm)
            .FirstOrDefaultAsync();

        // 2. Define o limite (se não achar ou for <= 0, assume o padrão 10)
        LimiteUsuarios = (politico != null && politico.LimiteUsuarios > 0) ? politico.LimiteUsuarios : 10;

        // 3. Carrega a lista de usuários ativos/cadastrados
        await CarregarUsuariosAsync();
        await CarregarCabosEleitoraisAsync();

        // 4. Define o total exato de usuários ativos no sistema
        TotalUsuarios = Usuarios.Count(u => u.Ativo);
    }
    public async Task<IActionResult> OnPostSalvarAsync()
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Verifique os campos do formulário antes de guardar.";
            await CarregarUsuariosAsync();
            await CarregarCabosEleitoraisAsync();
            return Page();
        }

        if (!Enum.TryParse<PerfilAcesso>(Input.Perfil, out var perfilEnum))
        {
            perfilEnum = PerfilAcesso.Assessor;
        }

        var emailTratado = Input.Email.Trim().ToLower();
        CaboEleitoralEntity caboEleitoralVinculado = null; // Variável para armazenar o cabo encontrado

        // VALIDAÇÃO E BUSCA DO CABO ELEITORAL
        if (perfilEnum == PerfilAcesso.CaboEleitoral)
        {
            // Aqui removemos o AsNoTracking para podermos atualizar o registo depois
            caboEleitoralVinculado = await _context.Set<CaboEleitoralEntity>()
                .FirstOrDefaultAsync(c => c.Ativo && c.Email != null && c.Email.ToLower() == emailTratado);

            if (caboEleitoralVinculado == null)
            {
                var msgCabo = "O e-mail informado não pertence a um cabo eleitoral ativo cadastrado.";
                ModelState.AddModelError("Input.Email", msgCabo);
                TempData["ErrorMessage"] = msgCabo;

                await CarregarUsuariosAsync();
                await CarregarCabosEleitoraisAsync();
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Input.Nome))
            {
                Input.Nome = caboEleitoralVinculado.Nome;
            }
        }

        // ATUALIZAÇÃO DE USUÁRIO EXISTENTE
        if (Input.Id.HasValue && Input.Id.Value != Guid.Empty)
        {
            var usuario = await _context.Set<UsuarioEntity>().FindAsync(Input.Id.Value);
            if (usuario != null)
            {
                usuario.Nome = Input.Nome.Trim();
                usuario.Email = emailTratado;
                usuario.Telefone = Input.Telefone;
                usuario.Perfil = perfilEnum;
                usuario.AtualizadoEm = DateTimeExtensions.ObterDataHoraBrasilia();

                _context.Set<UsuarioEntity>().Update(usuario);

                // VINCULAR O ID AO CABO ELEITORAL SE APLICÁVEL
                if (perfilEnum == PerfilAcesso.CaboEleitoral && caboEleitoralVinculado != null)
                {
                    caboEleitoralVinculado.UsuarioId = usuario.Id;
                    _context.Set<CaboEleitoralEntity>().Update(caboEleitoralVinculado);
                }

                TempData["SuccessMessage"] = "Usuário atualizado com sucesso!";
            }
        }
        // CRIAÇÃO DE NOVO USUÁRIO
        else
        {
            var politico = await _context.Set<PoliticoEntity>()
                .AsNoTracking()
                .OrderByDescending(p => p.CriadoEm)
                .FirstOrDefaultAsync();

            int limiteMaximo = (politico != null && politico.LimiteUsuarios > 0) ? politico.LimiteUsuarios : 10;
            int totalUsuariosAtuais = await _context.Set<UsuarioEntity>().CountAsync();

            if (totalUsuariosAtuais >= limiteMaximo)
            {
                TempData["ErrorMessage"] = $"Limite de {limiteMaximo} usuários atingido para este gabinete! Não é possível adicionar novos acessos.";
                return RedirectToPage();
            }

            var emailExiste = await _context.Set<UsuarioEntity>()
                .AnyAsync(u => u.Email.ToLower() == emailTratado);

            if (emailExiste)
            {
                var msgEmail = "Este e-mail já está cadastrado.";
                ModelState.AddModelError("Input.Email", msgEmail);
                TempData["ErrorMessage"] = msgEmail;

                await CarregarUsuariosAsync();
                await CarregarCabosEleitoraisAsync();
                return Page();
            }

            var novoUsuario = new UsuarioEntity
            {
                Id = Guid.NewGuid(),
                Nome = Input.Nome.Trim(),
                Email = emailTratado,
                Telefone = Input.Telefone,
                Perfil = perfilEnum,
                Ativo = true,
                CriadoEm = DateTimeExtensions.ObterDataHoraBrasilia()
            };

            await _context.Set<UsuarioEntity>().AddAsync(novoUsuario);

            // VINCULAR O ID AO CABO ELEITORAL (NOVO USUÁRIO)
            if (perfilEnum == PerfilAcesso.CaboEleitoral && caboEleitoralVinculado != null)
            {
                caboEleitoralVinculado.UsuarioId = novoUsuario.Id;
                _context.Set<CaboEleitoralEntity>().Update(caboEleitoralVinculado);
            }

            TempData["SuccessMessage"] = "Novo usuário liberado para acesso com sucesso!";
        }

        await _context.SaveChangesAsync();
        return RedirectToPage();
    }

    // Alternar Status (Ativar / Desativar)
    public async Task<IActionResult> OnPostAlternarStatusAsync(Guid id)
    {
        var usuario = await _context.Set<UsuarioEntity>().FindAsync(id);
        if (usuario != null)
        {
            var meuid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (meuid != null && Guid.Parse(meuid) == usuario.Id)
            {
                TempData["ErrorMessage"] = "Você não pode desativar sua própria conta!";
                return RedirectToPage();
            }

            usuario.Ativo = !usuario.Ativo;
            usuario.AtualizadoEm = DateTimeExtensions.ObterDataHoraBrasilia();

            await _context.SaveChangesAsync();

            var acao = usuario.Ativo ? "ativado" : "desativado";
            TempData["SuccessMessage"] = $"Usuário {usuario.Nome} foi {acao} com sucesso!";
        }

        return RedirectToPage();
    }

    // Remover Usuário
    public async Task<IActionResult> OnPostRemoverAsync(Guid id)
    {
        var usuario = await _context.Set<UsuarioEntity>().FindAsync(id);
        if (usuario != null)
        {
            var meuid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (meuid != null && Guid.Parse(meuid) == usuario.Id)
            {
                TempData["ErrorMessage"] = "Você não pode remover sua própria conta!";
                return RedirectToPage();
            }

            _context.Set<UsuarioEntity>().Remove(usuario);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Usuário {usuario.Nome} foi removido com sucesso!";
        }

        return RedirectToPage();
    }

    private async Task CarregarUsuariosAsync()
    {
        Usuarios = await _context.Set<UsuarioEntity>()
            .AsNoTracking()
            .OrderByDescending(u => u.Ativo)
            .ThenBy(u => u.Nome)
            .Select(u => new UsuarioDto
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email,
                Telefone = u.Telefone,
                Perfil = u.Perfil.ToString(),
                Ativo = u.Ativo,
                CriadoEm = u.CriadoEm,
                UltimoAcesso = u.UltimoAcesso,
                AvatarUrl = u.AvatarUrl
            })
            .ToListAsync();
    }

    private async Task CarregarCabosEleitoraisAsync()
    {
        CabosEleitoraisDisponiveis = await _context.Set<CaboEleitoralEntity>()
            .AsNoTracking()
            .Where(c => c.Ativo)
            .OrderBy(c => c.Nome)
            .Select(c => new CaboEleitoralDto
            {
                Id = c.Id,
                Nome = c.Nome,
                Email = c.Email,
                Telefone = c.Telefone,
                Bairro = c.Bairro,
                AreaInfluencia = c.AreaInfluencia
            })
            .ToListAsync();
    }
}