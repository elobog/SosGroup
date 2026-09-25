using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using UglyToad.PdfPig.Writer;

namespace dashboard.Data;

// Un PDF exportado de Computrabajo trae muchos currículums seguidos; cada uno empieza en una página
// nueva con el encabezado "Curriculum de <nombre>" y ocupa de 1 a 4 páginas. Separarlos acá (sin IA)
// permite mandar a OpenAI un CV por llamada en vez del documento completo.
public static partial class CvSeparador
{
    public record Segmento(int Numero, int PaginaInicio, int PaginaFin, string Texto, byte[]? Pdf);

    [GeneratedRegex(@"Curr[ií]culum\s+de\s", RegexOptions.IgnoreCase)]
    private static partial Regex EncabezadoCv();

    public static List<Segmento> Separar(byte[] contenidoPdf)
    {
        using var documento = PdfDocument.Open(contenidoPdf);
        var textos = documento.GetPages().Select(TextoDePagina).ToList();

        var inicios = Enumerable.Range(0, textos.Count).Where(i => EncabezadoCv().IsMatch(textos[i])).ToList();

        // Sin encabezados reconocibles: se trata el archivo completo como un único CV (PDF individual).
        if (inicios.Count == 0)
            return [new Segmento(1, 1, textos.Count, string.Join(Environment.NewLine, textos), null)];

        var segmentos = new List<Segmento>();
        for (var k = 0; k < inicios.Count; k++)
        {
            var desde = inicios[k];
            var hasta = k + 1 < inicios.Count ? inicios[k + 1] - 1 : textos.Count - 1;
            var texto = string.Join(Environment.NewLine, textos.Skip(desde).Take(hasta - desde + 1));
            segmentos.Add(new Segmento(k + 1, desde + 1, hasta + 1, texto, ConstruirPdf(documento, desde + 1, hasta + 1)));
        }
        return segmentos;
    }

    // Extractor por orden de contenido: separa palabras y líneas (page.Text las deja pegadas).
    private static string TextoDePagina(UglyToad.PdfPig.Content.Page pagina)
    {
        try { return ContentOrderTextExtractor.GetText(pagina); }
        catch { return pagina.Text; }
    }

    private static byte[]? ConstruirPdf(PdfDocument origen, int paginaInicio, int paginaFin)
    {
        try
        {
            var constructor = new PdfDocumentBuilder();
            for (var pagina = paginaInicio; pagina <= paginaFin; pagina++)
                constructor.AddPage(origen, pagina);
            return constructor.Build();
        }
        catch
        {
            return null;
        }
    }
}
