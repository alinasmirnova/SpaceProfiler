using FluentAssertions;
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
        
        var tree = new TestFileSystemEntryTree(rootFullName);
        var changed = tree.SynchronizeWithFileSystem($"{rootFullName}\\1\\11\\111f");

        var file111 = new FileEntry($"{rootFullName}\\1\\11\\111f", 1000);
        var file112 = new FileEntry($"{rootFullName}\\1\\11\\112f", 1000);
        var dir11 = new DirectoryEntry($"{rootFullName}\\1\\11", 2000)
        {
            Files = new[] { file111, file112 }
        };
        
        var file11 = new FileEntry($"{rootFullName}\\1\\11f", 1000);
        var file12 = new FileEntry($"{rootFullName}\\1\\12f", 1000);
        var dir1 = new DirectoryEntry($"{rootFullName}\\1", 4000)
        {
            Subdirectories = new[] { dir11 },
            Files = new[] { file11, file12 }
        };
        var file1 = new FileEntry($"{rootFullName}\\1f", 1000);
        var file2 = new FileEntry($"{rootFullName}\\2f", 1000);
        var expectedRoot = new DirectoryEntry(rootFullName, 6000)
        {
            Subdirectories = new[] { dir1 },
            Files = new[] { file1, file2 }
        };

        tree.Root.Should().BeEquivalentTo(expectedRoot, o => o.IgnoringCyclicReferences());
        changed.Should().BeEquivalentTo(new FileSystemEntry[] { file111, file112, dir11, dir1, file11, file12, expectedRoot },
            o => o.IgnoringCyclicReferences());
        tree.GetNodes.Should().BeEquivalentTo(CreateExpectedNodes(expectedRoot, file1, file2, dir1, file11, file12, dir11, file111, file112), o => o.IgnoringCyclicReferences());
    }

    [Test]
    public void AddEmptyFileRecursively()
    {
        CreateTreeOnDisk(rootFullName);
        
        var tree = new TestFileSystemEntryTree(rootFullName);
        tree.SynchronizeWithFileSystem($"{rootFullName}\\1");
        fileSystemHelper.CreateFile("1\\11\\113f", 0);
        
        var changed = tree.SynchronizeWithFileSystem($"{rootFullName}\\1\\11\\113f");

        var file111 = new FileEntry($"{rootFullName}\\1\\11\\111f", 1000);
        var file112 = new FileEntry($"{rootFullName}\\1\\11\\112f", 1000);
        var file113 = new FileEntry($"{rootFullName}\\1\\11\\113f", 0);
        var dir11 = new DirectoryEntry($"{rootFullName}\\1\\11", 2000)
        {
            Files = new[] { file111, file112, file113 }
        };
        
        var file11 = new FileEntry($"{rootFullName}\\1\\11f", 1000);
        var file12 = new FileEntry($"{rootFullName}\\1\\12f", 1000);
        var dir1 = new DirectoryEntry($"{rootFullName}\\1", 4000)
        {
            Subdirectories = new[] { dir11 },
            Files = new[] { file11, file12 }
        };
        var file1 = new FileEntry($"{rootFullName}\\1f", 1000);
        var file2 = new FileEntry($"{rootFullName}\\2f", 1000);
        var expectedRoot = new DirectoryEntry(rootFullName, 6000)
        {
            Subdirectories = new[] { dir1 },
            Files = new[] { file1, file2 }
        };

        tree.Root.Should().BeEquivalentTo(expectedRoot, o => o.IgnoringCyclicReferences());
        changed.Should().BeEquivalentTo(new FileSystemEntry[] { file113, file111, file112, dir11, dir1, expectedRoot },
            o => o.IgnoringCyclicReferences());
        tree.GetNodes.Should().BeEquivalentTo(CreateExpectedNodes(expectedRoot, file1, file2, dir1, file11, file12, dir11, file111, file112, file113), o => o.IgnoringCyclicReferences());
    }

    [Test]
    public void AddDirectoryRecursively()
    {
        CreateTreeOnDisk(rootFullName);
        
        var tree = new TestFileSystemEntryTree(rootFullName);
        var changed = tree.SynchronizeWithFileSystem($"{rootFullName}\\1\\11");

        var file111 = new FileEntry($"{rootFullName}\\1\\11\\111f", 1000);
        var file112 = new FileEntry($"{rootFullName}\\1\\11\\112f", 1000);
        var dir11 = new DirectoryEntry($"{rootFullName}\\1\\11", 2000)
        {
            Files = new[] { file111, file112 }
        };
        
        var file11 = new FileEntry($"{rootFullName}\\1\\11f", 1000);
        var file12 = new FileEntry($"{rootFullName}\\1\\12f", 1000);
        var dir1 = new DirectoryEntry($"{rootFullName}\\1", 4000)
        {
            Subdirectories = new[] { dir11 },
            Files = new[] { file11, file12 }
        };
        var file1 = new FileEntry($"{rootFullName}\\1f", 1000);
        var file2 = new FileEntry($"{rootFullName}\\2f", 1000);
        var expectedRoot = new DirectoryEntry(rootFullName, 6000)
        {
            Subdirectories = new[] { dir1 },
            Files = new[] { file1, file2 }
        };
        tree.Root.Should().BeEquivalentTo(expectedRoot, o => o.IgnoringCyclicReferences());
        changed.Should().BeEquivalentTo(new FileSystemEntry[] { file111, file112, dir11, dir1, file11, file12, expectedRoot },
            o => o.IgnoringCyclicReferences());
        tree.GetNodes.Should().BeEquivalentTo(CreateExpectedNodes(expectedRoot, file1, file2, dir1, file11, file12, dir11, file111, file112), o => o.IgnoringCyclicReferences());
    }

    [Test]
    public void DeleteEmptyFileFromRoot()
    {
        CreateTreeOnDisk(rootFullName);
        fileSystemHelper.CreateFile("3f", 0);
        
        var tree = new TestFileSystemEntryTree(rootFullName);
        tree.SynchronizeWithFileSystem($"{rootFullName}\\3f");
        tree.GetNodes.Should().Contain(e => e.Key == $"{rootFullName}\\3f");
        
        fileSystemHelper.Delete($"{rootFullName}\\3f");
        var changed = tree.SynchronizeWithFileSystem($"{rootFullName}\\3f");

        var file1 = new FileEntry($"{rootFullName}\\1f", 1000);
        var file2 = new FileEntry($"{rootFullName}\\2f", 1000);
        var expectedRoot = new DirectoryEntry(rootFullName, 2000)
        {
            Files = new[] { file1, file2 }
        };
        
        tree.Root.Should().BeEquivalentTo(expectedRoot, o => o.IgnoringCyclicReferences());
        changed.Should().BeEquivalentTo(new[] { expectedRoot }, o => o.IgnoringCyclicReferences());
        tree.GetNodes.Should().BeEquivalentTo(CreateExpectedNodes(expectedRoot, file1, file2), o => o.IgnoringCyclicReferences());
    }

    [Test]
    public void DeleteNotEmptyFileFromRoot()
    {
        CreateTreeOnDisk(rootFullName);
        
        var tree = new TestFileSystemEntryTree(rootFullName);
        fileSystemHelper.Delete($"{rootFullName}\\1f");
        var changed = tree.SynchronizeWithFileSystem($"{rootFullName}\\1f");

        var file2 = new FileEntry($"{rootFullName}\\2f", 1000);
        var expectedRoot = new DirectoryEntry(rootFullName, 1000)
        {
            Files = new[] { file2 }
        };
        
        tree.Root.Should().BeEquivalentTo(expectedRoot, o => o.IgnoringCyclicReferences());
        changed.Should().BeEquivalentTo(new[] { expectedRoot }, o => o.IgnoringCyclicReferences());
        tree.GetNodes.Should().BeEquivalentTo(CreateExpectedNodes(expectedRoot, file2), o => o.IgnoringCyclicReferences());
    }
    
    [Test]
    public void DeleteEmptyDirectoryFromRoot()
    {
        CreateTreeOnDisk(rootFullName);
        
        var tree = new TestFileSystemEntryTree(rootFullName);
        tree.SynchronizeWithFileSystem($"{rootFullName}\\2");
        tree.GetNodes.Should().Contain(e => e.Key == $"{rootFullName}\\2");
        
        fileSystemHelper.Delete($"{rootFullName}\\2");
        var changed = tree.SynchronizeWithFileSystem($"{rootFullName}\\2");

        var file1 = new FileEntry($"{rootFullName}\\1f", 1000);
        var file2 = new FileEntry($"{rootFullName}\\2f", 1000);
        var expectedRoot = new DirectoryEntry(rootFullName, 2000)
        {
            Files = new[] { file1, file2 }
        };
        
        tree.Root.Should().BeEquivalentTo(expectedRoot, o => o.IgnoringCyclicReferences());
        changed.Should().BeEquivalentTo(new[] { expectedRoot }, o => o.IgnoringCyclicReferences());
        tree.GetNodes.Should().BeEquivalentTo(CreateExpectedNodes(expectedRoot, file1, file2), o => o.IgnoringCyclicReferences());
    }

    [Test]
    public void DeleteNotEmptyDirectoryFromRoot()
    {
        CreateTreeOnDisk(rootFullName);
        
        var tree = new TestFileSystemEntryTree(rootFullName);
        tree.SynchronizeWithFileSystem($"{rootFullName}\\1\\11\\111f");
        tree.GetNodes.Should().Contain(e => e.Key == $"{rootFullName}\\1\\11\\111f");
        
        fileSystemHelper.Delete($"{rootFullName}\\1");
        var changed = tree.SynchronizeWithFileSystem($"{rootFullName}\\1");

        var file1 = new FileEntry($"{rootFullName}\\1f", 1000);
        var file2 = new FileEntry($"{rootFullName}\\2f", 1000);
        var expectedRoot = new DirectoryEntry(rootFullName, 2000)
        {
            Files = new[] { file1, file2 }
        };
        
        tree.Root.Should().BeEquivalentTo(expectedRoot, o => o.IgnoringCyclicReferences());
        changed.Should().BeEquivalentTo(new[] { expectedRoot }, o => o.IgnoringCyclicReferences());
        tree.GetNodes.Should().BeEquivalentTo(CreateExpectedNodes(expectedRoot, file1, file2), o => o.IgnoringCyclicReferences());
    }
    
    [Test]
    public void DeleteEmptyFileFromSubdirectory()
    {
        CreateTreeOnDisk(rootFullName);
        fileSystemHelper.CreateFile($"{rootFullName}\\1\\11\\113f", 0);
        
        var tree = new TestFileSystemEntryTree(rootFullName);
        tree.SynchronizeWithFileSystem($"{rootFullName}\\1\\11\\113f");
        tree.GetNodes.Should().Contain(e => e.Key == $"{rootFullName}\\1\\11\\113f");
        
        fileSystemHelper.Delete($"{rootFullName}\\1\\11\\113f");
        var changed = tree.SynchronizeWithFileSystem($"{rootFullName}\\1\\11\\113f");
        
        var file111 = new FileEntry($"{rootFullName}\\1\\11\\111f", 1000);
        var file112 = new FileEntry($"{rootFullName}\\1\\11\\112f", 1000);
        var dir11 = new DirectoryEntry($"{rootFullName}\\1\\11", 2000)
        {
            Files = new[] { file111, file112 }
        };
        
        var file11 = new FileEntry($"{rootFullName}\\1\\11f", 1000);
        var file12 = new FileEntry($"{rootFullName}\\1\\12f", 1000);
        var dir1 = new DirectoryEntry($"{rootFullName}\\1", 4000)
        {
            Subdirectories = new[] { dir11 },
            Files = new[] { file11, file12 }
        };
        var file1 = new FileEntry($"{rootFullName}\\1f", 1000);
        var file2 = new FileEntry($"{rootFullName}\\2f", 1000);
        var expectedRoot = new DirectoryEntry(rootFullName, 6000)
        {
            Subdirectories = new[] { dir1 },
            Files = new[] { file1, file2 }
        };
        
        tree.Root.Should().BeEquivalentTo(expectedRoot, o => o.IgnoringCyclicReferences());
        changed.Should().BeEquivalentTo(new FileSystemEntry[] { dir11 },
            o => o.IgnoringCyclicReferences());
        tree.GetNodes.Should().BeEquivalentTo(CreateExpectedNodes(expectedRoot, file1, file2, dir1, file11, file12, dir11, file111, file112), o => o.IgnoringCyclicReferences());
    }

    [Test]
    public void DeleteNotEmptyFileFromSubdirectory()
    {
        CreateTreeOnDisk(rootFullName);
        
        var tree = new TestFileSystemEntryTree(rootFullName);
        tree.SynchronizeWithFileSystem($"{rootFullName}\\1\\11\\111f");
        tree.GetNodes.Should().Contain(e => e.Key == $"{rootFullName}\\1\\11\\111f");
        
        fileSystemHelper.Delete($"{rootFullName}\\1\\11\\112f");
        var changed = tree.SynchronizeWithFileSystem($"{rootFullName}\\1\\11\\112f");
        
        var file111 = new FileEntry($"{rootFullName}\\1\\11\\111f", 1000);
        var dir11 = new DirectoryEntry($"{rootFullName}\\1\\11", 1000)
        {
            Files = new[] { file111 }
        };
        
        var file11 = new FileEntry($"{rootFullName}\\1\\11f", 1000);
        var file12 = new FileEntry($"{rootFullName}\\1\\12f", 1000);
        var dir1 = new DirectoryEntry($"{rootFullName}\\1", 3000)
        {
            Subdirectories = new[] { dir11 },
            Files = new[] { file11, file12 }
        };
        var file1 = new FileEntry($"{rootFullName}\\1f", 1000);
        var file2 = new FileEntry($"{rootFullName}\\2f", 1000);
        var expectedRoot = new DirectoryEntry(rootFullName, 5000)
        {
            Subdirectories = new[] { dir1 },
            Files = new[] { file1, file2 }
        };
        
        tree.Root.Should().BeEquivalentTo(expectedRoot, o => o.IgnoringCyclicReferences());
        changed.Should().BeEquivalentTo(new FileSystemEntry[] { dir11, dir1, expectedRoot },
            o => o.IgnoringCyclicReferences());
        tree.GetNodes.Should().BeEquivalentTo(CreateExpectedNodes(expectedRoot, file1, file2, dir1, file11, file12, dir11, file111), o => o.IgnoringCyclicReferences());
    }
    
    [Test]
    public void DeleteEmptyDirectoryFromSubdirectory()
    {
        CreateTreeOnDisk(rootFullName);
        
        var tree = new TestFileSystemEntryTree(rootFullName);
        tree.SynchronizeWithFileSystem($"{rootFullName}\\1\\11\\111f");
        
        fileSystemHelper.CreateDirectory($"{rootFullName}\\1\\11\\111");
        tree.SynchronizeWithFileSystem($"{rootFullName}\\1\\11\\111");
        tree.GetNodes.Should().Contain(e => e.Key == $"{rootFullName}\\1\\11\\111");
        
        fileSystemHelper.Delete($"{rootFullName}\\1\\11\\111");
        var changed = tree.SynchronizeWithFileSystem($"{rootFullName}\\1\\11\\111");
        
        var file111 = new FileEntry($"{rootFullName}\\1\\11\\111f", 1000);
        var file112 = new FileEntry($"{rootFullName}\\1\\11\\112f", 1000);
        var dir11 = new DirectoryEntry($"{rootFullName}\\1\\11", 2000)
        {
            Files = new[] { file111, file112 }
        };
        
        var file11 = new FileEntry($"{rootFullName}\\1\\11f", 1000);
        var file12 = new FileEntry($"{rootFullName}\\1\\12f", 1000);
        var dir1 = new DirectoryEntry($"{rootFullName}\\1", 4000)
        {
            Subdirectories = new[] { dir11 },
            Files = new[] { file11, file12 }
        };
        var file1 = new FileEntry($"{rootFullName}\\1f", 1000);
        var file2 = new FileEntry($"{rootFullName}\\2f", 1000);
        var expectedRoot = new DirectoryEntry(rootFullName, 6000)
        {
            Subdirectories = new[] { dir1 },
            Files = new[] { file1, file2 }
        };
        
        tree.Root.Should().BeEquivalentTo(expectedRoot, o => o.IgnoringCyclicReferences());
        changed.Should().BeEquivalentTo(new FileSystemEntry[] { dir11 },
            o => o.IgnoringCyclicReferences());
        tree.GetNodes.Should().BeEquivalentTo(CreateExpectedNodes(expectedRoot, file1, file2, dir1, file11, file12, dir11, file111, file112), o => o.IgnoringCyclicReferences());
    }

    [Test]
    public void DeleteNotEmptyDirectoryFromSubdirectory()
    {
        CreateTreeOnDisk(rootFullName);
        
        var tree = new TestFileSystemEntryTree(rootFullName);
        tree.SynchronizeWithFileSystem($"{rootFullName}\\1\\11\\111f");
        tree.GetNodes.Should().Contain(e => e.Key == $"{rootFullName}\\1\\11\\111f");
        
        fileSystemHelper.Delete($"{rootFullName}\\1");
        var changed = tree.SynchronizeWithFileSystem($"{rootFullName}\\1");
        
        var file1 = new FileEntry($"{rootFullName}\\1f", 1000);
        var file2 = new FileEntry($"{rootFullName}\\2f", 1000);
        var expectedRoot = new DirectoryEntry(rootFullName, 2000)
        {
            Subdirectories = Array.Empty<DirectoryEntry>(),
            Files = new[] { file1, file2 }
        };
        
        tree.Root.Should().BeEquivalentTo(expectedRoot, o => o.IgnoringCyclicReferences());
        changed.Should().BeEquivalentTo(new FileSystemEntry[] { expectedRoot },
            o => o.IgnoringCyclicReferences());
        tree.GetNodes.Should().BeEquivalentTo(CreateExpectedNodes(expectedRoot, file1, file2), o => o.IgnoringCyclicReferences());

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
        fileSystemHelper.CreateDirectory(@"2");
    }

    private IEnumerable<KeyValuePair<string, FileSystemEntry>> CreateExpectedNodes(params FileSystemEntry[] entries)
    {
        foreach (var entry in entries)
        {
            yield return new KeyValuePair<string, FileSystemEntry>(entry.FullName, entry);
        }
    }

    [TearDown]
    public void TearDown()
    {
        fileSystemHelper.Clear();
    }
}