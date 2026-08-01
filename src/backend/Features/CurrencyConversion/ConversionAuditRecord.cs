using System.Text.Json.Serialization;

namespace OuterloopLabApi.Features.CurrencyConversion;

public sealed class ConversionAuditRecord
{
    // Cosmos uses `id` as the document identifier.
    [JsonPropertyName("id")]
    public string Id
    {
        get => ConversionId;
        set => ConversionId = value;
    }

    // Partition key.
    [JsonPropertyName("conversionId")]
    public string ConversionId { get; set; } = string.Empty;

    public string SourceCurrency { get; set; } = string.Empty;
    public string TargetCurrency { get; set; } = string.Empty;

    public decimal OriginalAmount { get; set; }
    public decimal AppliedRate { get; set; }
    public decimal ConvertedAmount { get; set; }

    public string ProviderDateMarker { get; set; } = string.Empty;
    public DateTimeOffset ExecutedAtUtc { get; set; }
}
