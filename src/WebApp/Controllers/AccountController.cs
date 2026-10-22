using CrossCutting.Helpers;
using Data.Connections;
using Data.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace WebApp.Controllers;

[Route("[controller]")]
public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("LoginGoogle")]
    [AllowAnonymous]
    public IActionResult LoginGoogle(string returnUrl = "/")
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleCallback), new { returnUrl })
        };

        // Força a exibição do seletor de contas do Google
        properties.Parameters["prompt"] = "select_account";

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("GoogleCallback")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleCallback(string returnUrl = "/", string? remoteError = null)
    {
        if (remoteError != null)
        {
            TempData["ErrorMessage"] = $"Erro no provedor do Google: {remoteError}";
            return RedirectToPage("/Login");
        }

        // 1. Obtém as informações validadas pelo Google OAuth
        var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (!authenticateResult.Succeeded || authenticateResult.Principal == null)
        {
            TempData["ErrorMessage"] = "Falha ao autenticar com a conta do Google.";
            return RedirectToPage("/Login");
        }

        var claims = authenticateResult.Principal.Claims;
        var googleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value?.ToLower().Trim();
        var nome = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var avatarUrl = claims.FirstOrDefault(c => c.Type == "picture")?.Value;

        if (string.IsNullOrEmpty(email))
        {
            TempData["ErrorMessage"] = "Não foi possível recuperar o e-mail da sua conta Google.";
            return RedirectToPage("/Login");
        }

        // 2. BUSCA O USUÁRIO NO BANCO DE DADOS PELO E-MAIL
        var usuario = await _context.Set<UsuarioEntity>()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

        // BLOQUEIO 1: E-mail não cadastrado no banco de dados
        if (usuario == null)
        {
            TempData["ErrorMessage"] = "Sua conta não está autorizada a acessar o sistema.";
            return RedirectToPage("/AcessoNegado");
        }

        // BLOQUEIO 2: Usuário cadastrado, porém inativo
        if (!usuario.Ativo)
        {
            TempData["ErrorMessage"] = "Seu acesso ao sistema está inativo. Entre em contato com o administrador.";
            return RedirectToPage("/AcessoNegado");
        }

        // 3. Atualiza os dados do usuário com informações vindas do Google
        usuario.GoogleId = googleId;
        usuario.AvatarUrl = avatarUrl;
        usuario.UltimoAcesso = DateTimeExtensions.ObterDataHoraBrasilia();

        _context.Set<UsuarioEntity>().Update(usuario);
        await _context.SaveChangesAsync();

        // 4. Monta os Claims locais com a Role/Perfil do Banco
        var claimsInternos = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Perfil.ToString()),
            new("AvatarUrl", usuario.AvatarUrl ?? string.Empty)
        };

        var identity = new ClaimsIdentity(claimsInternos, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeExtensions.ObterDataHoraBrasilia().AddDays(7)
        };

        // 5. Cria a sessão local no Cookie
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

        return LocalRedirect(returnUrl);
    }

    [HttpGet("Logout")]
    [HttpPost("Logout")]
    public async Task<IActionResult> Logout()
    {
        // Encerra a sessão apenas do Cookie da aplicação local
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        TempData.Clear();

        return RedirectToPage("/Login", new { deslogado = "true" });
    }
}