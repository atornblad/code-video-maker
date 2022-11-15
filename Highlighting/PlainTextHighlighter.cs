namespace CodeVideoMaker.Highlighting;

class PlainTextHighlighter : Highlighter
{
    public override string Highlight(string input)
    {
        return new string(' ', input.Length);
    }
}