namespace CodeVideoMaker.Model;

class Addition : Change
{
    public string[] Lines
    {
        get
        {
            return lines.ToArray();
        }
        set
        {
            lines = new List<string>(value);
        }
    }

    private List<string> lines = new List<string>();
    public void Add(string line)
    {
        lines.Add(line);
    }
}
