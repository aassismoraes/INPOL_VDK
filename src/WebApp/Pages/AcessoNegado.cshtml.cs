using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

[AllowAnonymous]
public class AcessoNegadoModel : PageModel
{
    public async Task OnGetAsync()
    {
        // Força a destruição de qualquer Cookie de sessão residual assim que cai nesta tela
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}