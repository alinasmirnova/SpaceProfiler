using FluentAssertions;
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

        var expected = BuildExpected("1");

        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void FileCreated()
    {
        watcher.Start(root);
        helper.CreateFile("1", 1000);
       
        Thread.Sleep(100);
        
        var actual = watcher.FlushChanges();

        var expected = BuildExpected("1");

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
        var expected = BuildExpected(
        "destination",
            "toMove",
            "destination\\toMove",
            "destination\\toMove\\1f",
            "destination\\toMove\\1",
            "destination\\toMove\\2",
            "destination\\toMove\\2\\1f",
            "destination\\toMove\\2\\2f"
        );

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
        var expected = BuildExpected(
            "destination\\toMove",
            "destination\\toMove\\1",
            "destination\\toMove\\2",
            "destination\\toMove\\2\\1f",
            "destination\\toMove\\2\\2f",
            "destination\\toMove\\1f"
        );

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
        var expected = BuildExpected("1");
        
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
        var expected = BuildExpected(
            "1",
            "1\\1f",
            "1\\2\\2f",
            "1\\2",
            "1\\3"
        );
        
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

        var expected = BuildExpected("1\\1f");

        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void ChangeAccessWritesForDirectory()
    {
        helper.CreateDirectory("1");
        helper.CreateDirectory("1\\2");
        helper.CreateFile("1\\2\\1f", 1000);
        
        watcher.Start(root);
        
        helper.ChangeAccessRights("1");
        Thread.Sleep(100);
        
        var actual = watcher.FlushChanges();
        var expected = BuildExpected(
            "1",
            "1\\2",
            "1\\2\\1f"
        );


        actual.Should().BeEquivalentTo(expected);
    }

    private HashSet<string> BuildExpected(params string[] paths)
    {
        var result = new HashSet<string>();
        foreach (var path in paths)
        {
            result.Add(GetFullPath(path));
        }

        return result;
    }

    private string GetFullPath(string path) => Path.GetFullPath(Path.Combine(root, path));

    [TearDown]
    public void TearDown()
    {
        
    }
}