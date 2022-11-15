using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace CodeVideoMaker.Output;

class FfmpegProcess : IOutput
{
    private Process process;
    private const int DEFAULT_FPS = 60;
    private const string DEFAULT_FILENAME = "output.mp4";
    private TimeSpan rendered = TimeSpan.Zero;
    private TimeSpan nextOutput = TimeSpan.FromSeconds(0.0);
    private DateTime? lastOutput = null;
    private TimeSpan timePerFrame;

    public FfmpegProcess() : this(DEFAULT_FPS, DEFAULT_FPS, DEFAULT_FILENAME) { }
    public FfmpegProcess(string filename) : this(DEFAULT_FPS, DEFAULT_FPS, filename) { }
    public FfmpegProcess(int fps) : this(fps, fps, DEFAULT_FILENAME) { }
    public FfmpegProcess(int fpsIn, int fpsOut) : this(fpsIn, fpsOut, DEFAULT_FILENAME) { }
    public FfmpegProcess(int fps, string filename) : this(fps, fps, filename) { }
    public FfmpegProcess(int fpsIn, int fpsOut, string filename)
    {
        if (File.Exists(filename))
        {
            File.Delete(filename);
        }

        timePerFrame = TimeSpan.FromSeconds(1.0 / fpsIn);
        
        Console.WriteLine("Starting ffmpeg...");
        process = new Process();
        process.StartInfo.FileName = @"..\ffmpeg\bin\ffmpeg.exe";
        process.StartInfo.Arguments = $"-framerate {fpsIn} -f image2pipe -i - -c:v libx264 -r {fpsOut} -pix_fmt yuv420p -v fatal {filename}";
        process.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.UseShellExecute = false;
        process.Start();
    }

    public void AddFrame(Bitmap bitmap)
    {
        bitmap.Save(process.StandardInput.BaseStream, ImageFormat.Png);
        process.StandardInput.BaseStream.Flush();
        rendered += timePerFrame;
        if (rendered > nextOutput)
        {
            Console.WriteLine($"Rendered {rendered.TotalSeconds:0.0} seconds");
            var now = DateTime.Now;
            if (lastOutput.HasValue)
            {
                var diff = now - lastOutput.Value;
                Console.WriteLine($"Current speed ratio: {1000.0 / diff.TotalSeconds:0.0} %");
            }
            lastOutput = now;
            nextOutput += TimeSpan.FromSeconds(10.0);
        }
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