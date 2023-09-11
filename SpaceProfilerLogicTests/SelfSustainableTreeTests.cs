using FluentAssertions;
using SpaceProfilerLogic;
using SpaceProfilerLogic.Tree;
using SpaceProfilerLogicTests.TestHelpers;

namespace SpaceProfilerLogicTests;

public class SelfSustainableTreeTests
{
    private FileSystemHelper helper = null!;
    private readonly string root = Path.GetFullPath("TestData");

    [SetUp]
    public void SetUp()
    {
        helper = new FileSystemHelper(root);
    }
   
    [Test]
    public void EmptyDirectoryDeleted()
    {
        helper.CreateDirectory("1");
        
        var actual = new SelfSustainableTree(root);
        Thread.Sleep(100);
        
        var created = new DirectoryEntry(root, 0)
        {
            Subdirectories = new DirectoryEntry[]
            {
                new($"{root}\\1", 0)
            },
        };
        actual.Root.Should().BeEquivalentTo(created, options => options.IgnoringCyclicReferences());
        
        DoWithDelay(() => helper.Delete("1"));

        var expected = new DirectoryEntry(root, 0);
        actual.Root.Should().BeEquivalentTo(expected, options => options.IgnoringCyclicReferences());
    }

    [Test]
    public void NotEmptyDirectoryDeleted()
    {
        helper.CreateDirectoryWithFiles("1", 1000, "1f");
        
        var actual = new SelfSustainableTree(root);
        Thread.Sleep(100);
        
        var created = new DirectoryEntry(root, 1000)
        {
            Subdirectories = new DirectoryEntry[]
            {
                new($"{root}\\1", 1000)
                {
                    Files = new FileEntry[]
                    {
                        new($@"{root}\1\1f", 1000)
                    }
                }
            },
        };
        actual.Root.Should().BeEquivalentTo(created, options => options.IgnoringCyclicReferences());
        
        DoWithDelay(() => helper.Delete("1"));

        var expected = new DirectoryEntry(root, 0);
        actual.Root.Should().BeEquivalentTo(expected, options => options.IgnoringCyclicReferences());
    }

    [Test]
    public void FileDeleted()
    {
        helper.CreateDirectoryWithFiles("1", 1000, "1f");
        
        var actual = new SelfSustainableTree(root);
        Thread.Sleep(100);
        
        var created = new DirectoryEntry(root, 1000)
        {
            Subdirectories = new DirectoryEntry[]
            {
                new($"{root}\\1", 1000)
                {
                    Files = new FileEntry[]
                    {
                        new($@"{root}\1\1f", 1000)
                    }
                }
            },
        };
        actual.Root.Should().BeEquivalentTo(created, options => options.IgnoringCyclicReferences());
        
        DoWithDelay(() => helper.Delete("1\\1f"));

        var expected = new DirectoryEntry(root, 0)
        {
            Subdirectories = new DirectoryEntry[]
            {
                new($"{root}\\1", 0)
            }
        };
        actual.Root.Should().BeEquivalentTo(expected, options => options.IgnoringCyclicReferences());
    }

    private DirectoryEntry? BuildTree(string treeRoot)
    {
        var tree = new SelfSustainableTree(treeRoot);
        Thread.Sleep(100);
        tree.StopSynchronization();
        return tree.Root;
    }

    private void DoWithDelay(Action action)
    {
        Thread.Sleep(500);
        action();
        Thread.Sleep(500);
    }
    
    [TearDown]
    public void TearDown()
    {
        helper.Clear();
    }
}