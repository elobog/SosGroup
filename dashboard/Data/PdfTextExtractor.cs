using UglyToad.PdfPig;

namespace dashboard.Data;

// Extrae el texto plano de un PDF en el servidor — así al modelo de IA solo se le manda texto,
// no el archivo completo (más simple y barato que usar un modelo con capacidad de visión).
public static class PdfTextExtractor
{
    public static string ExtraerTexto(byte[] contenidoPdf)
    {
        using var documento = PdfDocument.Open(contenidoPdf);
        var texto = new System.Text.StringBuilder();
        foreach (var pagina in documento.GetPages())
        {
            texto.AppendLine(pagina.Text);
        }
        return texto.ToString();
    }
}
