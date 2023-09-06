using FluentAssertions;
using Newtonsoft.Json;
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
        Assert.That(FileSystemEntriesTreeBuilder.Build("Unknown"), Is.Null);
    }

    [Test]
    public void EmptyDirectory()
    {
        var expected = new FileSystemEntryTree(new DirectoryEntry(root, root));
        var actual = FileSystemEntriesTreeBuilder.Build(root);
        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void FilesOnlyDirectory()
    {
        helper.CreateFiles(1000, "1f", "2f", "3f");
        var expected = new FileSystemEntryTree(new DirectoryEntry(root, root, 3000)
        {
            Files =
            {
                new FileEntry($"{root}\\1f", "1f", 1000),
                new FileEntry($"{root}\\2f", "2f", 1000),
                new FileEntry($"{root}\\3f", "3f", 1000),
            }
        });
        
        var actual = FileSystemEntriesTreeBuilder.Build(root);
        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void SubDirectory()
    {
        helper.CreateDirectory("1");
        helper.CreateFiles(1000, "1\\1f", "1\\2f");

        var expected = new FileSystemEntryTree(new DirectoryEntry(root, root, 2000)
        {
            Subdirectories =
            {
                new DirectoryEntry($"{root}\\1", "1", 2000)
                {
                    Files =
                    {
                        new FileEntry($@"{root}\1\1f","1f", 1000),
                        new FileEntry($@"{root}\1\2f", "2f", 1000),
                    }
                }
            }
        });
        
        var actual = FileSystemEntriesTreeBuilder.Build(root);
        actual.Should().BeEquivalentTo(expected);
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

        var expected = new FileSystemEntryTree(new DirectoryEntry(root, root, 9000)
        {
            Subdirectories =
            {
                new DirectoryEntry($"{root}\\1", "1", 2000)
                {
                    Files =
                    {
                        new FileEntry($@"{root}\1\11f", "11f", 1000),
                        new FileEntry($@"{root}\1\12f", "12f", 1000),
                    }
                },
                new DirectoryEntry($"{root}\\2", "2", 0),
                new DirectoryEntry($"{root}\\3", "3", 5000)
                {
                    Subdirectories =
                    {
                        new DirectoryEntry($@"{root}\3\31", "31", 2000)
                        {
                            Subdirectories =
                            {
                                new DirectoryEntry($@"{root}\3\31\311", "311", 0),
                            },
                            Files =
                            {
                                new FileEntry($@"{root}\3\31\311f", "311f", 1000),
                                new FileEntry($@"{root}\3\31\312f", "312f", 1000),
                            }
                        },
                    },
                    Files =
                    {
                        new FileEntry($@"{root}\3\31f", "31f", 1000),
                        new FileEntry($@"{root}\3\32f", "32f", 1000),
                        new FileEntry($@"{root}\3\33f", "33f", 1000),
                    }
                },
            },
            Files =
            {
                new FileEntry($"{root}\\01f", "01f", 1000),
                new FileEntry($"{root}\\02f", "02f", 1000),
            }
        });
        
        var actual = FileSystemEntriesTreeBuilder.Build(root);
        actual.Should().BeEquivalentTo(expected);
    }
    
    [TearDown]
    public void TearDown()
    {
        helper.Clear();
    }
}