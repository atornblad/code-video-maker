using System.Diagnostics;
using System.Drawing;
using CodeVideoMaker.Model;
using CodeVideoMaker.Output;
using CodeVideoMaker.Rendering;
using CodeVideoMaker.Rendering.CutScenes;

namespace CodeVideoMaker;

public static class Program
{
    public static void Main(string[] args)
    {
        int? width = null;
        int? height = null;
        int? fps = null;
        string? output = null;
        int? maxCommitCount = null;
        string? repo = null;
        bool opening = false;
        string? title = null;
        string? subtitle = null;
        int? backgroundSeconds = null;

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
            else if (args[i] == "-c" || args[i] == "--commits")
            {
                maxCommitCount = int.Parse(args[++i]);
            }
            else if (args[i] == "-o" || args[i] == "--output")
            {
                output = args[++i];
            }
            else if (args[i] == "-r" || args[i] == "--repo")
            {
                repo = args[++i];
            }
            else if (args[i] == "-O" || args[i] == "--opening")
            {
                opening = true;
                title = args[++i];
                subtitle = args[++i];
            }
            else if (args[i] == "-b" || args[i] == "--background")
            {
                backgroundSeconds = int.Parse(args[++i]);
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
        if (output == null)
        {
            output = "output.mp4";
        }

        using var ffmpeg = new FfmpegProcess(fps.Value, output);
        if (opening)
        {
            RenderOpening(width.Value, height!.Value, fps.Value, title!, subtitle!, null, ffmpeg);
        }
        if (repo != null)
        {
            RenderGitLog(width.Value, height!.Value, fps.Value, repo, maxCommitCount, ffmpeg);
        }
        if (backgroundSeconds.HasValue)
        {
            RenderOpening(width.Value, height!.Value, fps.Value, string.Empty, string.Empty, backgroundSeconds.Value, ffmpeg);
        }
    }

    static void RenderOpening(int width, int height, int fps, string title, string subtitle, int? background, IOutput output)
    {
        using var renderer = new IntroRenderer(output, title, subtitle, fps, width, height);
        if (background.HasValue)
        {
            renderer.RenderBackground(TimeSpan.FromSeconds(background.Value));
        }
        else
        {
            renderer.RenderTimeline();
        }
    }

    static void RenderGitLog(int width, int height, int fps, string repoOrDiffPath, int? maxCommitCount, IOutput output)
    {
        System.Console.WriteLine($"Rendering git log from {repoOrDiffPath}");
        string fullPath = Path.Combine(Environment.CurrentDirectory, repoOrDiffPath);
        if (File.Exists(fullPath))
        {
            RenderGitLogFromDiff(width, height, fps, fullPath, maxCommitCount, output);
        }
        else
        {
            RenderGitLogFromRepo(width, height, fps, fullPath, maxCommitCount, output);
        }
    }

    private static void RenderGitLogFromRepo(int width, int height, int fps, string fullPath, int? maxCommitCount, IOutput output)
    {
        throw new NotImplementedException();
    }

    private static void RenderGitLogFromDiff(int width, int height, int fps, string fullPath, int? maxCommitCount, IOutput output)
    {
        string[] lines = File.ReadAllLines(fullPath);
        IEnumerable<Commit> commits = Commit.CreateCommitsFromDiff(lines);

        var coders = new Dictionary<string, Coder>();

        int titleHeight = height / 20;
        int editorHeight = height - titleHeight;
        using var ide = new IdeOutput(width, height, titleHeight, output);
        string currentFilename = string.Empty;
        Coder? coder = null;

        var commitList = maxCommitCount.HasValue ? commits.Take(maxCommitCount.Value) : commits;
        foreach (var commit in commitList)
        {
            Console.WriteLine($"Rendering commit {commit.Message}");
            // TODO: Render a splash screen with the commit message, maybe?
            foreach (var fileCommit in commit.Changes)
            {
                string nextFilename = fileCommit.File!.Filename;
                if (nextFilename != currentFilename)
                {
                    if (coder == null)
                    {
                        coder = new Coder(width, editorHeight, fps, "New file", Array.Empty<string>());
                    }

                    coder.RenderBlink(ide, TimeSpan.FromSeconds(2.0));

                    if (coders.TryGetValue(nextFilename, out var nextCoder))
                    {
                        Debug.WriteLine($"Found coder for file {nextFilename}");
                        coder.RenderSwitchToFile(ide, currentFilename, nextFilename, coders.Keys.ToArray());
                        coder = nextCoder;
                    }
                    else
                    {
                        Debug.WriteLine($"Creating new coder for file {nextFilename}");
                        coder.RenderNewFile(ide, nextFilename);
                        coder = new Coder(width, editorHeight, fps, nextFilename, Array.Empty<string>());
                        coders.Add(nextFilename, coder);
                    }
                }
                int cpm = nextFilename.EndsWith(".html") ? 1500 : 1000;
                ide.SetFilename(nextFilename);
                if (nextFilename != currentFilename)
                {
                    coder!.RenderBlink(ide, TimeSpan.FromSeconds(2.0));
                }
                currentFilename = nextFilename;
                coder!.Render(fileCommit, ide, cpm, 1.5);
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
