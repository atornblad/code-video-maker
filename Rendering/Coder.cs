using System.Drawing;
using CodeVideoMaker.Model;
using CodeVideoMaker.Output;

namespace CodeVideoMaker.Rendering;

class Coder : IDisposable
{
    private Bitmap bitmap;
    private Graphics graphics;
    private EditorView editor;
    private int fps;

    public Coder(int width, int height, int fps, string filename, string[] initialLines)
    {
        bitmap = new Bitmap(width, height);
        graphics = Graphics.FromImage(bitmap);
        editor = new EditorView(graphics, bitmap,filename, initialLines);
        this.fps = fps;
    }

    public Coder(int width, int height, int fps, string filename) : this(width, height, fps, filename, Array.Empty<string>())
    {

    }


    public void RenderBlink(IOutput output, TimeSpan time)
    {
        editor.Blink(time, fps, output.AddFrame);
    }

    /// <summary>
    /// Renders an entire file commit.
    /// </summary>
    /// <param name="commit">The commit to render.</param>
    /// <param name="ffmpeg">The FfmpegProcess to render to.</param>
    /// <param name="cpm">Average characters per minute.</param>
    /// <param name="randomness">The randomness of the typing.</param>
    public void Render(FileCommit fileCommit, IOutput output, double cpm, double randomness)
    {
        double secondsPerChar = 60.0 / cpm;
        double secondsPerFrame = 1.0 / fps;
        double framesPerChar = secondsPerChar / secondsPerFrame;

        foreach (var change in fileCommit.Changes)
        {
            RenderChange(change, output, framesPerChar, randomness);
        }
    }

    private void RenderChange(Change change, IOutput ffmpeg, double framesPerChar, double randomness)
    {
        ChangeRenderer changeRenderer = change switch
        {
            Addition addition => new AdditionRenderer(addition, editor, ffmpeg, fps, framesPerChar, randomness),
            Deletion deletion => new DeletionRenderer(deletion, editor, ffmpeg, fps, framesPerChar, randomness),
            Edit edit => new EditRenderer(edit, editor, ffmpeg, fps, framesPerChar, randomness),
            _ => throw new NotImplementedException()
        };
        changeRenderer.Render();
    }

    #region IDisposable
    private bool disposedValue;
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                graphics.Dispose();
                bitmap.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}