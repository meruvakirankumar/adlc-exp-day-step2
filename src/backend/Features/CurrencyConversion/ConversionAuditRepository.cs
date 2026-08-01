using System.Collections.Concurrent;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace OuterloopLabApi.Features.CurrencyConversion;

public interface IConversionAuditRepository
{
    Task CreateAsync(ConversionAuditRecord record, CancellationToken cancellationToken);
    Task<ConversionAuditRecord?> GetByIdAsync(string conversionId, CancellationToken cancellationToken);
}

public sealed class ConversionAuditRepository : IConversionAuditRepository
{
    private readonly Container _container;

    public ConversionAuditRepository(Container container)
    {
        _container = container;
    }

    public async Task CreateAsync(ConversionAuditRecord record, CancellationToken cancellationToken)
    {
        await _container.CreateItemAsync(record, new PartitionKey(record.ConversionId), cancellationToken: cancellationToken);
    }

    public async Task<ConversionAuditRecord?> GetByIdAsync(string conversionId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _container.ReadItemAsync<ConversionAuditRecord>(conversionId, new PartitionKey(conversionId), cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}

// Dev-only, opt-in via USE_INMEMORY_AUDIT=true. Never used in Production/CI: audit records are lost on restart.
public sealed class InMemoryConversionAuditRepository : IConversionAuditRepository
{
    private readonly ConcurrentDictionary<string, ConversionAuditRecord> _store = new();

    public Task CreateAsync(ConversionAuditRecord record, CancellationToken cancellationToken)
    {
        _store[record.ConversionId] = record;
        return Task.CompletedTask;
    }

    public Task<ConversionAuditRecord?> GetByIdAsync(string conversionId, CancellationToken cancellationToken)
    {
        _store.TryGetValue(conversionId, out var record);
        return Task.FromResult(record);
    }
}
