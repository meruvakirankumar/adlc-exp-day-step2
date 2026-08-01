namespace OuterloopLabApi.Features.CurrencyConversion;

public sealed class ConvertCurrencyRequest
{
    public decimal Amount { get; init; }
    public string SourceCurrency { get; init; } = string.Empty;
    public string TargetCurrency { get; init; } = string.Empty;
}
