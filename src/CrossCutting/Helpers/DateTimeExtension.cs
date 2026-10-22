using System.Runtime.InteropServices;

namespace CrossCutting.Helpers
{
    public static class DateTimeExtensions
    {
        public static DateTime ObterDataHoraBrasilia()
        {
            // Trata o nome da TimeZone para ser compatível tanto com Windows quanto com Linux/Docker
            string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "E. South America Standard Time"
                : "America/Sao_Paulo";

            TimeZoneInfo tzBrasilia = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzBrasilia);
        }

        public static DateTime ParaHorarioBrasilia(this DateTime dateTime)
        {
            // Retorna o valor exato digitado no formulário, sem fuso (Unspecified),
            // garantindo que o MySQL grave exatamente a hora informada (ex: 14:00 -> 14:00)
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
        }
              

        

    }
}