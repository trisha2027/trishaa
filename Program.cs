using System.Text;
using DinkToPdf;
using DinkToPdf.Contracts;
using DinkToPdfAllOs;

// Batch‑convert HTML files in ./html-files to PDF using DinkToPdf (wkhtmltopdf)
// This creates real PDFs that render the full HTML (text, Indic fonts, images, CSS).

// Use the current working directory so you can run `dotnet run` from the project folder
// and keep an `html-files` folder right there.
var baseFolder = Directory.GetCurrentDirectory();
var htmlFolder = Path.Combine(baseFolder, "html-files");
var pdfFolder  = Path.Combine(htmlFolder, "pdfs");

// Load wkhtmltox native libraries for current OS/architecture (x86/x64, Windows)
LibraryLoader.Load();

// Build CSS that uses project-bundled Indic fonts via @font-face so PDFs work even on machines without those fonts installed.
var fontsFolder = Path.Combine(baseFolder, "fonts");

string BuildFontFace(string family, string fileName)
{
    var path = Path.Combine(fontsFolder, fileName);
    if (!File.Exists(path))
    {
        Console.WriteLine($"⚠ Font file missing: {path}");
        return string.Empty;
    }

    var url = "file:///" + path.Replace("\\", "/");
    return $"@font-face {{\n  font-family: '{family}';\n  src: url('{url}') format('truetype');\n}}\n";
}

var fontFaceCss = new StringBuilder();
fontFaceCss.Append(BuildFontFace("IndicDevanagari", "NotoSansDevanagari-Regular.ttf"));
fontFaceCss.Append(BuildFontFace("IndicBengali", "NotoSansBengali-Regular.ttf"));
fontFaceCss.Append(BuildFontFace("IndicTamil", "NotoSansTamil-Regular.ttf"));
fontFaceCss.Append(BuildFontFace("IndicTelugu", "NotoSansTelugu-Regular.ttf"));
fontFaceCss.Append(BuildFontFace("IndicArabic", "NotoNaskhArabic-Regular.ttf"));

var indicFontCss = $@"
<style>
{fontFaceCss}

/* Default fallback */
* {{
  font-family: 'IndicDevanagari', 'IndicBengali', 'IndicTamil', 'IndicTelugu', 'IndicArabic', sans-serif !important;
}}

/* Arabic / Urdu (RTL) */
html[lang='ur'], body[lang='ur'], .urdu {{
  font-family: 'IndicArabic' !important;
  direction: rtl;
  unicode-bidi: isolate;
  text-align: right;
  line-height: 1.8;
}}

/* Devanagari */
html[lang='hi'], html[lang='mr'], .devanagari {{
  font-family: 'IndicDevanagari' !important;
}}

/* Bengali */
html[lang='bn'], .bengali {{
  font-family: 'IndicBengali' !important;
}}

/* Tamil */
html[lang='ta'], .tamil {{
  font-family: 'IndicTamil' !important;
}}

/* Telugu */
html[lang='te'], .telugu {{
  font-family: 'IndicTelugu' !important;
}}
</style>";
 
Console.WriteLine("🔍 Checking input HTML files...");

// Ensure folders exist
if (!Directory.Exists(htmlFolder))
{
    Console.WriteLine("❌ html-files folder NOT FOUND!");
    Console.WriteLine("Create folder next to the .exe and put your .html files inside it.");
    return;
}

Directory.CreateDirectory(pdfFolder);

var htmlFiles = Directory.GetFiles(htmlFolder, "*.html", SearchOption.TopDirectoryOnly);
Console.WriteLine($"📁 Found {htmlFiles.Length} HTML files");

if (htmlFiles.Length == 0)
{
    Console.WriteLine("❌ No HTML files found. Add .html files to the html-files folder.");
    return;
}

// Initialize DinkToPdf converter (thread‑safe wrapper)
IConverter converter = new SynchronizedConverter(new PdfTools());

foreach (var htmlFile in htmlFiles)
{
    var name = Path.GetFileNameWithoutExtension(htmlFile);
    var pdfPath = Path.Combine(pdfFolder, name + ".pdf");


    Console.WriteLine($"➡ Converting {name}.html -> {name}.pdf...");

    // Read HTML as UTF‑8 so Indic scripts render correctly
    var htmlContent = File.ReadAllText(htmlFile, Encoding.UTF8);

    // Inject @font-face CSS that points to local Indic fonts in the /fonts folder so wkhtmltopdf can embed them.
    var headCloseIndex = htmlContent.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
    if (headCloseIndex >= 0)
    {
        htmlContent = htmlContent.Insert(headCloseIndex, indicFontCss);
    }
    else
    {
        // No </head> tag found, just prepend the style at the top.
        htmlContent = indicFontCss + htmlContent;
    }

    var doc = new HtmlToPdfDocument
    {
        GlobalSettings = new GlobalSettings
        {
            ColorMode = ColorMode.Color,
            Orientation = Orientation.Portrait,
            PaperSize = PaperKind.A4,
            // Write directly to file so we don't accidentally create empty PDFs
            Out = pdfPath
        },
        Objects =
        {
            
        }
    };

    // Convert HTML -> PDF. If anything goes wrong, you'll see an exception instead of a blank PDF.
    converter.Convert(doc);

    Console.WriteLine($"✅ CREATED: {pdfPath} ({new FileInfo(pdfPath).Length} bytes)");
}

Console.WriteLine("\n🎉 Done! Open the html-files/pdfs folder and double‑click your PDFs.");
