namespace SpaceProfilerLogic.DirectoryWatcher;

public class ChangesMerger
{
    private readonly Dictionary<string, List<Change>> changes = new();

    public List<Change> Merged => changes.Keys.SelectMany(key => changes[key]).ToList();

    public void Push(Change change)
    {
        if (!changes.ContainsKey(change.FullName))
        {
            changes.Add(change.FullName, new List<Change>() { change });
            return;
        }

        var last = changes[change.FullName].LastOrDefault();
        if (last != null && last.Merge(change, out var merged))
        {
            changes[change.FullName].RemoveAt(changes[change.FullName].Count - 1);
            if (merged != null) changes[change.FullName].Add(merged);
        }
        else
        {
            changes[change.FullName].Add(change);
        }
    }
}