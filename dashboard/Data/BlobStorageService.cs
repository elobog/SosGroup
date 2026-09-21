using Azure.Storage.Blobs;

namespace dashboard.Data;

public class BlobStorageService
{
    private readonly BlobContainerClient containerPerfilesCargo;

    public BlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"]
            ?? throw new InvalidOperationException("Falta la configuración 'AzureStorage:ConnectionString'.");
        var containerName = configuration["AzureStorage:ContainerPerfilesCargo"]
            ?? throw new InvalidOperationException("Falta la configuración 'AzureStorage:ContainerPerfilesCargo'.");

        containerPerfilesCargo = new BlobContainerClient(connectionString, containerName);
    }

    public async Task<string> SubirPerfilCargoDocumentoAsync(int perfilCargoVersionId, string nombreArchivo, Stream contenido)
    {
        var rutaBlob = $"{perfilCargoVersionId}/{Guid.NewGuid()}-{nombreArchivo}";
        var blobClient = containerPerfilesCargo.GetBlobClient(rutaBlob);
        await blobClient.UploadAsync(contenido, overwrite: true);
        return rutaBlob;
    }

    public async Task<byte[]> DescargarPerfilCargoDocumentoAsync(string rutaBlob)
    {
        var blobClient = containerPerfilesCargo.GetBlobClient(rutaBlob);
        var respuesta = await blobClient.DownloadContentAsync();
        return respuesta.Value.Content.ToArray();
    }
}
