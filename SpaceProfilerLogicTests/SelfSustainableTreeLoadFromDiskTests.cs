using System.Diagnostics;
using FluentAssertions;
using SpaceProfilerLogic;
using SpaceProfilerLogic.Tree;
using FileSystemHelper = SpaceProfilerLogicTests.TestHelpers.FileSystemHelper;

namespace SpaceProfilerLogicTests;

public class SelfSustainableTreeLoadFromDiskTests
{
    private FileSystemHelper helper = null!;
    private readonly string root = Path.GetFullPath("TestData");
    
    [SetUp]
    public void SetUp()
    {
        helper = new FileSystemHelper(root);
    }
    
    [Test]
    public void DirectoryNotExist()
    {
        Assert.That(() => new SelfSustainableTree("Unknown"), Throws.ArgumentException);
    }

    [Test]
    public void EmptyDirectory()
    {
        var tree = new SelfSustainableTree(root);
        WaitUntilLoaded(tree, TimeSpan.FromMinutes(1));

        tree.Root.Should().BeEquivalentTo(new DirectoryEntry(root, 0));
        tree.GetChangedNodes().Should().BeEmpty();
    }

    [Test]
    public void Subdirectories()
    {
        helper.CreateDirectories("1", "1\\11", "1\\12", "2");
        
        var tree = new SelfSustainableTree(root);
        WaitUntilLoaded(tree, TimeSpan.FromMinutes(1));

        var dir11 = new DirectoryEntry(@$"{root}\1\11", 0);
        var dir12 = new DirectoryEntry(@$"{root}\1\12", 0);
        var dir1 = new DirectoryEntry($"{root}\\1", 0)
        {
            Subdirectories = new[] { dir11, dir12 }
        };
        var dir2 = new DirectoryEntry($"{root}\\2", 0);

        var expectedRoot = new DirectoryEntry(root, 0)
        {
            Subdirectories = new[] { dir1, dir2 }
        };

        tree.Root.Should().BeEquivalentTo(expectedRoot, o => o.IgnoringCyclicReferences());
        tree.GetChangedNodes().Should().BeEquivalentTo(new[] { expectedRoot, dir1, dir2, dir11, dir12 }, o => o.IgnoringCyclicReferences());
    }

    [Test]
    public void Files()
    {
        helper.CreateFiles(1000 , "1f", "2f");
        helper.CreateDirectoryWithFiles("1", 1000, "11f", "12f");
        helper.CreateDirectoryWithFiles("1\\11", 1000, "111f", "112f");
        helper.CreateDirectory(@"2");
        
        var tree = new SelfSustainableTree(root);
        WaitUntilLoaded(tree, TimeSpan.FromMinutes(1));

        var file11 = new FileEntry($@"{root}\1\11f", 1000);
        var file12 = new FileEntry($@"{root}\1\12f", 1000);
        var file111 = new FileEntry($@"{root}\1\11\111f", 1000);
        var file112 = new FileEntry($@"{root}\1\11\112f", 1000);

        var dir11 = new DirectoryEntry($@"{root}\1\11", 2000)
        {
            Files = new[] { file111, file112 }
        };
        var dir1 = new DirectoryEntry($"{root}\\1", 4000)
        {
            Files = new[] { file11, file12 },
            Subdirectories = new []{ dir11 }
        };
        var dir2 = new DirectoryEntry($"{root}\\2", 0);
        
        var expectedRoot = new DirectoryEntry(root, 6000)
        {
            Files = new[] { new FileEntry($"{root}\\1f", 1000), new FileEntry($"{root}\\2f", 1000) },
            Subdirectories = new[] { dir1, dir2 }
        };
        
        tree.Root.Should().BeEquivalentTo(expectedRoot, o => o.IgnoringCyclicReferences());
        tree.GetChangedNodes().Should().BeEquivalentTo(new FileSystemEntry[]
            { expectedRoot, dir1, dir2, dir11, file11, file12, file111, file112 });
    }

    private void WaitUntilLoaded(SelfSustainableTree tree, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();
        while (!tree.Loaded && stopwatch.Elapsed < timeout)
        {
            Thread.Sleep(10);
        }
        tree.StopSynchronization();
        if (!tree.Loaded)
            throw new Exception("Failed to load the tree");
    }
    
    [TearDown]
    public void TearDown()
    {
        helper.Clear();
    }
}