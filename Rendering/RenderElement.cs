using System.Drawing;

namespace CodeVideoMaker.Rendering;

public abstract class RenderElement
{
    public virtual Font Font { get; set; } = SystemFonts.DefaultFont;
    public abstract void Render(Graphics graphics, float x, float y, float width, float height);
}

public class LabelElement : RenderElement
{
    public string Text { get; set; }

    public LabelElement(string text)
    {
        Text = text;
    }

    public override void Render(Graphics graphics, float x, float y, float width, float height)
    {
        using var format = new StringFormat()
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Center,
        };
        graphics.DrawString(Text, Font, Brushes.White, x, y + height / 2.0f, format);
    }
}

public class ButtonElement : RenderElement
{
    public string Text { get; set; }

    public ButtonElement(string text)
    {
        Text = text;
    }

    public override void Render(Graphics graphics, float x, float y, float width, float height)
    {
        graphics.FillRectangle(Brushes.White, x, y, width, height);
        using var format = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
        };
        graphics.DrawString(Text, Font, Brushes.Black, x + width / 2.0f, y + height / 2.0f, format);
    }
}

public class TextInputElement : RenderElement
{
    private Font setFont = SystemFonts.DefaultFont;
    public override Font Font
    {
        get => setFont;
        set
        {
            setFont = value;
            base.Font = new Font(value.FontFamily, value.Size * 0.8f);
        }
    }

    public string Text { get; set; }

    public TextInputElement()
    {
        Text = "";
    }

    public override void Render(Graphics graphics, float x, float y, float width, float height)
    {
        graphics.FillRectangle(Brushes.White, x, y, width, height);
        using var format = new StringFormat()
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Center,
        };
        graphics.DrawString(Text, base.Font, Brushes.Black, x + height / 10.0f, y + height / 2.0f, format);
        var textSize = graphics.MeasureString(Text, base.Font);
        using var pen = new Pen(Brushes.Black, Math.Max(1.0f, height / 20.0f));
        graphics.DrawLine(pen, x + textSize.Width + height / 10.0f, y + height / 10.0f, x + textSize.Width + height / 10.0f, y + height * 9.0f / 10.0f);
    }
}