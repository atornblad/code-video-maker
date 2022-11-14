namespace CodeVideoMaker.Model;

class Edit : Change
{
    public EditLine[] Edits
    {
        get
        {
            return edits.ToArray();
        }
        set
        {
            edits = new List<EditLine>(value);
        }
    }

    private List<EditLine> edits = new List<EditLine>();
    public void Add(EditLine line)
    {
        edits.Add(line);
    }
}

class EditLine
{
    public string CommonStart { get; set; } = string.Empty;
    public string CommonEnd { get; set; } = string.Empty;
    public string Deleted { get; set; } = string.Empty;
    public string Added { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{CommonStart}({Deleted} -> {Added}){CommonEnd}";
    }
}