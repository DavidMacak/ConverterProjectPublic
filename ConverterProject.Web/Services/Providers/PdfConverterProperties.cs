using iText.Html2pdf;
using iText.Html2pdf.Resolver.Font;
using iText.Layout.Font;

namespace ConverterProject.Web.Services.Providers
{
    /// <summary>
    /// Created as singleton because it uses a lot of ram. It is shared to multiple PdfConverterServices. Needed for better font conversion.
    /// </summary>
    public class PdfConverterProperties
    {
        private FontProvider fontProvider;
        public ConverterProperties converterProperties;
        public PdfConverterProperties()
        {
            fontProvider = new DefaultFontProvider(true, true, true);
            converterProperties = new ConverterProperties();
            converterProperties.SetFontProvider(fontProvider);
        }
    }
}
