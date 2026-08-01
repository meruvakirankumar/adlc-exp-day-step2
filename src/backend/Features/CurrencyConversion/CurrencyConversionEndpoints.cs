using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace OuterloopLabApi.Features.CurrencyConversion;

public static class CurrencyConversionEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/conversions", async (
            ConvertCurrencyRequest request,
            CurrencyProviderClient provider,
            IConversionAuditRepository repository,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (request.Amount <= 0m)
                return Results.Problem(statusCode: (int)HttpStatusCode.BadRequest, title: "Invalid amount");

            if (string.IsNullOrWhiteSpace(request.SourceCurrency) || string.IsNullOrWhiteSpace(request.TargetCurrency))
                return Results.Problem(statusCode: (int)HttpStatusCode.BadRequest, title: "Missing currency codes");

            var source = request.SourceCurrency.Trim().ToUpperInvariant();
            var target = request.TargetCurrency.Trim().ToUpperInvariant();

            var executedAtUtc = DateTimeOffset.UtcNow;
            var conversionId = Guid.NewGuid().ToString("N");

            try
            {
                var providerResult = await provider.GetLatestRateAsync(source, target, cancellationToken);

                var appliedRate = Math.Round(providerResult.AppliedRate, 6, MidpointRounding.AwayFromZero);
                var convertedAmount = Math.Round(request.Amount * appliedRate, 4, MidpointRounding.AwayFromZero);

                var record = new ConversionAuditRecord
                {
                    ConversionId = conversionId,
                    SourceCurrency = source,
                    TargetCurrency = target,
                    OriginalAmount = request.Amount,
                    AppliedRate = appliedRate,
                    ConvertedAmount = convertedAmount,
                    ProviderDateMarker = providerResult.ProviderDateMarker,
                    ExecutedAtUtc = executedAtUtc
                };

                await repository.CreateAsync(record, cancellationToken);

                return Results.Ok(record);
            }
            catch (CurrencyProviderClient.CurrencyProviderUnavailableException)
            {
                // Provider errors must not write partial audit records.
                var problem = new ProblemDetails
                {
                    Status = (int)HttpStatusCode.ServiceUnavailable,
                    Title = "Currency provider unavailable",
                    Detail = "Unable to fetch a valid exchange rate right now."
                };
                return Results.Problem(problem);
            }
        }).WithTags("Currency Conversion");

        app.MapGet("/api/conversions/{conversionId}", async (
            string conversionId,
            IConversionAuditRepository repository,
            CancellationToken cancellationToken) =>
        {
            var record = await repository.GetByIdAsync(conversionId, cancellationToken);
            if (record is null)
                return Results.Problem(statusCode: (int)HttpStatusCode.NotFound, title: "Conversion not found");

            return Results.Ok(record);
        }).WithTags("Currency Conversion");
    }
}
