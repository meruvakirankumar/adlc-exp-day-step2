using System.Text.Json;

namespace OuterloopLabApi.Features.CurrencyConversion;

public sealed class CurrencyProviderResponseAdapter
{
    public sealed class CurrencyProviderResponseAdapterException : Exception
    {
        public CurrencyProviderResponseAdapterException(string message) : base(message) { }
    }

    public sealed record CurrencyProviderResult(decimal AppliedRate, string ProviderDateMarker);

    public CurrencyProviderResult AdaptRate(JsonDocument document, string targetCurrency)
    {
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new CurrencyProviderResponseAdapterException("Currency provider payload is not an object.");

        var root = document.RootElement;
        var providerDateMarker = TryGetString(root, "date")
                                  ?? TryGetString(root, "provider_date")
                                  ?? TryGetString(root, "providerDate")
                                  ?? TryGetString(root, "timestamp")
                                  ?? throw new CurrencyProviderResponseAdapterException("Currency provider payload is missing a date marker.");

        var appliedRate = TryExtractRateFromRates(root, targetCurrency)
                           ?? TryExtractRateFromConversionRates(root, targetCurrency)
                           ?? TryExtractRateFromScalar(root)
                           ?? throw new CurrencyProviderResponseAdapterException("Currency provider payload does not contain an extractable rate.");

        return new CurrencyProviderResult(appliedRate, providerDateMarker);
    }

    private static decimal? TryExtractRateFromRates(JsonElement root, string targetCurrency)
    {
        if (!TryGetObjectProperty(root, "rates", out var rates))
            return null;

        foreach (var prop in rates.EnumerateObject())
        {
            if (!string.Equals(prop.Name, targetCurrency, StringComparison.OrdinalIgnoreCase))
                continue;

            if (TryGetDecimal(prop.Value, out var v))
                return v;
        }

        return null;
    }

    private static decimal? TryExtractRateFromConversionRates(JsonElement root, string targetCurrency)
    {
        if (!TryGetObjectProperty(root, "conversion_rates", out var rates))
            return null;

        foreach (var prop in rates.EnumerateObject())
        {
            if (!string.Equals(prop.Name, targetCurrency, StringComparison.OrdinalIgnoreCase))
                continue;

            if (TryGetDecimal(prop.Value, out var v))
                return v;
        }

        return null;
    }

    private static decimal? TryExtractRateFromScalar(JsonElement root)
    {
        // Supports optional scalar shapes (e.g. v2 rate endpoints).
        if (!root.TryGetProperty("rate", out var rateEl))
            return null;

        return TryGetDecimal(rateEl, out var v) ? v : null;
    }

    private static bool TryGetObjectProperty(JsonElement root, string propertyName, out JsonElement value)
    {
        if (root.TryGetProperty(propertyName, out value) && value.ValueKind == JsonValueKind.Object)
            return true;

        value = default;
        return false;
    }

    private static string? TryGetString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value))
            return null;

        if (value.ValueKind == JsonValueKind.String)
            return value.GetString();

        return null;
    }

    private static bool TryGetDecimal(JsonElement element, out decimal value)
    {
        try
        {
            if (element.ValueKind == JsonValueKind.Number)
            {
                value = element.GetDecimal();
                return true;
            }

            if (element.ValueKind == JsonValueKind.String)
            {
                if (decimal.TryParse(element.GetString(), out value))
                    return true;
            }
        }
        catch
        {
            // fallthrough
        }

        value = default;
        return false;
    }
}
