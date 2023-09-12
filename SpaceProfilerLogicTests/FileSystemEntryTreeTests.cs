using FluentAssertions;
using SpaceProfilerLogic;
using SpaceProfilerLogic.DirectoryWatcher;
using SpaceProfilerLogic.Tree;
using SpaceProfilerLogicTests.TestHelpers;

namespace SpaceProfilerLogicTests;

public class FileSystemEntryTreeTests
{
    private readonly string rootFullName = Path.GetFullPath("TestData");
    private FileSystemHelper fileSystemHelper = null!;

    [SetUp]
    public void SetUp()
    {
        fileSystemHelper = new FileSystemHelper(rootFullName);
    }
    
    [Test]
    public void AddFileRecursively()
    {
        CreateTreeOnDisk(rootFullName);
        
        var tree = new FileSystemEntryTree(rootFullName);
        var changed = tree.Apply(new Change($"{rootFullName}\\1\\11\\111f", ChangeType.Create));

        var file = new FileEntry($"{rootFullName}\\1\\11\\111f", 1000);
        var dir2 = new DirectoryEntry($"{rootFullName}\\1\\11", 1000)
        {
            Files = new[] { file }
        };
        var dir1 = new DirectoryEntry($"{rootFullName}\\1", 1000)
        {
            Subdirectories = new[] { dir2 }
        };
        var expectedRoot = new DirectoryEntry(rootFullName, 1000)
        {
            Subdirectories = new[] { dir1 }
        };

        tree.Root.Should().BeEquivalentTo(expectedRoot, o => o.IgnoringCyclicReferences());
        changed.Should().BeEquivalentTo(new FileSystemEntry[] { expectedRoot, dir1, dir2, file },
            o => o.IgnoringCyclicReferences());
    }
    
    [Test]
    public void AddEmptyFileRecursively()
    {
        CreateTreeOnDisk(rootFullName);
        
        var tree = new FileSystemEntryTree(rootFullName);
        tree.Apply(new Change($"{rootFullName}\\1", ChangeType.Create));
        
        var changed = tree.Apply(new Change($"{rootFullName}\\1\\11\\112f", ChangeType.Create));

        var file = new FileEntry($"{rootFullName}\\1\\11\\112f", 0);
        var dir2 = new DirectoryEntry($"{rootFullName}\\1\\11", 0)
        {
            Files = new[] { file }
        };
        var dir1 = new DirectoryEntry($"{rootFullName}\\1", 0)
        {
            Subdirectories = new[] { dir2 }
        };
        var expectedRoot = new DirectoryEntry(rootFullName, 0)
        {
            Subdirectories = new[] { dir1 }
        };

        tree.Root.Should().BeEquivalentTo(expectedRoot, o => o.IgnoringCyclicReferences());
        changed.Should().BeEquivalentTo(new FileSystemEntry[] { dir1, dir2, file },
            o => o.IgnoringCyclicReferences());
    }
    
    [Test]
    public void AddDirectoryRecursively()
    {
        CreateTreeOnDisk(rootFullName);
        
        var tree = new FileSystemEntryTree(rootFullName);
        var changed = tree.Apply(new Change($"{rootFullName}\\1\\11", ChangeType.Create));

        var dir2 = new DirectoryEntry($"{rootFullName}\\1\\11", 0);
        var dir1 = new DirectoryEntry($"{rootFullName}\\1", 0)
        {
            Subdirectories = new[] { dir2 }
        };
        var expectedRoot = new DirectoryEntry(rootFullName, 0)
        {
            Subdirectories = new[] { dir1 }
        };

        tree.Root.Should().BeEquivalentTo(expectedRoot, o => o.IgnoringCyclicReferences());
        changed.Should().BeEquivalentTo(new FileSystemEntry[] { expectedRoot, dir1, dir2 },
            o => o.IgnoringCyclicReferences());
    }
    
    /// <summary>
    /// Creates tree
    /// TestData
    ///     1f (filesize 1000)
    ///     2f (filesize 1000)
    ///     1
    ///         11f (filesize 1000)
    ///         12f (filesize 1000)
    ///         11
    ///             111f (filesize 1000)
    ///             112f (filesize 1000)
    ///     2
    /// </summary>
    /// <returns></returns>
    private void CreateTreeOnDisk(string root)
    {
        fileSystemHelper.CreateFiles(1000 , "1f", "2f");
        fileSystemHelper.CreateDirectoryWithFiles("1", 1000, "11f", "12f");
        fileSystemHelper.CreateDirectoryWithFiles("1\\11", 1000, "111f", "112f");
        fileSystemHelper.CreateFile("1\\11\\112f", 0);
        fileSystemHelper.CreateDirectory(@"2");
    }
    
    [TearDown]
    public void TearDown()
    {
        fileSystemHelper.Clear();
    }
}