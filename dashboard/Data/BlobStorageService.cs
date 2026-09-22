using Azure.Storage.Blobs;

namespace dashboard.Data;

public class BlobStorageService
{
    private readonly BlobContainerClient containerPerfilesCargo;
    private readonly BlobContainerClient containerPostulantes;

    public BlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"]
            ?? throw new InvalidOperationException("Falta la configuración 'AzureStorage:ConnectionString'.");

        var containerPerfilesCargoName = configuration["AzureStorage:ContainerPerfilesCargo"]
            ?? throw new InvalidOperationException("Falta la configuración 'AzureStorage:ContainerPerfilesCargo'.");
        var containerPostulantesName = configuration["AzureStorage:ContainerPostulantes"]
            ?? throw new InvalidOperationException("Falta la configuración 'AzureStorage:ContainerPostulantes'.");

        containerPerfilesCargo = new BlobContainerClient(connectionString, containerPerfilesCargoName);
        containerPostulantes = new BlobContainerClient(connectionString, containerPostulantesName);
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

    public async Task<string> SubirDocumentoPostulanteAsync(int postulanteId, string nombreArchivo, Stream contenido)
    {
        var rutaBlob = $"{postulanteId}/{Guid.NewGuid()}-{nombreArchivo}";
        var blobClient = containerPostulantes.GetBlobClient(rutaBlob);
        await blobClient.UploadAsync(contenido, overwrite: true);
        return rutaBlob;
    }

    public async Task<byte[]> DescargarDocumentoPostulanteAsync(string rutaBlob)
    {
        var blobClient = containerPostulantes.GetBlobClient(rutaBlob);
        var respuesta = await blobClient.DownloadContentAsync();
        return respuesta.Value.Content.ToArray();
    }
}
