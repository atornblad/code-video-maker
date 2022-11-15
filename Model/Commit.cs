using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CodeVideoMaker.Model;

class Commit
{
    public string Hash { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.MinValue;
    public string Author { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public IEnumerable<FileCommit> Changes => changes;
    private readonly ICollection<FileCommit> changes = new List<FileCommit>();

    public static IEnumerable<Commit> CreateCommitsFromDiff(string[] lines)
    {
        var files = new Dictionary<string, CodeFile>();
        var commits = new List<Commit>();
        Commit? commit = null;
        FileCommit? fileCommit = null;
        Change? change = null;

        int i = 0;
        int? sourceRow = null;
        int? targetRow = null;

        while (i < lines.Length)
        {
            if (lines[i].StartsWith("commit"))
            {
                commit = new Commit
                {
                    Hash = lines[i].Split(' ')[1]
                };
                commits.Add(commit);
                fileCommit = null;
                change = null;
                Debug.WriteLine($"New commit. Hash: {commit.Hash}");
                ++i;
            }
            else if (lines[i].StartsWith("Author:"))
            {
                commit!.Author = lines[i].Substring(8);
                Debug.WriteLine($"Author: {commit.Author}");
                ++i;
            }
            else if (lines[i].StartsWith("Date:"))
            {
                // Parse the date on the format Mon Nov 14 13:45:44 2022 +0100 using standard .NET date parsing
                string date = lines[i].Substring(8);
                commit!.Date = DateTime.ParseExact(date, "ddd MMM dd HH:mm:ss yyyy zzz", CultureInfo.InvariantCulture);
                Debug.WriteLine($"Date: {commit.Date}");
                ++i;
            }
            else if (lines[i].StartsWith("diff --git"))
            {
                ++i;
            }
            else if (lines[i].StartsWith("index"))
            {
                ++i;
            }
            else if (lines[i].StartsWith("+++"))
            {
                string filename = lines[i].Substring(6);
                if (files.TryGetValue(filename, out var file))
                {
                    Debug.WriteLine($"Existing file: {filename}");
                }
                else
                {
                    file = new CodeFile { Filename = filename };
                    files.Add(filename, file);
                    Debug.WriteLine($"New file: {filename}");
                }
                fileCommit = new FileCommit { File = file };
                commit!.changes.Add(fileCommit);
                change = null;
                ++i;
            }
            else if (lines[i].StartsWith("---"))
            {
                ++i;
            }
            else if (lines[i].StartsWith("    ") && fileCommit == null)
            {
                commit!.Message = lines[i].Substring(4);
                Debug.WriteLine($"Message: {commit!.Message}");
                ++i;
            }
            else if (lines[i].StartsWith("@@"))
            {
                Regex regex = new Regex(@"@@ -(\d+),(\d+) \+(\d+),(\d+) @@");
                Match match = regex.Match(lines[i]);
                sourceRow = int.Parse(match.Groups[1].Value);
                targetRow = int.Parse(match.Groups[3].Value);
                Debug.WriteLine($"Target row: {targetRow}");

                ++i;
            }
            else if (lines[i].StartsWith("+"))
            {
                if (change is Addition addition)
                {
                    addition.Add(lines[i].Substring(1));
                }
                else
                {
                    addition = new Addition { FirstLineNumber = targetRow!.Value - 1 };
                    fileCommit!.Changes.Add(addition);
                    change = addition;
                    addition.Add(lines[i].Substring(1));
                    Debug.WriteLine($"New addition");
                }
                
                ++targetRow;
                ++i;
            }
            else if (lines[i].StartsWith("-"))
            {
                if (change is Deletion deletion)
                {
                    deletion.Add(lines[i].Substring(1));
                }
                else
                {
                    deletion = new Deletion { FirstLineNumber = targetRow!.Value - 1 };
                    fileCommit!.Changes.Add(deletion);
                    change = deletion;
                    deletion.Add(lines[i].Substring(1));
                    Debug.WriteLine($"New deletion");
                }
                
                //++targetRow;
                ++i;
            }
            else if (lines[i].StartsWith(" "))
            {
                ++targetRow;
                ++i;
            }
            else
            {
                ++i;
            }
        }

        Commit.Cleanup(commits);

        return commits;
    }

    public static void Cleanup(IEnumerable<Commit> commits)
    {
        foreach (var commit in commits)
        {
            commit.Cleanup();
        }
    }

    public void Cleanup()
    {
        foreach (var fileCommit in this.changes)
        {
            fileCommit.Cleanup();
        }
    }
}
