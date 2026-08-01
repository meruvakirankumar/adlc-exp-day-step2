using System.Text.Json;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.CosmosDB;
using Azure.ResourceManager.CosmosDB.Models;
using Microsoft.Azure.Cosmos;
using OuterloopLabApi.Features.CurrencyConversion;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Dev-only local-runner escape hatch. Opt-in via USE_INMEMORY_AUDIT=true.
// When true, all Cosmos startup (env-var reads, ARM, data-plane provisioning) is skipped
// and audit records are held in memory only. Must never be set in Production/CI.
var useInMemoryAudit = string.Equals(
    Environment.GetEnvironmentVariable("USE_INMEMORY_AUDIT"),
    "true",
    StringComparison.OrdinalIgnoreCase);

builder.Services.AddHttpClient<CurrencyProviderClient>((sp, http) =>
{
    var providerBaseUrl = Environment.GetEnvironmentVariable("CURRENCY_API_BASE_URL") ?? "https://frankfurter.dev";
    var effectiveBase = providerBaseUrl.TrimEnd('/');

    // Constraint requires defaulting to https://frankfurter.dev but the actual HTTP API is hosted on api.frankfurter.dev.
    if (effectiveBase.Equals("https://frankfurter.dev", StringComparison.OrdinalIgnoreCase))
        effectiveBase = "https://api.frankfurter.dev";

    http.BaseAddress = new Uri(effectiveBase, UriKind.Absolute);
    http.Timeout = TimeSpan.FromSeconds(8);
    http.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddSingleton<CurrencyProviderResponseAdapter>();

if (useInMemoryAudit)
{
    builder.Services.AddSingleton<IConversionAuditRepository, InMemoryConversionAuditRepository>();
}
else
{
    // Keep API configuration strictly environment-driven (no appsettings for Cosmos).
    var cosmosUri = Environment.GetEnvironmentVariable("COSMOS_DB_URI")
                    ?? throw new InvalidOperationException("Missing env var COSMOS_DB_URI");
    var cosmosDatabaseName = Environment.GetEnvironmentVariable("COSMOS_DB_DATABASE")
                               ?? throw new InvalidOperationException("Missing env var COSMOS_DB_DATABASE");
    var cosmosContainerName = Environment.GetEnvironmentVariable("COSMOS_DB_CONTAINER")
                               ?? throw new InvalidOperationException("Missing env var COSMOS_DB_CONTAINER");
    var cosmosAccountName = Environment.GetEnvironmentVariable("COSMOS_DB_ACCOUNT_NAME")
                              ?? throw new InvalidOperationException("Missing env var COSMOS_DB_ACCOUNT_NAME");
    var cosmosResourceGroup = Environment.GetEnvironmentVariable("COSMOS_DB_RESOURCE_GROUP")
                               ?? throw new InvalidOperationException("Missing env var COSMOS_DB_RESOURCE_GROUP");
    var cosmosRegion = Environment.GetEnvironmentVariable("COSMOS_DB_REGION")
                       ?? throw new InvalidOperationException("Missing env var COSMOS_DB_REGION");
    var managedIdentityClientId = Environment.GetEnvironmentVariable("AZURE_MANAGED_IDENTITY_CLIENT_ID")
                                    ?? throw new InvalidOperationException("Missing env var AZURE_MANAGED_IDENTITY_CLIENT_ID");

    // Optional for ARM calls; if absent we still fulfill the data-plane provisioning requirement.
    var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID")
                        ?? Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");

    // Provision Cosmos before the web app starts.
    var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
    {
        ManagedIdentityClientId = managedIdentityClientId
    });

    // ARM is best-effort by constraint.
    try
    {
        if (!string.IsNullOrWhiteSpace(subscriptionId))
        {
            var armClient = new ArmClient(credential, subscriptionId);
            await ArmCosmosProvisioning.BestEffortAsync(
                armClient,
                subscriptionId,
                cosmosResourceGroup,
                cosmosAccountName,
                cosmosDatabaseName,
                cosmosContainerName,
                cosmosRegion);
        }
    }
    catch
    {
        // Ignore ARM provisioning errors; data-plane will handle create-if-not-exists.
    }

    // Required data-plane create-if-not-exists: must fail startup if it cannot create.
    var cosmosClient = new CosmosClient(cosmosUri, credential);
    var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(cosmosDatabaseName);

    var containerProperties = new ContainerProperties(cosmosContainerName, "/conversionId");
    var container = await database.Database.CreateContainerIfNotExistsAsync(containerProperties, throughput: 400);

    builder.Services.AddSingleton(container);
    builder.Services.AddSingleton<IConversionAuditRepository, ConversionAuditRepository>();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

CurrencyConversionEndpoints.Map(app);

app.Run();

static class ArmCosmosProvisioning
{
    public static async Task BestEffortAsync(
        ArmClient armClient,
        string subscriptionId,
        string resourceGroup,
        string accountName,
        string databaseName,
        string containerName,
        string region)
    {
        var accountId = CosmosDBAccountResource.CreateResourceIdentifier(subscriptionId, resourceGroup, accountName);
        var account = armClient.GetCosmosDBAccountResource(accountId);

        var location = new AzureLocation(region);

        var sqlDbResource = new CosmosDBSqlDatabaseResourceInfo(databaseName);
        var sqlDbContent = new CosmosDBSqlDatabaseCreateOrUpdateContent(location, sqlDbResource);

        var dbOp = await account.GetCosmosDBSqlDatabases().CreateOrUpdateAsync(
            WaitUntil.Completed,
            databaseName,
            sqlDbContent);

        var partitionKey = new CosmosDBContainerPartitionKey
        {
            Kind = CosmosDBPartitionKind.Hash
        };
        partitionKey.Paths.Add("/conversionId");

        var sqlContainerResource = new CosmosDBSqlContainerResourceInfo(containerName)
        {
            PartitionKey = partitionKey
        };
        var sqlContainerContent = new CosmosDBSqlContainerCreateOrUpdateContent(location, sqlContainerResource);

        await dbOp.Value.GetCosmosDBSqlContainers().CreateOrUpdateAsync(
            WaitUntil.Completed,
            containerName,
            sqlContainerContent);
    }
}
