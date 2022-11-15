using System.Text;
using System.Text.RegularExpressions;

namespace CodeVideoMaker.Highlighting;

class HtmlHighlighter : Highlighter
{
    public override string Highlight(string input)
    {
        var state = State.Outside;
        var sb = new StringBuilder();
        foreach (char c in input)
        {
            switch (state)
            {
                case State.Outside:
                    switch (c)
                    {
                        case '<':
                            // Don't add a color just yet. This could be a tag, or a comment!
                            state = State.TagOpening;
                            break;
                        default:
                            sb.Append(' ');
                            break;
                    }
                    break;
                case State.TagOpening:
                    if (c == '!') {
                        state = State.Comment;
                        sb.Append('c');
                        sb.Append('c');
                    }
                    else if (c >= 'a' && c <= 'z' || c == '/')
                    {
                        state = State.TagName;
                        sb.Append('i');
                        sb.Append('i');
                    }
                    else
                    {
                        state = State.Outside;
                        sb.Append(' ');
                        sb.Append(' ');
                    }
                    break;
                case State.Comment:
                    if (c == '-')
                    {
                        state = State.CommentFirstDash;
                    }
                    sb.Append('c');
                    break;
                case State.CommentFirstDash:
                    if (c == '-')
                    {
                        state = State.CommentSecondDash;
                    }
                    else
                    {
                        state = State.Comment;
                    }
                    sb.Append('c');
                    break;
                case State.CommentSecondDash:
                    if (c == '>')
                    {
                        state = State.Outside;
                    }
                    sb.Append('c');
                    break;
                case State.TagName:
                    if (c >= 'a' && c <= 'z')
                    {
                        sb.Append('i');
                    }
                    else if (c == ':')
                    {
                        sb.Append('o');
                    }
                    else if (c == '/')
                    {
                        state = State.SelfClosing;
                        sb.Append('i');
                    }
                    else if (c == '>')
                    {
                        state = State.Outside;
                        sb.Append('i');
                    }
                    else if (c == ' ')
                    {
                        state = State.InsideWhitespace;
                        sb.Append(' ');
                    }
                    else
                    {
                        // Probably a bad state...
                        sb.Append(' ');
                    }
                    break;
                case State.SelfClosing:
                    if (c == '>')
                    {
                        state = State.Outside;
                        sb.Append('i');
                    }
                    else
                    {
                        // Probably a bad state...
                        sb.Append(' ');
                    }
                    break;
                case State.InsideWhitespace:
                    if (c == ' ')
                    {
                        sb.Append(' ');
                    }
                    else if (c >= 'a' && c <= 'z')
                    {
                        state = State.AttributeName;
                        sb.Append('k');
                    }
                    else if (c == '>')
                    {
                        state = State.Outside;
                        sb.Append('i');
                    }
                    else if (c == '/')
                    {
                        state = State.SelfClosing;
                        sb.Append('i');
                    }
                    else
                    {
                        // Probably a bad state...
                        sb.Append(' ');
                    }
                    break;
                case State.AttributeName:
                    if (c >= 'a' && c <= 'z')
                    {
                        sb.Append('k');
                    }
                    else if (c == ':')
                    {
                        sb.Append('o');
                    }
                    else if (c == '/')
                    {
                        state = State.SelfClosing;
                        sb.Append('r');
                    }
                    else if (c == '>')
                    {
                        state = State.Outside;
                        sb.Append('r');
                    }
                    else if (c == ' ')
                    {
                        state = State.InsideWhitespace;
                        sb.Append(' ');
                    }
                    else if (c == '=')
                    {
                        state = State.AttributeValueStarting;
                        sb.Append('o');
                    }
                    else
                    {
                        // Probably a bad state...
                        sb.Append(' ');
                    }
                    break;
                case State.AttributeValueStarting:
                    if (c == '"')
                    {
                        state = State.AttributeValueQuoted;
                        sb.Append('s');
                    }
                    else if (c == '/')
                    {
                        state = State.SelfClosing;
                        sb.Append('i');
                    }
                    else if (c == '>')
                    {
                        state = State.Outside;
                        sb.Append('i');
                    }
                    else if (c != ' ')
                    {
                        state = State.AttributeValueUnquoted;
                        sb.Append('s');
                    }
                    else
                    {
                        // Probably a bad state...
                        sb.Append(' ');
                    }
                    break;
                case State.AttributeValueQuoted:
                    if (c == '"')
                    {
                        state = State.InsideWhitespace;
                    }
                    sb.Append('s');
                    break;
                case State.AttributeValueUnquoted:
                    if (c == '/')
                    {
                        state = State.SelfClosing;
                        sb.Append('i');
                    }
                    else if (c == '>')
                    {
                        state = State.Outside;
                        sb.Append('i');
                    }
                    else if (c == ' ')
                    {
                        state = State.InsideWhitespace;
                        sb.Append(' ');
                    }
                    else
                    {
                        sb.Append('s');
                    }
                    break;
            }
        }
        return sb.ToString();
    }

    private enum State
    {
        Outside,
        TagOpening,
        TagName,
        InsideWhitespace,
        AttributeName,
        AttributeValueStarting,
        AttributeValueQuoted,
        AttributeValueUnquoted,
        SelfClosing,
        Comment,
        CommentFirstDash,
        CommentSecondDash
    }
}
