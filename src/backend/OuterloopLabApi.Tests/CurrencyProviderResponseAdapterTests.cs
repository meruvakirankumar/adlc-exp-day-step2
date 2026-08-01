using System.Text.Json;
using OuterloopLabApi.Features.CurrencyConversion;
using Xunit;

namespace OuterloopLabApi.Tests;

public class CurrencyProviderResponseAdapterTests
{
    [Fact]
    public void Adapter_Reads_Rates_Target_Shape()
    {
        var json = "{" +
                   "\"date\":\"2026-08-01\"," +
                   "\"base\":\"USD\"," +
                   "\"rates\":{\"EUR\":0.92}" +
                   "}";

        using var doc = JsonDocument.Parse(json);

        var adapter = new CurrencyProviderResponseAdapter();
        var result = adapter.AdaptRate(doc, "EUR");

        Assert.Equal(0.92m, result.AppliedRate);
        Assert.Equal("2026-08-01", result.ProviderDateMarker);
    }

    [Fact]
    public void Adapter_Reads_ConversionRates_Target_Shape()
    {
        var json = "{" +
                   "\"date\":\"2026-08-01\"," +
                   "\"conversion_rates\":{\"EUR\":0.920000}" +
                   "}";

        using var doc = JsonDocument.Parse(json);

        var adapter = new CurrencyProviderResponseAdapter();
        var result = adapter.AdaptRate(doc, "EUR");

        Assert.Equal(0.920000m, result.AppliedRate);
        Assert.Equal("2026-08-01", result.ProviderDateMarker);
    }

    [Fact]
    public void Adapter_Throws_Domain_Exception_On_Unsupported_Payload()
    {
        var json = "{\"foo\":123}";
        using var doc = JsonDocument.Parse(json);

        var adapter = new CurrencyProviderResponseAdapter();

        Assert.Throws<CurrencyProviderResponseAdapter.CurrencyProviderResponseAdapterException>(() => adapter.AdaptRate(doc, "EUR"));
    }
}
