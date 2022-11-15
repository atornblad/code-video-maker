using System.Diagnostics;
using System.Drawing;
using CodeVideoMaker.Model;
using CodeVideoMaker.Output;
using CodeVideoMaker.Rendering;

namespace CodeVideoMaker;

public static class Program
{
    public static void Main(string[] args)
    {
        int? width = null;
        int? height = null;
        int? fps = null;
        string? output = null;

        for (int i = 0; i < args.Length; ++i)
        {
            if (args[i] == "-w" || args[i] == "--width")
            {
                width = int.Parse(args[++i]);
            }
            else if (args[i] == "-h" || args[i] == "--height")
            {
                height = int.Parse(args[++i]);
            }
            else if (args[i] == "-f" || args[i] == "--fps")
            {
                fps = int.Parse(args[++i]);
            }
            else if (args[i] == "-o" || args[i] == "--output")
            {
                output = args[++i];
            }
        }

        if (width == null && height == null)
        {
            width = 512;
            height = 288;
        }
        else if (width == null)
        {
            width = height! * 16 / 9;
        }
        else if (height == null)
        {
            height = width! * 9 / 16;
        }
        if (fps == null)
        {
            fps = 30;
        }

        /*var image = new Bitmap(1920, 1080);
        var graphics = Graphics.FromImage(image);
        using (var ffmpeg = new FfmpegProcess())
        {
            for (int i = 0; i < 300; ++i)
            {
                graphics.Clear(Color.FromArgb(255, (i * 255 / 300), 128, 192));
                graphics.DrawString("Hello World!", new Font("Arial", 200), Brushes.Black, (float)(100 + 200 * Math.Sin(i * 0.04)), (float)(200 - 200 * Math.Cos(i * 0.04)));
                ffmpeg.AddFrame(image);
            }
        }*/
        //RenderNewAddedFile1(width.Value, height!.Value, fps.Value);
        RenderGitLog(width.Value, height.Value, fps.Value, "git-log-p-output.txt", output);
    }

    static void RenderGitLog(int width, int height, int fps, string repoOrDiffPath, string videoFilename)
    {
        System.Console.WriteLine($"Rendering git log from {repoOrDiffPath} to {videoFilename} at {width}x{height} at {fps} fps");
        string fullPath = Path.Combine(Environment.CurrentDirectory, repoOrDiffPath);
        if (File.Exists(fullPath))
        {
            RenderGitLogFromDiff(width, height, fps, fullPath, videoFilename);
        }
        else
        {
            RenderGitLogFromRepo(width, height, fps, fullPath, videoFilename);
        }
    }

    private static void RenderGitLogFromRepo(int width, int height, int fps, string fullPath, string videoFilename)
    {
        throw new NotImplementedException();
    }

    private static void RenderGitLogFromDiff(int width, int height, int fps, string fullPath, string videoFilename)
    {
        string[] lines = File.ReadAllLines(fullPath);
        IEnumerable<Commit> commits = Commit.CreateCommitsFromDiff(lines);

        var coders = new Dictionary<string, Coder>();

        int titleHeight = height / 20;
        int editorHeight = height - titleHeight;
        using var ffmpeg = new FfmpegProcess(fps, videoFilename);
        using var ide = new IdeOutput(width, height, titleHeight, ffmpeg);
        string currentFilename = string.Empty;

        foreach (var commit in commits)
        {
            // TODO: Render a splash screen with the commit message, maybe?
            foreach (var fileCommit in commit.Changes)
            {
                string nextFilename = fileCommit.File!.Filename;
                if (coders.TryGetValue(nextFilename, out var coder))
                {
                    Debug.WriteLine($"Found coder for file {nextFilename}");
                }
                else
                {
                    Debug.WriteLine($"Creating new coder for file {nextFilename}");
                    coder = new Coder(width, editorHeight, fps, nextFilename, Array.Empty<string>());
                    coders.Add(nextFilename, coder);
                }
                int cpm = nextFilename.EndsWith(".html") ? 1500 : 1000;
                ide.SetFilename(nextFilename);
                if (nextFilename != currentFilename)
                {
                    coder.RenderBlink(ide, TimeSpan.FromSeconds(2.0));
                }
                currentFilename = nextFilename;
                coder.Render(fileCommit, ide, cpm, 1.5);
            }
        }
    }

    static void RenderNewAddedFile1(int width, int height, int fps)
    {
        var file = new CodeFile
        {
            Filename = "player.js"
        };

        var commit1 = new FileCommit { File = file };

        var addition1 = new Addition()
        {
            FirstLineNumber = 0,
            Lines = new []
            {
                "document.addEventListener('DOMContentLoaded', () => {",
                "    console.log('DOMContentLoaded!');",
                "});",
                "",
                "window.addEventListener('load', () => {",
                "    console.log('load!');",
                "});",
                "",
                "const x = 1, y = 2, z = 3;",
                "const a = 'Hi there!', b = 'Hello, world!';",
                "",
                "const add = (a, b) => a + b;",
                "const sub = (a, b) => a - b;",
                "const mul = (a, b) => a * b;",
                "const div = (a, b) => a / b;"             
            }
        };

        commit1.Changes.Add(addition1);
        
        var fileAfterCommit1 = new CodeFile
        {
            Filename = "player.js",
            InitialLines = addition1.Lines
        };
        var commit2 = new FileCommit { File = fileAfterCommit1 };
        var addition2 = new Addition()
        {
            FirstLineNumber = 8,
            InitialLines = fileAfterCommit1.InitialLines,       // TODO: Get this from the initial file contents instead!
            Lines = new [] {
                "window.addEventListener('click', (e) => {",
                "    e.preventDefault();",
                "    console.log('click!');",
                "});",
                ""
            }
        };
        commit2.Changes.Add(addition2);

        using (var ffmpeg = new FfmpegProcess(fps, "new-added-file1.mp4"))
        using (var coder = new Coder(width, height, fps, "player.js"))
        {
            coder.Render(commit1, ffmpeg, 500, 0.8);
            coder.Render(commit2, ffmpeg, 500, 0.8);
        }
    }
}
