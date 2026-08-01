using System.Net.Http.Json;
using System.Text.Json;

namespace OuterloopLabApi.Features.CurrencyConversion;

public sealed class CurrencyProviderClient
{
    private readonly HttpClient _http;
    private readonly CurrencyProviderResponseAdapter _adapter;

    public CurrencyProviderClient(HttpClient http, CurrencyProviderResponseAdapter adapter)
    {
        _http = http;
        _adapter = adapter;
    }

    public sealed class CurrencyProviderUnavailableException : Exception
    {
        public CurrencyProviderUnavailableException(string message, Exception? inner = null) : base(message, inner) { }
    }

    public async Task<CurrencyProviderResponseAdapter.CurrencyProviderResult> GetLatestRateAsync(
        string sourceCurrency,
        string targetCurrency,
        CancellationToken cancellationToken)
    {
        try
        {
            // Frankfurter v1 compatible shape: { date, rates: { TARGET: rate } }
            // Works with the adapter constraints (rates.{TARGET} / conversion_rates.{TARGET}).
            var requestUri = $"/v1/latest?base={Uri.EscapeDataString(sourceCurrency)}&symbols={Uri.EscapeDataString(targetCurrency)}";

            using var response = await _http.GetAsync(requestUri, cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new CurrencyProviderUnavailableException($"Currency provider returned HTTP {(int)response.StatusCode}.");

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            return _adapter.AdaptRate(doc, targetCurrency);
        }
        catch (CurrencyProviderUnavailableException)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException || ex is JsonException)
        {
            throw new CurrencyProviderUnavailableException("Currency provider unavailable", ex);
        }
        catch (CurrencyProviderResponseAdapter.CurrencyProviderResponseAdapterException ex)
        {
            throw new CurrencyProviderUnavailableException("Currency provider unavailable", ex);
        }
        catch (Exception ex)
        {
            throw new CurrencyProviderUnavailableException("Currency provider unavailable", ex);
        }
    }
}
