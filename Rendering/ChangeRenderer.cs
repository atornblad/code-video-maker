using System.Drawing;
using CodeVideoMaker.Model;
using CodeVideoMaker.Output;

namespace CodeVideoMaker.Rendering
{
    abstract class ChangeRenderer
    {
        private IOutput ffmpeg;
        protected double fps;
        protected double framesPerChar;
        protected double randomness;
        protected EditorView editor;

        public ChangeRenderer(EditorView editor, IOutput ffmpeg, double fps, double framesPerChar, double randomness)
        {
            this.ffmpeg = ffmpeg;
            this.fps = fps;
            this.framesPerChar = framesPerChar;
            this.randomness = randomness;
            this.editor = editor;
        }

        public abstract void Render();

        protected void AddFrame(Bitmap bitmap)
        {
            ffmpeg.AddFrame(bitmap);
        }
    }
}