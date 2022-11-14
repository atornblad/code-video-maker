using System.Drawing;
using CodeVideoMaker.Model;

namespace CodeVideoMaker.Rendering
{
    abstract class ChangeRenderer
    {
        private FfmpegProcess ffmpeg;
        protected double fps;
        protected double framesPerChar;
        protected double randomness;
        protected EditorView editor;

        public ChangeRenderer(EditorView editor, FfmpegProcess ffmpeg, double fps, double framesPerChar, double randomness)
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