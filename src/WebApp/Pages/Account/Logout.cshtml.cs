using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Account;

[AllowAnonymous]
public class LogoutModel : PageModel
{
    public async Task<IActionResult> OnPostAsync()
    {
        // 1. Limpa o cookie de autenticação
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // 2. Limpa o TempData para evitar mensagens acumuladas
        TempData.Clear();

        // 3. Redireciona para a página de Login avisando que foi deslogado
        return RedirectToPage("/Login", new { deslogado = true });
    }

    public IActionResult OnGet()
    {
        return RedirectToPage("/Login");
    }
}