using FluentAssertions;
using SpaceProfilerLogic.DirectoryWatcher;
using SpaceProfilerLogicTests.TestHelpers;

namespace SpaceProfilerLogicTests.FileWatcher;

public class FileWatcherTests
{
    private FileSystemHelper helper = null!;
    private const string root = "TestData";
    private DirectoryWatcher watcher = null!;
    
    [SetUp]
    public void SetUp()
    {
        helper = new FileSystemHelper(root);
        watcher = new DirectoryWatcher();
    }

    [Test]
    public void EmptyDirectoryCreated()
    {
        watcher.Start(root);
        helper.CreateDirectory("1");
       
        Thread.Sleep(100);
        
        var actual = watcher.FlushChanges();

        var expected = new List<FileSystemEventArgs>
        {
            new (WatcherChangeTypes.Created, Path.GetFullPath(root), "1"),
        };

        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void FileCreated()
    {
        watcher.Start(root);
        helper.CreateFile("1", 1000);
       
        Thread.Sleep(100);
        
        var actual = watcher.FlushChanges();

        var expected = new List<FileSystemEventArgs>
        {
            new (WatcherChangeTypes.Created, Path.GetFullPath(root), "1"),
            new (WatcherChangeTypes.Changed, Path.GetFullPath(root), "1"),
        };

        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void MoveDirectoryFromInside()
    {
        helper.CreateDirectory("toMove");
        helper.CreateDirectory("toMove\\1");
        helper.CreateDirectory("toMove\\2");
        helper.CreateFiles(1000, "toMove\\2\\1f", "toMove\\2\\2f");
        helper.CreateFiles(1000, "toMove\\1f");
        
        helper.CreateDirectory("destination");
        helper.CreateFile("destination\\1f", 1000);
        
        watcher.Start(root);
        
        Directory.Move($"{root}\\toMove", $"{root}\\destination\\toMove");
        Thread.Sleep(100);

        var actual = watcher.FlushChanges();
        var expected = new List<FileSystemEventArgs>
        {
            new (WatcherChangeTypes.Changed, Path.GetFullPath($"{root}"), "destination"),
            new (WatcherChangeTypes.Deleted, Path.GetFullPath($"{root}"), "toMove"),
            new (WatcherChangeTypes.Created, Path.GetFullPath($"{root}"), "destination\\toMove"),
            new (WatcherChangeTypes.Changed, Path.GetFullPath($"{root}"), "destination"),
        };

        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void MoveDirectoryFromOutside()
    {
        helper.CreateDirectory("toMove");
        helper.CreateDirectory("toMove\\1");
        helper.CreateDirectory("toMove\\2");
        helper.CreateFiles(1000, "toMove\\2\\1f", "toMove\\2\\2f");
        helper.CreateFiles(1000, "toMove\\1f");
        
        helper.CreateDirectory("destination");
        helper.CreateFile("destination\\1f", 1000);
        
        watcher.Start($"{root}\\destination");
        
        Directory.Move($"{root}\\toMove", $"{root}\\destination\\toMove");
        Thread.Sleep(100);

        var actual = watcher.FlushChanges();
        var expected = new List<FileSystemEventArgs>
        {
            new (WatcherChangeTypes.Created, Path.GetFullPath($"{root}\\destination"), "toMove"),
        };

        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void DeleteEmptyDirectory()
    {
        helper.CreateDirectory("1");
        
        watcher.Start(root);
        
        helper.Delete("1");
        Thread.Sleep(100);

        var actual = watcher.FlushChanges();
        var expected = new List<FileSystemEventArgs>
        {
            new (WatcherChangeTypes.Deleted, Path.GetFullPath($"{root}"), "1"),
        };
        
        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void DeleteNotEmptyDirectory()
    {
        helper.CreateDirectory("1");
        helper.CreateDirectory("1\\2");
        helper.CreateDirectory("1\\3");
        helper.CreateFiles(1000, "1\\1f", "1\\2\\2f");
        
        watcher.Start(root);
        
        helper.Delete("1");
        Thread.Sleep(200);

        var actual = watcher.FlushChanges();
        var expected = new List<FileSystemEventArgs>
        {
            new (WatcherChangeTypes.Changed, Path.GetFullPath($"{root}"), "1"),
            new (WatcherChangeTypes.Deleted, Path.GetFullPath($"{root}"), "1\\1f"),
            new (WatcherChangeTypes.Deleted, Path.GetFullPath($"{root}"), "1\\2\\2f"),
            new (WatcherChangeTypes.Changed, Path.GetFullPath($"{root}"), "1\\2"),
            new (WatcherChangeTypes.Deleted, Path.GetFullPath($"{root}"), "1\\2"),
            new (WatcherChangeTypes.Deleted, Path.GetFullPath($"{root}"), "1\\3"),
            new (WatcherChangeTypes.Changed, Path.GetFullPath($"{root}"), "1"),
            new (WatcherChangeTypes.Deleted, Path.GetFullPath($"{root}"), "1"),
        };
        
        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void DeleteFile()
    {
        helper.CreateDirectory("1");
        helper.CreateFile("1\\1f", 1000);
        
        watcher.Start(root);
        
        helper.Delete("1\\1f");
        Thread.Sleep(100);
        
        var actual = watcher.FlushChanges();

        var expected = new List<FileSystemEventArgs>
        {
            new (WatcherChangeTypes.Deleted, Path.GetFullPath(root), "1\\1f"),
        };

        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void ChangeAccessWritesForDirectory()
    {
        helper.CreateDirectory("1");
        helper.CreateFile("1\\1f", 1000);
        
        watcher.Start(root);
        
        helper.ChangeAccessRights("1");
        Thread.Sleep(100);
        
        var actual = watcher.FlushChanges();

        var expected = new List<FileSystemEventArgs>
        {
            new (WatcherChangeTypes.Changed, Path.GetFullPath(root), "1"),
            new (WatcherChangeTypes.Changed, Path.GetFullPath(root), "1\\1f"),
            new (WatcherChangeTypes.Changed, Path.GetFullPath(root), "1"),
        };

        actual.Should().BeEquivalentTo(expected);
    }
    
    [TearDown]
    public void TearDown()
    {
        
    }
}