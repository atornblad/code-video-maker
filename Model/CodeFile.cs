namespace CodeVideoMaker.Model;

class CodeFile
{
    public string Filename { get; set; } = string.Empty;
    public string[] InitialLines { get; set; } = Array.Empty<string>();

    public ICollection<FileCommit> Commits => commits;
    private readonly List<FileCommit> commits = new List<FileCommit>();
}
