using Data.Connections;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Notification;
using System.Security.Claims;

namespace WebApp._0___Utils
{
    public static class Util
    {      

        public static async Task<PoliticoEntity?> GetPolitico(ApplicationDbContext context)
        {
            // Busca o ID do primeiro político disponível no banco de dados
            var politico = await context.Politicos.FirstOrDefaultAsync();

            if (politico == null)
            {
                GlobalNotifications.Instance.AddNotification("Nenhum político cadastrado no sistema para vincular ao apoiador.");
                return null;
            }

            return politico;

        }

        public static async Task<CaboEleitoralEntity?> GetCaboEleitoralLogado(ApplicationDbContext context, ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return null;

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                // Busca o cabo eleitoral pelo UsuarioId ou diretamente pelo Id caso coincidam
                return await context.CabosEleitorais
                    .FirstOrDefaultAsync(c => (c.UsuarioId.HasValue && c.UsuarioId.Value == userId) || c.Id == userId);
            }

            return null;
        }

      
    }
}
