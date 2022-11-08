using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace CodeVideoMaker;

class FfmpegProcess : IDisposable
{
    private Process process;

    public FfmpegProcess(int fps)
    {
        Console.WriteLine("Starting ffmpeg...");
        process = new Process();
        process.StartInfo.FileName = @"..\ffmpeg\bin\ffmpeg.exe";
        process.StartInfo.Arguments = $"-framerate {fps} -f image2pipe -i - -c:v libx264 -r 60 -pix_fmt yuv420p -v fatal output.mp4";
        process.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.UseShellExecute = false;
        process.Start();
    }

    public void AddFrame(Bitmap bitmap)
    {
        Console.WriteLine("Adding a frame");
        bitmap.Save(process.StandardInput.BaseStream, ImageFormat.Png);
        process.StandardInput.BaseStream.Flush();
    }

    public void Dispose()
    {
        Console.WriteLine("Flushing video frames...");
        process.StandardInput.Close();
        process.StandardInput.Dispose();

        while (true)
        {
            if (process.WaitForExit(1000))
            {
                break;
            }
            Console.WriteLine("Waiting for ffmpeg to exit...");
        }

        process.Dispose();
    }
}