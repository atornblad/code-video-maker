using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;

namespace CodeVideoMaker.Rendering;

class EditorView
{
    private readonly Graphics graphics;
    private readonly Bitmap bitmap;

    private List<string> lines = new List<string>();
    private List<string> colors = new List<string>();
    private int topLine = 0;
    private int currentLine = 0;
    private int currentColumn = 0;
    private int rowsPerScreen = 24;
    private string filename;
    private Highlighting.Highlighter highlighter;
    private Dictionary<char, Brush> brushes = new Dictionary<char, Brush>();

    public int CurrentLine => currentLine;

    public EditorView(Graphics graphics, Bitmap bitmap, string filename, IEnumerable<string> initialLines)
    {
        this.graphics = graphics;
        this.bitmap = bitmap;
        this.filename = filename;
        this.lines.AddRange(initialLines);
        highlighter = Highlighting.Highlighter.For(filename);
        brushes['c'] = new SolidBrush(Color.FromArgb(118, 152,  92));
        brushes['i'] = new SolidBrush(Color.FromArgb(121, 191, 250));
        brushes['k'] = new SolidBrush(Color.FromArgb(185, 137, 189));
        brushes['r'] = new SolidBrush(Color.FromArgb(109, 119, 230));
        //brushes['r'] = new SolidBrush(Color.FromArgb(122, 198, 177));
        brushes['r'] = new SolidBrush(Color.FromArgb(220, 220, 175));
        brushes['o'] = new SolidBrush(Color.FromArgb(246, 216,  71));
        brushes['s'] = new SolidBrush(Color.FromArgb(194, 147, 124));
        brushes['n'] = new SolidBrush(Color.FromArgb(187, 205, 171));
        brushes[' '] = new SolidBrush(Color.FromArgb(212, 212, 212));
    }

    private static readonly Random random = new Random();

    private Brush GetBrush(int line, int col)
    {
        string colorForLine = line < colors.Count ? colors[line] : "";
        char style = col < colorForLine.Length ? colorForLine[col] : ' ';
        if (!brushes.ContainsKey(style))
        {
            var color = Color.FromArgb(random.Next(156) + 100, random.Next(156) + 100, random.Next(100) + 156);
            Debug.WriteLine($"Creating brush for style {style}: {color}");
            brushes[style] = new SolidBrush(color);
        }
        return brushes[style];
    }

    public void RenderFrame(bool showCursor = true)
    {
        // TODO: Introduce syntax highlighting
        // TODO: Introduce window frame, maybe? Or just a border?

        double frameWidth = bitmap.Width;
        double frameHeight = bitmap.Height;

        double totalWidth = frameWidth;
        double totalHeight = frameHeight;
        double lineHeight = totalHeight / rowsPerScreen;
        double charWidth = lineHeight * 0.5;
        double codeTop = 0.0;
        double codeBottom = codeTop + totalHeight;

        using (var black = new SolidBrush(Color.FromArgb(192, 0, 0, 0)))
        {
            graphics.FillRectangle(black, 0, (float)codeTop, (float)totalWidth, (float)totalHeight);
        }
        //graphics.Clear(Color.Black);    // TODO: Make this configurable
        using var font = new Font("Consolas", (float)lineHeight * 0.65f);
        var format = new StringFormat(StringFormatFlags.FitBlackBox)
        {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Center
        };
        
        int row = topLine;
        double y = codeTop;
        while (row <= lines.Count && y < codeBottom)
        {
            if (row < 0) continue;
            string line = row == lines.Count ? " " : lines[row] + " ";
            double x = 0.0;
            int col = 0;
            foreach (char c in line)
            {
                var point = new PointF((float)(x + charWidth / 2), (float)(y + lineHeight / 2));
                if (row == currentLine && col == currentColumn && showCursor)
                {
                    graphics.FillRectangle(Brushes.White, (float)x, (float)y, (float)charWidth, (float)lineHeight);
                    graphics.DrawString(c.ToString(), font, Brushes.DarkBlue, point, format);
                }
                else
                {
                    var brush = GetBrush(row, col);
                    graphics.DrawString(c.ToString(), font, brush, point, format);
                }
                ++col;
                x += charWidth;
            }
            ++row;
            y += lineHeight;
        }
    }

    public void Blink(TimeSpan span, double fps, Action<Bitmap> output) {
        var start = TimeSpan.Zero;
        var blinkLength = TimeSpan.FromSeconds(0.8);
        var brightLength = TimeSpan.FromSeconds(0.5);
        var blink = TimeSpan.Zero;
        while (start < span)
        {
            RenderFrame(blink < brightLength);
            output(bitmap);
            start += TimeSpan.FromSeconds(1.0 / fps);
            blink += TimeSpan.FromSeconds(1.0 / fps);
            if (blink > blinkLength)
            {
                blink = TimeSpan.Zero;
            }
        }
    }

    private void UpdateFormat(int line)
    {
        colors[line] = highlighter.Highlight(lines[line]);
    }

    public void AddChar(char c, Action<Bitmap> output)
    {
        lines[currentLine] = lines[currentLine].Substring(0, currentColumn) + c + lines[currentLine].Substring(currentColumn);
        UpdateFormat(currentLine);
        ++currentColumn;
        RenderFrame();
        output(bitmap);
    }

    public void Backspace(Action<Bitmap> output)
    {
        if (currentColumn > 0)
        {
            lines[currentLine] = lines[currentLine].Substring(0, currentColumn - 1) + lines[currentLine].Substring(currentColumn);
            UpdateFormat(currentLine);
            --currentColumn;
            RenderFrame();
            output(bitmap);
        }
        else
        {
            throw new InvalidOperationException($"Cannot backspace at column {currentColumn}");
        }
    }

    public void AddLine(int line, string text, Action<Bitmap> output)
    {
        Debug.WriteLine($"Adding line {line} with text {text}");
        string indentation = GetIndentation(text);
        lines.Insert(line, indentation);
        colors.Insert(line, indentation);
        MoveCursorTo(line, indentation.Length, output, true);
        RenderFrame();
        output(bitmap);
        for (int len = indentation.Length; len < text.Length; ++len)
        {
            AddChar(text[len], output);
        }
        Debug.WriteLine($"Line {line} is colored       {colors[line]}");
    }

    public void DeleteLine(int line, Action<Bitmap> output)
    {
        Debug.WriteLine($"Deleting line {line} with text {lines[line]}");
        MoveCursorTo(line, 0, output);
        lines.RemoveAt(line);
        colors.RemoveAt(line);
        RenderFrame();
        output(bitmap);
        output(bitmap);
    }

    private string GetIndentation(string line)
    {
        return Regex.Match(line, @"^\s*").Value;
    }

    public void MoveCursorTo(int line, int column, Action<Bitmap> output, bool instantColumn = false)
    {
        // TODO: Add support for different speeds
        // TODO: Move column nicer
        //currentColumn = column;
        while (currentLine != line)
        {
            if (currentLine < line)
            {
                ++currentLine;
            }
            else
            {
                --currentLine;
            }
            if (currentColumn >= lines[currentLine].Length)
            {
                currentColumn = lines[currentLine].Length;
            }
            ScrollIntoView(currentLine, output);
            RenderFrame();
            output(bitmap);
        }

        if (instantColumn) currentColumn = column;
        while (currentColumn != column)
        {
            if (currentColumn < column)
            {
                ++currentColumn;
            }
            else
            {
                --currentColumn;
            }
            RenderFrame();
            output(bitmap);
        }
    }

    public void ScrollIntoView(int lineNumber, Action<Bitmap> output)
    {
        // TODO: Add support for different speeds
        int safeTop = topLine + 2;
        int safeBottom = topLine + rowsPerScreen - 4;

        int targetTopLine = topLine;
        if (lineNumber < safeTop) {
            targetTopLine = Math.Max(0, lineNumber - 2);
        }
        if (lineNumber > safeBottom) {
            targetTopLine = Math.Min(lines.Count - rowsPerScreen + 4, lineNumber - rowsPerScreen + 4);
        }

        while (topLine != targetTopLine)
        {
            if (topLine < targetTopLine)
            {
                ++topLine;
            }
            else
            {
                --topLine;
            }
            RenderFrame();
            output(bitmap);
        }
    }
}