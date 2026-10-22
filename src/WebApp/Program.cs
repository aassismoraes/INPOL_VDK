using Data.Connections;
using Ioc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WebApp.Filters;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddSerilog();
Log.Information("Starting web host");

// Configura o ASP.NET para reconhecer os cabeçalhos HTTPS vindo do Nginx Proxy Manager
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// 2. Razor Pages e Autorização
builder.Services.AddRazorPages(options =>
{
    // Bloqueia toda a aplicação por padrão (exige usuário logado)
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AuthorizePage("/Index");

    // Libera as páginas públicas
    options.Conventions.AllowAnonymousToPage("/Login");
    options.Conventions.AllowAnonymousToPage("/AcessoNegado");

    // Restringe a pasta de Usuários do Gabinete APENAS para perfil Administrador
    options.Conventions.AuthorizeFolder("/Configuracoes/Usuarios", "ApenasAdmin");

    // Permite que a listagem e detalhes de Eventos sejam acessados por Gabinete e Cabos
    options.Conventions.AuthorizePage("/Eventos/Index", "AcessoEventos");
    options.Conventions.AuthorizePage("/Eventos/Detalhes", "AcessoEventos");

    // Mantém criação, edição e exclusão restritas à equipe do Gabinete
    options.Conventions.AuthorizePage("/Eventos/Cadastrar", "EquipeGabinete");
    options.Conventions.AuthorizePage("/Eventos/Editar", "EquipeGabinete");
    options.Conventions.AuthorizePage("/Eventos/Excluir", "EquipeGabinete");

    options.Conventions.AddFolderApplicationModelConvention("/", model =>
    {
        model.Filters.Add(new Microsoft.AspNetCore.Mvc.TypeFilterAttribute(typeof(WebApp.Filters.AlertasFilter)));
    });
});

// Suporte para Controllers
builder.Services.AddControllers();

// 3. Configuração de Autenticação (Cookie + Google)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Login";
    options.AccessDeniedPath = "/AcessoNegado";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);

    // Força a montagem de caminhos e cookies sob o esquema HTTPS
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
});

// 4. Políticas de Autorização
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApenasAdmin", policy =>
        policy.RequireRole("Administrador"));

    options.AddPolicy("EquipeEstrategica", policy =>
        policy.RequireRole("Administrador", "Assessor"));

    options.AddPolicy("EquipeGabinete", policy =>
        policy.RequireRole("Administrador", "Assessor", "Atendimento"));

    // Nova política exclusiva para Cabos Eleitorais
    options.AddPolicy("ApenasCaboEleitoral", policy =>
        policy.RequireRole("CaboEleitoral"));

    // Política para páginas compartilhadas (ex: Visualização de Eventos)
    options.AddPolicy("AcessoEventos", policy =>
        policy.RequireRole("Administrador", "Assessor", "Atendimento", "CaboEleitoral"));
});

// 5. DbContext MySQL + Spatial
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")),
        mySqlOptions => mySqlOptions.UseNetTopologySuite()
    );
});

// 6. Injeção de Dependências
builder.Services.ResolveDependencies();
builder.Services.AddHttpClient();

var app = builder.Build();

// OBRIGATÓRIO: PRIMEIRO middleware a ser executado
app.UseForwardedHeaders();

// Pipeline de Requisições HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Nota: app.UseHttpsRedirection() foi omitido para deixar a responsabilidade de TLS/SSL no Nginx Proxy Manager

app.UseStaticFiles();
app.UseRouting();

// Autenticação antes da Autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
try
{


    // Executa todas as migrations pendentes
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
    }

}
catch (Exception ex)
{

}
await app.RunAsync();