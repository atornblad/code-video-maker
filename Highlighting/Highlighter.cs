namespace CodeVideoMaker.Highlighting;

abstract class Highlighter
{
    // TODO: Add initial state, like block comment or string
    public abstract string Highlight(string input);

    public static Highlighter For(string filename)
    {
        int lastDotPos = filename.LastIndexOf('.');
        if (lastDotPos == -1)
        {
            return new PlainTextHighlighter();
        }
        string extension = filename.Substring(lastDotPos + 1).ToLowerInvariant();
        switch (extension)
        {
            case "js":
                return new JavaScriptHighlighter();
            case "html":
                return new HtmlHighlighter();
            case "txt":
                return new PlainTextHighlighter();
            default:
                return new PlainTextHighlighter();
        }
    }
}
