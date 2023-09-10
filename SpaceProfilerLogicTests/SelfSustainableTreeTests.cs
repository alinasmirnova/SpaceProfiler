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
    public void DirectoryNotExist()
    {
        Assert.That(() => new SelfSustainableTree("Unknown"), Throws.ArgumentException);
    }

    [Test]
    public void EmptyDirectory()
    {
        var expected = new DirectoryEntry(root);
        var actual = BuildTree(root);
        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void FilesOnlyDirectory()
    {
        helper.CreateFiles(1000, "1f", "2f", "3f");
        var expected = new DirectoryEntry(root, 3000)
        {
            Files = new FileEntry[]{
                new($"{root}\\1f", 1000),
                new($"{root}\\2f", 1000),
                new($"{root}\\3f", 1000)}
        };
        
        var actual = BuildTree(root);
        actual.Should().BeEquivalentTo(expected, options => options.IgnoringCyclicReferences());
    }

    [Test]
    public void SubDirectory()
    {
        helper.CreateDirectoryWithFiles("1", 1000, "1f", "2f");

        var expected = new DirectoryEntry(root, 2000)
        {
            Subdirectories = new DirectoryEntry[]
            {
                new($"{root}\\1", 2000)
                {
                    Files = new FileEntry[]
                    {
                        new($@"{root}\1\1f", 1000),
                        new($@"{root}\1\2f", 1000),
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
        
        helper.CreateDirectoryWithFiles("1", 1000, "11f", "12f");
        
        helper.CreateDirectory("2");
        
        helper.CreateDirectoryWithFiles("3", 1000, "31f", "32f", "33f");
        helper.CreateDirectoryWithFiles("3\\31", 1000, "311f", "312f");
        helper.CreateDirectory(@"3\31\311");

        var expected = new DirectoryEntry(root, 9000)
        {
            Subdirectories = new DirectoryEntry[]
            {
                new ($"{root}\\1", 2000)
                {
                    Files = new FileEntry[]
                    {
                        new ($@"{root}\1\11f", 1000),
                        new ($@"{root}\1\12f", 1000),
                    }
                },
                new ($"{root}\\2", 0),
                new ($"{root}\\3", 5000)
                {
                    Subdirectories = new DirectoryEntry[]
                    {
                        new ($@"{root}\3\31", 2000)
                        {
                            Subdirectories = new DirectoryEntry[]
                            {
                                new ($@"{root}\3\31\311", 0),
                            },
                            Files = new FileEntry[]
                            {
                                new ($@"{root}\3\31\311f", 1000),
                                new ($@"{root}\3\31\312f", 1000),
                            }
                        },
                    },
                    Files = new FileEntry[]
                    {
                        new ($@"{root}\3\31f", 1000),
                        new ($@"{root}\3\32f", 1000),
                        new ($@"{root}\3\33f", 1000),
                    }
                },
            },
            Files = new FileEntry[]
            {
                new ($"{root}\\01f", 1000),
                new ($"{root}\\02f", 1000),
            }
        };
        
        var actual = BuildTree(root);
        actual.Should().BeEquivalentTo(expected, options => options.IgnoringCyclicReferences());
    }

    [Test]
    public void EmptyDirectoryAdded()
    {
        var actual = new SelfSustainableTree(root);
        actual.StartSynchronization();
        DoWithDelay(() => helper.CreateDirectory("1"));
        actual.StopSynchronization();

        var expected = new DirectoryEntry(root, 0)
        {
            Subdirectories = new DirectoryEntry[]
            {
                new($"{root}\\1", 0)
            }
        };

        actual.Root.Should().BeEquivalentTo(expected, options => options.IgnoringCyclicReferences());
    }

    [Test]
    public void NotEmptyDirectoryAdded()
    {
        var actual = new SelfSustainableTree(root);
        actual.StartSynchronization();
        DoWithDelay(() => helper.CreateDirectoryWithFiles("1", 1000, "1f"));
        actual.StopSynchronization();

        var expected = new DirectoryEntry(root, 1000)
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
        
        actual.Root.Should().BeEquivalentTo(expected, options => options.IgnoringCyclicReferences());
    }

    [Test]
    public void FileAdded()
    {
        var actual = new SelfSustainableTree(root);
        actual.StartSynchronization();
        DoWithDelay(() => helper.CreateFile("1f", 1000));
        actual.StopSynchronization();

        var expected = new DirectoryEntry(root, 1000)
        {
            Files = new FileEntry[]
            {
                new($@"{root}\1f", 1000)
            }
        };
        
        actual.Root.Should().BeEquivalentTo(expected, options => options.IgnoringCyclicReferences());
    }

    [Test]
    public void EmptyDirectoryDeleted()
    {
        helper.CreateDirectory("1");
        
        var actual = new SelfSustainableTree(root);
        actual.StartSynchronization();
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
        actual.StartSynchronization();
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
        actual.StartSynchronization();
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
        tree.StartSynchronization();
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