using System.Drawing;
using CodeVideoMaker.Model;

namespace CodeVideoMaker.Rendering
{
    class DeletionRenderer : ChangeRenderer
    {
        private Deletion deletion;
        
        public DeletionRenderer(Deletion deletion, EditorView editor, FfmpegProcess ffmpeg, double fps, double framesPerChar, double randomness)
            : base(editor, ffmpeg, fps, framesPerChar, randomness)
        {
            this.deletion = deletion;
        }

        public override void Render()
        {
            var random = new Random();
            double time = 0.0;
            double nextTyping = framesPerChar;

            Action<Bitmap> addFrameForTyping = (bitmap) => 
            {
                bool addedFrame = false;
                while (nextTyping > time + 0.5)
                {
                    AddFrame(bitmap);
                    addedFrame = true;
                    time += 1.0;
                }
                double addTime = framesPerChar * (1.0 + randomness * (random.NextDouble() - 0.5));
                nextTyping += addTime;
                if (!addedFrame)
                {
                    AddFrame(bitmap);
                }
            };

            editor.MoveCursorTo(deletion.FirstLineNumber, 0, AddFrame);

            editor.Blink(TimeSpan.FromSeconds(2.0), fps, AddFrame);

            for (int i = 0; i < deletion.Lines.Length; ++i)
            {
                editor.DeleteLine(deletion.FirstLineNumber, addFrameForTyping);
                editor.Blink(TimeSpan.FromSeconds(0.15), fps, AddFrame);
            }

            editor.Blink(TimeSpan.FromSeconds(2.0), fps, AddFrame);
        }
    }
}