using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace CodeVideoMaker.Output;

class IdeOutput : IOutput
{
    private int titleHeight;
    private Bitmap bitmap;
    private Graphics graphics;
    private IOutput parent;
    private List<string> filenames;
    private Font activeFont;
    private Font inactiveFont;
    private Brush backgroundBrush;
    private Brush activeBrush;
    private Brush inactiveBrush;
    private int currentIndex;
    private Pen edgePen;

    public IdeOutput(int width, int height, int titleHeight, IOutput parent)
    {
        bitmap = new Bitmap(width, height);
        graphics = Graphics.FromImage(bitmap);
        this.titleHeight = titleHeight;
        this.parent = parent;
        filenames = new List<string>();
        activeFont = new Font("Calibri", titleHeight * 0.5f, FontStyle.Bold);
        inactiveFont = new Font("Calibri", titleHeight * 0.5f);
        backgroundBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
            new Point(0, 0),
            new Point(0, titleHeight),
            Color.FromArgb(0x80, 0x90, 0xb0),
            Color.FromArgb(0x70, 0x7e, 0x9a)
        );
        activeBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
            new Point(0, 0),
            new Point(0, titleHeight),
            Color.FromArgb(0x38, 0x3f, 0x4d),
            Color.FromArgb(0, 0, 0)
        );
        inactiveBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
            new Point(0, 0),
            new Point(0, titleHeight),
            Color.FromArgb(0x60, 0x6c, 0x84),
            Color.FromArgb(0x38, 0x3f, 0x4d)
        );
        edgePen = new Pen(Brushes.Black, Math.Max(1.0f, titleHeight * 0.02f));
    }

    public void SetFilename(string filename)
    {
        EnsurePresentAndFirst(filename);
    }

    public void AddFrame(Bitmap editorBitmap)
    {
        graphics.Clear(Color.Black);
        graphics.FillRectangle(backgroundBrush, 0, 0, bitmap.Width, titleHeight);
        RenderTabs();
        graphics.DrawImage(editorBitmap, 0, titleHeight);
        parent.AddFrame(bitmap);
    }

    private void RenderTabs()
    {
        float x = 0.0f;
        float tabPadding = titleHeight * 0.4f;
        float tabTop = titleHeight * 0.1f;

        for (int i = 0; i < filenames.Count; ++i)
        {
            var font = (i == currentIndex) ? activeFont : inactiveFont;
            var background = (i == currentIndex) ? activeBrush : inactiveBrush;
            float textWidth = graphics.MeasureString(filenames[i], font).Width;
            graphics.FillRectangle(background, x, tabTop, textWidth + tabPadding * 2, titleHeight - tabTop);
            graphics.DrawLines(edgePen, new []
            {
                new PointF(x, titleHeight),
                new PointF(x, tabTop),
                new PointF(x + textWidth + tabPadding * 2, tabTop),
                new PointF(x + textWidth + tabPadding * 2, titleHeight)
            });
            graphics.DrawString(filenames[i], font, Brushes.White, x + tabPadding, tabTop);
            x += textWidth + tabPadding * 2;
        }
    }

    private void EnsurePresentAndFirst(string currentFilename)
    {
        int index = filenames.IndexOf(currentFilename);
        if (index == -1)
        {
            filenames.Add(currentFilename);
            currentIndex = filenames.Count - 1;
        }
        else
        {
            currentIndex = index;
        }
        /*else if (index > 0)
        {
            filenames.RemoveAt(index);
            filenames.Insert(0, currentFilename);
        }*/
    }

    public void Dispose()
    {
        graphics.Dispose();
        bitmap.Dispose();
    }
}

