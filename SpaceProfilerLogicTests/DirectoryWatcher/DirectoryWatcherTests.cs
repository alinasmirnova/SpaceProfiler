using FluentAssertions;
using SpaceProfilerLogic.DirectoryWatcher;
using SpaceProfilerLogicTests.TestHelpers;

namespace SpaceProfilerLogicTests.DirectoryWatcher;

public class DirectoryWatcherTests
{
    private FileSystemHelper helper = null!;
    private const string root = "TestData";
    private SpaceProfilerLogic.DirectoryWatcher.DirectoryWatcher watcher = null!;
    
    [SetUp]
    public void SetUp()
    {
        helper = new FileSystemHelper(root);
        watcher = new SpaceProfilerLogic.DirectoryWatcher.DirectoryWatcher();
    }

    [Test]
    public void EmptyDirectoryCreated()
    {
        watcher.Start(root);
        helper.CreateDirectory("1");
       
        Thread.Sleep(100);
        
        var actual = watcher.FlushChanges();

        var expected = new List<Change>
        {
            new (GetFullPath("1"), ChangeType.Create),
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

        var expected = new List<Change>
        {
            new (GetFullPath("1"), ChangeType.Create),
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
        var expected = new List<Change>
        {
            new (GetFullPath("destination"), ChangeType.Update),
            new (GetFullPath("toMove"), ChangeType.Delete),
            new (GetFullPath("destination\\toMove"), ChangeType.Create),
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
        var expected = new List<Change>
        {
            new (GetFullPath("destination\\toMove"), ChangeType.Create),
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
        var expected = new List<Change>
        {
            new (GetFullPath("1"), ChangeType.Delete),
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
        var expected = new List<Change>
        {
            new (GetFullPath("1"), ChangeType.Delete),
            new (GetFullPath("1\\1f"), ChangeType.Delete),
            new (GetFullPath("1\\2\\2f"), ChangeType.Delete),
            new (GetFullPath("1\\2"), ChangeType.Delete),
            new (GetFullPath("1\\3"), ChangeType.Delete),
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

        var expected = new List<Change>
        {
            new (GetFullPath("1\\1f"), ChangeType.Delete),
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
        var expected = new List<Change>
        {
            new (GetFullPath("1"), ChangeType.Update),
            new (GetFullPath("1\\1f"), ChangeType.Update),
        };


        actual.Should().BeEquivalentTo(expected);
    }

    private string GetFullPath(string path) => Path.GetFullPath(Path.Combine(root, path));

    [TearDown]
    public void TearDown()
    {
        
    }
}