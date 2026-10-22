using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Application.Services;

public interface IGeoService
{
    Task<(double lat, double lng)?> ObterCoordenadasPorCepAsync(string? cep);
    Task<(double lat, double lng)?> ObterCoordenadasPorEnderecoAsync(string? logradouro, string? numero, string? bairro, string? cidade, string? uf);
    double? NormalizarLatitude(double? latitude);
    double? NormalizarLongitude(double? longitude);
}

public class GeoService : IGeoService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public GeoService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<(double lat, double lng)?> ObterCoordenadasPorCepAsync(string? cep)
    {
        if (string.IsNullOrWhiteSpace(cep)) return null;

        var cepLimpo = System.Text.RegularExpressions.Regex.Replace(cep, @"[^\d]", "");
        if (cepLimpo.Length != 8) return null;

        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetFromJsonAsync<BrasilApiCepResult>($"https://brasilapi.com.br/api/cep/v2/{cepLimpo}");

            if (response?.Location?.Coordinates != null &&
                double.TryParse(response.Location.Coordinates.Latitude, CultureInfo.InvariantCulture, out var lat) &&
                double.TryParse(response.Location.Coordinates.Longitude, CultureInfo.InvariantCulture, out var lon))
            {
                return (lat, lon);
            }
        }
        catch { }

        return null;
    }

    public async Task<(double lat, double lng)?> ObterCoordenadasPorEnderecoAsync(
        string? logradouro, string? numero, string? bairro, string? cidade, string? uf)
    {
        if (string.IsNullOrWhiteSpace(logradouro)) return null;

        var coordsLocationIq = await ObterCoordenadasLocationIqAsync(logradouro, numero, cidade, uf);
        if (coordsLocationIq.HasValue) return coordsLocationIq;

        return await ObterCoordenadasNominatimAsync(logradouro, numero, cidade, uf);
    }

    private async Task<(double lat, double lng)?> ObterCoordenadasLocationIqAsync(
        string? logradouro, string? numero, string? cidade, string? uf)
    {
        try
        {
            var apiKey = _configuration["LocationIQApiKey"];
            if (string.IsNullOrEmpty(apiKey)) return null;

            var client = _httpClientFactory.CreateClient();
            var num = (!string.IsNullOrWhiteSpace(numero) && numero != "0") ? $", {numero}" : "";
            var query = $"{logradouro}{num}, {cidade}, {uf}, Brasil";

            var url = $"https://us1.locationiq.com/v1/search?key={apiKey}&q={Uri.EscapeDataString(query)}&format=json&limit=1";

            var response = await client.GetFromJsonAsync<List<NominatimResult>>(url);
            var result = response?.FirstOrDefault();

            if (result != null &&
                double.TryParse(result.Lat, CultureInfo.InvariantCulture, out var lat) &&
                double.TryParse(result.Lon, CultureInfo.InvariantCulture, out var lon))
            {
                return (lat, lon);
            }
        }
        catch { }

        return null;
    }

    private async Task<(double lat, double lng)?> ObterCoordenadasNominatimAsync(
        string? logradouro, string? numero, string? cidade, string? uf)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("InPol-GabineteApp/1.0");

            var rua = logradouro?.Trim() ?? "";
            var ruaAbreviada = System.Text.RegularExpressions.Regex.Replace(rua, @"\bDoutor\b", "Dr.", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            ruaAbreviada = System.Text.RegularExpressions.Regex.Replace(ruaAbreviada, @"\bProfessor\b", "Prof.", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            ruaAbreviada = System.Text.RegularExpressions.Regex.Replace(ruaAbreviada, @"\bSanto\b", "St.", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            ruaAbreviada = System.Text.RegularExpressions.Regex.Replace(ruaAbreviada, @"\bSanta\b", "Sta.", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            var localidade = $"{cidade ?? ""}, {uf ?? ""}, Brasil".Trim(',', ' ');

            var queries = new List<string>
            {
                $"{ruaAbreviada}, {localidade}",
                $"{rua}, {localidade}"
            };

            foreach (var q in queries)
            {
                var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(q)}&format=json&limit=1";
                var response = await client.GetFromJsonAsync<List<NominatimResult>>(url);
                var result = response?.FirstOrDefault();

                if (result != null &&
                    double.TryParse(result.Lat, CultureInfo.InvariantCulture, out var lat) &&
                    double.TryParse(result.Lon, CultureInfo.InvariantCulture, out var lon))
                {
                    return (lat, lon);
                }
            }
        }
        catch { }

        return null;
    }

    public double? NormalizarLatitude(double? latitude)
    {
        if (!latitude.HasValue || double.IsNaN(latitude.Value)) return null;
        double lat = latitude.Value;
        while (Math.Abs(lat) > 90) lat /= 10.0;
        return Math.Round(lat, 7);
    }

    public double? NormalizarLongitude(double? longitude)
    {
        if (!longitude.HasValue || double.IsNaN(longitude.Value)) return null;
        double lng = longitude.Value;
        while (Math.Abs(lng) > 180) lng /= 10.0;
        return Math.Round(lng, 7);
    }

    private class NominatimResult
    {
        [JsonPropertyName("lat")] public string Lat { get; set; } = string.Empty;
        [JsonPropertyName("lon")] public string Lon { get; set; } = string.Empty;
    }

    private class BrasilApiCepResult
    {
        [JsonPropertyName("location")] public BrasilApiLocation? Location { get; set; }
    }

    private class BrasilApiLocation
    {
        [JsonPropertyName("coordinates")] public BrasilApiCoordinates? Coordinates { get; set; }
    }

    private class BrasilApiCoordinates
    {
        [JsonPropertyName("latitude")] public string Latitude { get; set; } = string.Empty;
        [JsonPropertyName("longitude")] public string Longitude { get; set; } = string.Empty;
    }
}