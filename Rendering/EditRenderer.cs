using System.Drawing;
using CodeVideoMaker.Model;

namespace CodeVideoMaker.Rendering;

class EditRenderer : ChangeRenderer
{
    private Edit edit;

    public EditRenderer(Edit edit, EditorView editor, FfmpegProcess ffmpeg, double fps, double framesPerChar, double randomness)
    : base(editor, ffmpeg, fps, framesPerChar, randomness)
    {
        this.edit = edit;
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

            for (int i = 0; i < edit.Edits.Length; ++i)
            {
                var lineEdit = edit.Edits[i];
                editor.MoveCursorTo(edit.FirstLineNumber + i, lineEdit.CommonStart.Length + lineEdit.Deleted.Length, AddFrame);
                editor.Blink(TimeSpan.FromSeconds(2.0), fps, AddFrame);
                for (int j = 0; j < lineEdit.Deleted.Length; ++j)
                {
                    editor.Backspace(AddFrame);
                }
                for (int j = 0; j < lineEdit.Added.Length; ++j)
                {
                    editor.AddChar(lineEdit.Added[j], addFrameForTyping);
                }
                editor.Blink(TimeSpan.FromSeconds(2.0), fps, AddFrame);
            }

        }
}