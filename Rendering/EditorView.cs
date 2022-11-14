using System.Drawing;
using System.Text.RegularExpressions;

namespace CodeVideoMaker.Rendering;

class EditorView
{
    private readonly Graphics graphics;
    private readonly Bitmap bitmap;

    private List<string> lines = new List<string>();
    private int topLine = 0;
    private int currentLine = 0;
    private int currentColumn = 0;
    private int rowsPerScreen = 24;
    private string filename;

    public EditorView(Graphics graphics, Bitmap bitmap, string filename, IEnumerable<string> initialLines)
    {
        this.graphics = graphics;
        this.bitmap = bitmap;
        this.filename = filename;
        this.lines.AddRange(initialLines);
    }

    public void RenderFrame(bool showCursor = true)
    {
        // TODO: Introduce syntax highlighting
        // TODO: Introduce window frame, maybe? Or just a border?

        double frameWidth = bitmap.Width;
        double frameHeight = bitmap.Height;
        double titleHeight = frameHeight * 0.07;

        double totalWidth = frameWidth;
        double totalHeight = frameHeight - titleHeight;
        double lineHeight = totalHeight / rowsPerScreen;
        double charWidth = lineHeight * 0.5;
        double codeTop = titleHeight;
        double codeBottom = titleHeight + totalHeight;

        using (var blue = new SolidBrush(Color.FromArgb(255, 0, 0, 192)))
        {
            graphics.FillRectangle(blue, 0, 0, (float)frameWidth, (float)titleHeight);
            graphics.DrawString(filename, new Font("Arial", (float)(titleHeight * 0.6)), Brushes.White, (float)(frameWidth / 400), 0);
        }


        using (var black = new SolidBrush(Color.FromArgb(192, 0, 0, 0)))
        {
            graphics.FillRectangle(black, 0, (float)codeTop, (float)totalWidth, (float)totalHeight);
        }
        //graphics.Clear(Color.Black);    // TODO: Make this configurable
        using var font = new Font("Consolas", (float)lineHeight * 0.7f);
        
        int row = topLine;
        double y = codeTop;
        while (row < lines.Count && y < codeBottom)
        {
            if (row < 0) continue;
            string line = lines[row] + " ";
            double x = 0.0;
            int col = 0;
            foreach (char c in line)
            {
                if (row == currentLine && col == currentColumn && showCursor)
                {
                    graphics.FillRectangle(Brushes.White, (float)x, (float)y, (float)charWidth, (float)lineHeight);
                    graphics.DrawString(c.ToString(), font, Brushes.DarkBlue, (float)x, (float)y);
                }
                else
                {
                    graphics.DrawString(c.ToString(), font, Brushes.White, (float)x, (float)y);
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

    public void AddChar(char c, Action<Bitmap> output)
    {
        lines[currentLine] = lines[currentLine].Substring(0, currentColumn) + c + lines[currentLine].Substring(currentColumn);
        ++currentColumn;
        RenderFrame();
        output(bitmap);
    }

    public void Backspace(Action<Bitmap> output)
    {
        if (currentColumn > 0)
        {
            lines[currentLine] = lines[currentLine].Substring(0, currentColumn - 1) + lines[currentLine].Substring(currentColumn);
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
        Console.WriteLine($"Adding line {line} with text {text}");
        string indentation = GetIndentation(text);
        lines.Insert(line, indentation);
        MoveCursorTo(line, indentation.Length, output, true);
        RenderFrame();
        output(bitmap);
        for (int len = indentation.Length; len < text.Length; ++len)
        {
            AddChar(text[len], output);
/*            lines[line] = text.Substring(0, len);
            MoveCursorTo(line, lines[line].Length, output);
            RenderFrame();
            output(bitmap);*/
        }
    }

    public void DeleteLine(int line, Action<Bitmap> output)
    {
        Console.WriteLine($"Deleting line {line} with text {lines[line]}");
        MoveCursorTo(line, 0, output);
        lines.RemoveAt(line);
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
        int targetTopLine = lineNumber - (rowsPerScreen / 3);
        if (targetTopLine > lines.Count - rowsPerScreen + 8) targetTopLine = lines.Count - rowsPerScreen + 8;
        if (targetTopLine < 0) targetTopLine = 0;
        if (Math.Abs(targetTopLine - topLine) < 4) return;

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