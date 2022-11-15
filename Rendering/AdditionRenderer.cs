using System.Drawing;
using CodeVideoMaker.Model;
using CodeVideoMaker.Output;

namespace CodeVideoMaker.Rendering
{
    class AdditionRenderer : ChangeRenderer
    {
        private Addition addition;
        
        public AdditionRenderer(Addition addition, EditorView editor, IOutput ffmpeg, double fps, double framesPerChar, double randomness)
            : base(editor, ffmpeg, fps, framesPerChar, randomness)
        {
            this.addition = addition;
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

            editor.MoveCursorTo(addition.FirstLineNumber, 0, AddFrame);

            for (int i = 0; i < addition.Lines.Length; ++i)
            {
                editor.AddLine(addition.FirstLineNumber + i, addition.Lines[i], addFrameForTyping);
            }

            editor.Blink(TimeSpan.FromSeconds(3.0), fps, AddFrame);
        }
    }
}