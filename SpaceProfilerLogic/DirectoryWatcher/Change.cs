namespace SpaceProfilerLogic.DirectoryWatcher;

public class Change
{
    public string FullName { get; }

    public ChangeType Type { get; }

    public Change(FileSystemEventArgs e)
    {
        FullName = e.FullPath;
        Type = ToChangeType(e.ChangeType);
    }
    
    public Change(string fullName, ChangeType type)
    {
        FullName = fullName;
        Type = type;
    }

    public bool Merge(Change newer, out Change? result)
    {
        if (newer.Type == Type)
        {
            result = newer;
            return true;
        }

        if (newer.Type == ChangeType.Delete)
        {
            result = Type == ChangeType.Create ? null : newer;
            return true;
        }

        if (Type == ChangeType.Create && newer.Type == ChangeType.Update)
        {
            result = this;
            return true;
        }

        result = null;
        return false;
    }
    
    private ChangeType ToChangeType(WatcherChangeTypes eChangeType)
    {
        switch (eChangeType)
        {
            case WatcherChangeTypes.Created:
                return ChangeType.Create;
            case WatcherChangeTypes.Deleted:
                return ChangeType.Delete;
            case WatcherChangeTypes.Changed:
                return ChangeType.Update;
            default:
                throw new ArgumentOutOfRangeException(nameof(eChangeType), eChangeType, null);
        }
    }

    public override string ToString()
    {
        return $"{Type:G} {FullName}";
    }
}