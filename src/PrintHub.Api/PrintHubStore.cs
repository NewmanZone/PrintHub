using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace PrintHub.Api;

public interface IPrintHubStore
{
    Task<PrintHubState> ReadAsync(CancellationToken ct = default);
    Task WriteAsync(PrintHubState state, CancellationToken ct = default);
}

public sealed class PrintHubStore : IPrintHubStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly BlobContainerClient? _container;
    private readonly string _localStatePath;

    public PrintHubStore(IOptions<StorageOptions> options)
    {
        var value = options.Value;
        var connectionString = value.ConnectionString ?? Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            _container = new BlobContainerClient(connectionString, value.ContainerName);
        }
        _localStatePath = Path.Combine(AppContext.BaseDirectory, value.LocalPath, "printhub-store.json");
    }

    public async Task<PrintHubState> ReadAsync(CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            if (_container is not null)
            {
                await _container.CreateIfNotExistsAsync(cancellationToken: ct);
                var blob = _container.GetBlobClient("state/printhub-store.json");
                if (!await blob.ExistsAsync(ct)) return new PrintHubState();
                var response = await blob.DownloadContentAsync(ct);
                return response.Value.Content.ToObjectFromJson<PrintHubState>(JsonOptions) ?? new PrintHubState();
            }

            if (!File.Exists(_localStatePath)) return new PrintHubState();
            await using var stream = File.OpenRead(_localStatePath);
            return await JsonSerializer.DeserializeAsync<PrintHubState>(stream, JsonOptions, ct) ?? new PrintHubState();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task WriteAsync(PrintHubState state, CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            if (_container is not null)
            {
                await _container.CreateIfNotExistsAsync(cancellationToken: ct);
                await using var stream = new MemoryStream();
                await JsonSerializer.SerializeAsync(stream, state, JsonOptions, ct);
                stream.Position = 0;
                await _container.GetBlobClient("state/printhub-store.json").UploadAsync(stream, overwrite: true, cancellationToken: ct);
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(_localStatePath)!);
            await using var file = File.Create(_localStatePath);
            await JsonSerializer.SerializeAsync(file, state, JsonOptions, ct);
        }
        finally
        {
            _gate.Release();
        }
    }
}

public interface IPrintHubFileStorage
{
    Task<string> SaveAsync(Guid productId, string fileName, Stream stream, CancellationToken ct = default);
    Task<Stream> OpenReadAsync(string storagePath, CancellationToken ct = default);
}

public sealed class PrintHubFileStorage : IPrintHubFileStorage
{
    private readonly BlobContainerClient? _container;
    private readonly string _localRoot;

    public PrintHubFileStorage(IOptions<StorageOptions> options)
    {
        var value = options.Value;
        var connectionString = value.ConnectionString ?? Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            _container = new BlobContainerClient(connectionString, value.ContainerName);
        }
        _localRoot = Path.Combine(AppContext.BaseDirectory, value.LocalPath, "files");
    }

    public async Task<string> SaveAsync(Guid productId, string fileName, Stream stream, CancellationToken ct = default)
    {
        var safeName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        var storagePath = $"files/{productId}/{Guid.NewGuid()}-{safeName}";
        if (_container is not null)
        {
            await _container.CreateIfNotExistsAsync(cancellationToken: ct);
            await _container.GetBlobClient(storagePath).UploadAsync(stream, overwrite: true, cancellationToken: ct);
            return storagePath;
        }

        var localPath = Path.Combine(_localRoot, productId.ToString(), $"{Guid.NewGuid()}-{safeName}");
        Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);
        await using var file = File.Create(localPath);
        await stream.CopyToAsync(file, ct);
        return localPath;
    }

    public async Task<Stream> OpenReadAsync(string storagePath, CancellationToken ct = default)
    {
        if (_container is not null)
        {
            var response = await _container.GetBlobClient(storagePath).DownloadStreamingAsync(cancellationToken: ct);
            return response.Value.Content;
        }

        return File.OpenRead(storagePath);
    }
}
