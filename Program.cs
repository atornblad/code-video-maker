using System.Drawing;

namespace CodeVideoMaker;

public static class Program
{
    public static void Main()
    {
        var image = new Bitmap(1920, 1080);
        var graphics = Graphics.FromImage(image);
        using (var ffmpeg = new FfmpegProcess(60))
        {
            for (int i = 0; i < 300; ++i)
            {
                graphics.Clear(Color.FromArgb(255, (i * 255 / 300), 128, 192));
                graphics.DrawString("Hello World!", new Font("Arial", 120), Brushes.Black, i, 500 + i);
                ffmpeg.AddFrame(image);
            }
        }
    }
}