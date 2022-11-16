using System.Diagnostics;

namespace CodeVideoMaker.Model;

// The FileCommit class doesn't reflect a real Git commit, but rather the changes
// made to a single file in a single commit.

class FileCommit
{
    public CodeFile? File { get; set; } = null;
    public ICollection<Change> Changes => changes;
    private readonly List<Change> changes = new List<Change>();

    public void Cleanup()
    {
        JoinDeletionsToAdditions();
        JoinAdditions();
        JoinDeletions();
    }

    private void JoinDeletionsToAdditions()
    {
        for (int i = 0; i < changes.Count - 1; ++i)
        {
            if (changes[i] is Deletion deletion && changes[i + 1] is Addition addition)
            {
                if (deletion.FirstLineNumber != addition.FirstLineNumber) continue;
                // Delete exactly one line and add one or more line
                if (deletion.Lines.Length != 1) continue;
                string deletedLine = deletion.Lines[0];
                string addedLine = addition.Lines[0];
                Debug.WriteLine($"Checking if {deletedLine} can be replaced with {addedLine}");
                // Number of letters common at the start of both lines
                int commonStart = 0;
                while (commonStart < deletedLine.Length && commonStart < addedLine.Length && deletedLine[commonStart] == addedLine[commonStart])
                {
                    ++commonStart;
                }
                // Number of letters common at the end of both lines
                int commonEnd = 0;
                while (commonEnd < deletedLine.Length - commonStart && commonEnd < addedLine.Length - commonStart && deletedLine[deletedLine.Length - 1 - commonEnd] == addedLine[addedLine.Length - 1 - commonEnd])
                {
                    ++commonEnd;
                }
                // If the common start and end is at least half of the shortest line, we can join the deletion and addition
                if (commonStart + commonEnd >= Math.Min(deletedLine.Length, addedLine.Length) / 2)
                {
                    // Join the deletion and first line of the addition
                    var edit = new Edit
                    {
                        FirstLineNumber = deletion.FirstLineNumber,
                        InitialLines = deletion.InitialLines,
                        Edits = new []
                        {
                            new EditLine
                            {
                                CommonStart = deletedLine.Substring(0, commonStart),
                                CommonEnd = deletedLine.Substring(deletedLine.Length - commonEnd),
                                Deleted = deletedLine.Substring(commonStart, deletedLine.Length - commonStart - commonEnd),
                                Added = addedLine.Substring(commonStart, addedLine.Length - commonStart - commonEnd)
                            }
                        }
                    };
                    // Remove the first line from the addition
                    addition.Lines = addition.Lines.Skip(1).ToArray();
                    addition.FirstLineNumber++;
                    Debug.WriteLine($"Joined deletion and addition at line: {edit.Edits[0]}");
                    changes.RemoveAt(i);
                    changes.Insert(i, edit);
                    // If the addition is now empty, remove it
                    if (addition.Lines.Length == 0)
                    {
                        changes.RemoveAt(i + 1);
                    }
                    else
                    {
                        changes[i] = edit;
                    }
                }

            }
        }
    }

    private void JoinAdditions()
    {
        for (int i = 0; i < changes.Count - 1; ++i)
        {
            if (changes[i] is Addition addition && changes[i + 1] is Addition nextAddition)
            {
                if (addition.FirstLineNumber + addition.Lines.Length == nextAddition.FirstLineNumber)
                {
                    addition.Lines = addition.Lines.Concat(nextAddition.Lines).ToArray();
                    changes.RemoveAt(i + 1);
                    --i;
                }
            }
        }
    }

    private void JoinDeletions()
    {
        for (int i = 0; i < changes.Count - 1; ++i)
        {
            if (changes[i] is Deletion deletion && changes[i + 1] is Deletion nextDeletion)
            {
                if (deletion.FirstLineNumber + deletion.Lines.Length == nextDeletion.FirstLineNumber)
                {
                    deletion.Lines = deletion.Lines.Concat(nextDeletion.Lines).ToArray();
                    changes.RemoveAt(i + 1);
                    --i;
                }
            }
        }
    }
}