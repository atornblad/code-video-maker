using System.Drawing;
using System.Drawing.Drawing2D;
using CodeVideoMaker.Output;

namespace CodeVideoMaker.Rendering;

class SwitchRenderer : IDisposable
{
    private string[] allFiles;
    private int currentFilenameIndex;
    private int nextFilenameIndex;
    private IOutput output;
    private int fps;
    private Bitmap bitmap;
    private Graphics graphics;
    private float docHeight, docWidth, gap;
    private float[] fileSizes;

    public SwitchRenderer(string[] allFiles, string currentFilename, string nextFilename, int width, int height, IOutput output, int fps)
    {
        this.allFiles = allFiles;
        this.currentFilenameIndex = Array.IndexOf(allFiles, currentFilename);
        this.nextFilenameIndex = Array.IndexOf(allFiles, nextFilename);
        this.output = output;
        this.fps = fps;
        this.bitmap = new Bitmap(width, height);
        this.graphics = Graphics.FromImage(bitmap);

        docHeight = bitmap.Height * 0.5f;
        docWidth = docHeight * 0.7f;

        float totalDocsWidth = docWidth * allFiles.Length * 0.9f;
        if (totalDocsWidth > bitmap.Width)
        {
            docWidth = bitmap.Width * 0.9f / allFiles.Length;
            docHeight = docWidth / 0.7f;
        }

        int currentIndex = Array.IndexOf(allFiles, currentFilename);
        fileSizes = Enumerable.Range(0, allFiles.Length).
            Select(i => i == currentIndex ? 1.0f : 0.9f).
            ToArray();
        
        
        Console.WriteLine($"SwitchRenderer: {allFiles.Length} files, Bitmap.Width: {bitmap.Width}, docWidth: {docWidth}, totalDocsWidth: {totalDocsWidth}");
    }

    private float Smooth(float ratio)
    {
        return (float)((1.0 - Math.Cos(ratio * Math.PI)) * 0.5);
    }

    public void Render()
    {
        for (var i = 0.0f; i < 0.5f; i += 1.0f / fps)
        {
            graphics.Clear(Color.Black);
            RenderDocs();
            output.AddFrame(bitmap);
        }
        
        for (var i = 0.0f; i < 1.0f; i += 3.0f / fps)
        {
            fileSizes[nextFilenameIndex] = 0.9f + Smooth(i) * 0.1f;
            fileSizes[currentFilenameIndex] = 1.0f - Smooth(i) * 0.1f;
            graphics.Clear(Color.Black);
            RenderDocs();
            output.AddFrame(bitmap);
        }

        for (var i = 0.0f; i < 0.1f; i += 1.0f / fps)
        {
            graphics.Clear(Color.Black);
            RenderDocs();
            output.AddFrame(bitmap);
        }
    }

    private void RenderDocs()
    {
        for (int i = 0; i < allFiles.Length; ++i)
        {
            string filename = allFiles[i];
            float size = fileSizes[i];
            float centerX = bitmap.Width / 2.0f - docWidth * (allFiles.Length - 1) * 0.5f + docWidth * i;
            float centerY = bitmap.Height / 2.0f;

            float thisDocHeight = docHeight * size;
            float thisDocWidth = docWidth * size;

            float left = centerX - thisDocWidth * 0.5f;
            float top = centerY - thisDocHeight * 0.5f;
            float right = centerX + thisDocWidth * 0.5f;
            float bottom = centerY + thisDocHeight * 0.5f;
            float rightIn = right - this.docWidth * 0.1f;
            float topIn = top + this.docWidth * 0.1f;

            using var path = new GraphicsPath();
            path.AddLine(left, top, rightIn, top);
            path.AddLine(rightIn, top, right, topIn);
            path.AddLine(right, topIn, right, bottom);
            path.AddLine(right, bottom, left, bottom);
            path.AddLine(left, bottom, left, top);
            path.CloseFigure();

            using var brush = new SolidBrush(Color.FromArgb((byte)(0xf0 * size * size), (byte)(0xf0 * size * size), (byte)(0xf0 * size * size)));
            float penWidth = Math.Max(1.0f, bitmap.Width * 0.002f);
            using var pen = new Pen(Brushes.Black, penWidth);
            graphics.FillPath(brush, path);
            graphics.DrawPath(pen, path);
            graphics.DrawLine(pen, rightIn, top, right, topIn);
            using var font = new Font("Calibri", docHeight * 0.07f * size);

            using var format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Near;
            graphics.DrawString(filename, font, brush, centerX, bottom, format);
        }
    }

    public void Dispose()
    {
        graphics.Dispose();
        bitmap.Dispose();
    }
}