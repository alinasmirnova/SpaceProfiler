using System.Collections.Concurrent;
using SpaceProfilerLogic.Tree;

namespace SpaceProfilerLogicTests;

public class TestFileSystemEntryTree : FileSystemEntryTree
{
    public TestFileSystemEntryTree(string fullRootName) : base(fullRootName)
    {
    }

    public ConcurrentDictionary<string, FileSystemEntry> GetNodes => nodes;
}