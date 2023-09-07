using FluentAssertions;
using Newtonsoft.Json;
using SpaceProfilerLogic;
using SpaceProfilerLogic.Tree;

namespace SpaceProfilerLogicTests.Tree;

public class FileSystemEntriesTreeBuilderTests
{
    private FileSystemHelper helper = null!;
    private const string root = "TestData";

    [SetUp]
    public void SetUp()
    {
        helper = new FileSystemHelper(root);
    }

    [Test]
    public void DirectoryNotExist()
    {
        Assert.That(() => SelfSustainableTreeBuilder.Build("Unknown"), Throws.ArgumentException);
    }

    [Test]
    public void EmptyDirectory()
    {
        var expected = new DirectoryEntry(root, root);
        var actual = BuildTree(root);
        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void FilesOnlyDirectory()
    {
        helper.CreateFiles(1000, "1f", "2f", "3f");
        var expected = new DirectoryEntry(root, root, 3000)
        {
            Files = new FileEntry[]{
                new($"{root}\\1f", "1f", 1000),
                new($"{root}\\2f", "2f", 1000),
                new($"{root}\\3f", "3f", 1000)}
        };
        
        var actual = BuildTree(root);
        actual.Should().BeEquivalentTo(expected, options => options.IgnoringCyclicReferences());
    }

    [Test]
    public void SubDirectory()
    {
        helper.CreateDirectory("1");
        helper.CreateFiles(1000, "1\\1f", "1\\2f");

        var expected = new DirectoryEntry(root, root, 2000)
        {
            Subdirectories = new DirectoryEntry[]
            {
                new($"{root}\\1", "1", 2000)
                {
                    Files = new FileEntry[]
                    {
                        new($@"{root}\1\1f","1f", 1000),
                        new($@"{root}\1\2f", "2f", 1000),
                    }
                }
            }
        };
        
        var actual = BuildTree(root);
        actual.Should().BeEquivalentTo(expected, options => options.IgnoringCyclicReferences());
    }

    [Test]
    public void FilesAndSubDirectories()
    {
        helper.CreateFiles(1000, "01f", "02f");
        
        helper.CreateDirectory("1");
        helper.CreateFiles(1000, "1\\11f", "1\\12f");
        
        helper.CreateDirectory("2");
        
        helper.CreateDirectory("3");
        helper.CreateFiles(1000, "3\\31f", "3\\32f", "3\\33f");
        helper.CreateDirectory("3\\31");
        helper.CreateDirectory(@"3\31\311");
        helper.CreateFiles(1000, @"3\31\311f", @"3\31\312f");

        var expected = new DirectoryEntry(root, root, 9000)
        {
            Subdirectories = new DirectoryEntry[]
            {
                new ($"{root}\\1", "1", 2000)
                {
                    Files = new FileEntry[]
                    {
                        new ($@"{root}\1\11f", "11f", 1000),
                        new ($@"{root}\1\12f", "12f", 1000),
                    }
                },
                new ($"{root}\\2", "2", 0),
                new ($"{root}\\3", "3", 5000)
                {
                    Subdirectories = new DirectoryEntry[]
                    {
                        new ($@"{root}\3\31", "31", 2000)
                        {
                            Subdirectories = new DirectoryEntry[]
                            {
                                new ($@"{root}\3\31\311", "311", 0),
                            },
                            Files = new FileEntry[]
                            {
                                new ($@"{root}\3\31\311f", "311f", 1000),
                                new ($@"{root}\3\31\312f", "312f", 1000),
                            }
                        },
                    },
                    Files = new FileEntry[]
                    {
                        new ($@"{root}\3\31f", "31f", 1000),
                        new ($@"{root}\3\32f", "32f", 1000),
                        new ($@"{root}\3\33f", "33f", 1000),
                    }
                },
            },
            Files = new FileEntry[]
            {
                new ($"{root}\\01f", "01f", 1000),
                new ($"{root}\\02f", "02f", 1000),
            }
        };
        
        var actual = BuildTree(root);
        actual.Should().BeEquivalentTo(expected, options => options.IgnoringCyclicReferences());
    }

    private DirectoryEntry BuildTree(string treeRoot)
    {
        var tree = SelfSustainableTreeBuilder.Build(treeRoot);
        tree.StartSynchronization();
        Thread.Sleep(100);
        tree.StopSynchronization();
        return tree.Root;
    }
    
    [TearDown]
    public void TearDown()
    {
        helper.Clear();
    }
}