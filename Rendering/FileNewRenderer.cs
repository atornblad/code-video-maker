using System.Drawing;
using CodeVideoMaker.Output;

namespace CodeVideoMaker.Rendering;

class FileNewRenderer : IDisposable
{
    private string filename;
    private string[] filenames;
    private IOutput output;
    private int fps;
    private Bitmap bitmap;
    private Graphics graphics;


    public FileNewRenderer(string filename, int width, int height, IOutput output, int fps)
    {
        this.filename = filename;
        this.output = output;
        this.fps = fps;
        this.bitmap = new Bitmap(width, height);
        this.graphics = Graphics.FromImage(bitmap);
    }

    public void Render()
    {
        using var dialogBox = new DialogBoxRenderer(bitmap.Width, bitmap.Height, "Create new file");

        var input = new TextInputElement();
        dialogBox.Add(new LabelElement("Filename:"), 0, 0, 8, 1);
        dialogBox.Add(input, 8, 0, 16, 1);
        dialogBox.Add(new ButtonElement("OK"), 12, 1, 6, 1);
        dialogBox.Add(new ButtonElement("Cancel"), 18, 1, 6, 1);

        dialogBox.Render(graphics, fps, TimeSpan.FromSeconds(1.0), () => output.AddFrame(bitmap));
        for (int i = 0; i < filename.Length; ++i)
        {
            input.Text = filename.Substring(0, i + 1);
            dialogBox.Render(graphics, fps, TimeSpan.FromSeconds(0.08), () => output.AddFrame(bitmap));
        }
        dialogBox.Render(graphics, fps, TimeSpan.FromSeconds(1.0), () => output.AddFrame(bitmap));
    }

    public void Dispose()
    {
        graphics.Dispose();
        bitmap.Dispose();
    }
}