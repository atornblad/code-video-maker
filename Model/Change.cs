namespace CodeVideoMaker.Model;

abstract class Change
{
    public int FirstLineNumber { get; set; } = 0;
    public string[] InitialLines { get; set; }= Array.Empty<string>();
}