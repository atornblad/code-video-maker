using System.Drawing;
using CodeVideoMaker.Output;

namespace CodeVideoMaker.Rendering;

public class DialogBoxRenderer : IDisposable
{
    private int maxWidth;
    private int maxHeight;
    private int titleHeight;
    private string title;
    private Font titleFont;
    private int rowHeight;
    private Font font;
    private int colWidth;
    private int gap;
    private bool dirty = true;
    private Brush titleBrush = Brushes.Black;
    private Brush windowBrush;

    private List<PositionedRenderElement> elements = new ();

    private class PositionedRenderElement
    {
        public RenderElement Element { get; init; }
        public int col { get; init; }
        public int row { get; init; }
        public int colSpan { get; init; } = 1;
        public int rowSpan { get; init; } = 1;

        public PositionedRenderElement(RenderElement element)
        {
            this.Element = element;
        }
    }

    public DialogBoxRenderer(int maxWidth, int maxHeight, string title)
    {
        this.maxWidth = maxWidth;
        this.maxHeight = maxHeight;
        this.title = title;
        this.titleHeight = maxHeight / 16;
        this.titleFont = new Font("Calibri", titleHeight * 0.7f);
        this.rowHeight = maxHeight / 16;
        this.gap = rowHeight / 4;
        this.font = new Font("Calibri", (rowHeight - gap) * 0.7f);
        this.colWidth = rowHeight * 7 / 10;
        this.windowBrush = new SolidBrush(Color.FromArgb(0x13, 0x17, 0x21));
    }

    public void Add(RenderElement element, int col, int row, int colSpan, int rowSpan)
    {
        elements.Add(new PositionedRenderElement(element)
        {
            col = col,
            row = row,
            colSpan = colSpan,
            rowSpan = rowSpan
        });

        element.Font = font;
        dirty = true;
    }

    public void Render(Graphics graphics, int fps, TimeSpan time, Action addFrame)
    {
        if (dirty)
        {
            RecalcLayout(graphics);
        }
        for (var i = TimeSpan.Zero; i < time; i += TimeSpan.FromSeconds(1.0 / fps))
        {
            RenderPanel(graphics);
            foreach (var element in elements)
            {
                float left = colLeft(element.col);
                float top = rowTop(element.row);
                float right = colRight(element.col + element.colSpan - 1);
                float bottom = rowBottom(element.row + element.rowSpan - 1);

                element.Element.Render(graphics, left, top, right - left, bottom - top);
            }
            addFrame();
        }
    }

    private void RenderPanel(Graphics graphics)
    {
        graphics.Clear(Color.Black);
        graphics.FillRectangle(windowBrush, windowLeft, windowTop, windowWidth, windowHeight);
        graphics.FillRectangle(titleBrush, windowLeft, windowTop, windowWidth, titleHeight);
        graphics.DrawLine(Pens.Black, windowLeft, windowTop + titleHeight, windowLeft + windowWidth, windowTop + titleHeight);
        var titleFormat = new StringFormat();
        titleFormat.Alignment = StringAlignment.Near;
        titleFormat.LineAlignment = StringAlignment.Center;
        graphics.DrawString(title, titleFont, Brushes.White, windowLeft + gap, windowTop + titleHeight / 2, titleFormat);
    }

    private int columns;
    private int rows;

    private float windowLeft;
    private float windowWidth;
    private float windowTop;
    private float windowHeight;

    private void RecalcLayout(Graphics graphics)
    {
        columns = elements.Max(e => e.col + e.colSpan);
        float titleWidth = graphics.MeasureString(title, titleFont).Width;
        float titleBarMinWidth = titleWidth + 2 * gap;
        float contentPaneMinWidth = colLeft(columns, true);
        windowWidth = Math.Max(titleBarMinWidth, contentPaneMinWidth);
        windowLeft = (maxWidth - windowWidth) / 2.0f;

        rows = elements.Max(e => e.row + e.rowSpan);
        float contentHeight = rowTop(rows, true);
        windowHeight = titleHeight + contentHeight;
        windowTop = (maxHeight - windowHeight) / 2.0f;

        this.titleBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
            new PointF(0, windowTop),
            new PointF(0, windowTop + titleHeight),
            Color.FromArgb(0x80, 0x90, 0xb0),
            Color.FromArgb(0x70, 0x7e, 0x9a)
        );
    }

    private float colLeft(int col, bool raw = false)
    {
        return gap + col * colWidth + (raw ? 0 : windowLeft);
    }

    private float colRight(int col, bool raw = false)
    {
        return colWidth * (col + 1) + (raw ? 0 : windowLeft);
    }

    private float rowTop(int row, bool raw = false)
    {
        return gap + row * rowHeight + (raw ? 0 : windowTop + titleHeight);
    }

    private float rowBottom(int row, bool raw = false)
    {
        return rowHeight * (row + 1) + (raw ? 0 : windowTop + titleHeight);
    }

    public void Dispose()
    {

    }
}
