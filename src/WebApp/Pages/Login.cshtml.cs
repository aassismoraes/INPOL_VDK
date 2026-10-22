using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

[AllowAnonymous]
public class LoginModel : PageModel
{
    public bool Deslogado { get; set; }

    public IActionResult OnGet(string returnUrl = "/", bool deslogado = false)
    {
        // Define a propriedade para que a View saiba que o modal deve ser exibido
        Deslogado = deslogado;

        // Se o usuário já estiver logado, redireciona para a home do gabinete
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(returnUrl);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return Page();
    }
}