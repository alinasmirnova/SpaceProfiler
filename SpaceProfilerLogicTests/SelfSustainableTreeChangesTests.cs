using System.Diagnostics;
using FluentAssertions;
using FluentAssertions.Equivalency;
using SpaceProfilerLogic;
using SpaceProfilerLogic.Tree;
using SpaceProfilerLogicTests.TestHelpers;

namespace SpaceProfilerLogicTests;

public class SelfSustainableTreeChangesTests
{
    private FileSystemHelper helper = null!;
    private readonly string rootFullName = Path.GetFullPath("TestData");
    private SelfSustainableTree? tree;

    private readonly Func<EquivalencyAssertionOptions<DirectoryEntry>,EquivalencyAssertionOptions<DirectoryEntry>> options = o => o
        .IgnoringCyclicReferences()
        .WithoutStrictOrderingFor(entry => entry.Files)
        .WithoutStrictOrderingFor(entry => entry.Subdirectories);

    [SetUp]
    public void SetUp()
    {
        helper = new FileSystemHelper(rootFullName);
    }

    [Test]
    public void TryAddExternalFile()
    {
        (tree, var expectedRoot) = CreateTree(Path.GetFullPath($"{rootFullName}\\TestData1"));
        
        DoWithDelay(tree, () => helper.CreateFile("external", 1000));
        
        tree.Root.Should().BeEquivalentTo(expectedRoot, options);
        tree.GetChangedNodes().Should().BeEmpty();
    }

    [Test]
    public void TryAddExternalDirectory()
    {
        (tree, var expectedRoot) = CreateTree(Path.GetFullPath($"{rootFullName}\\TestData1"));
        
        DoWithDelay(tree, () => helper.CreateDirectory("external"));
        
        tree.Root.Should().BeEquivalentTo(expectedRoot, options);
        tree.GetChangedNodes().Should().BeEmpty();
    }

    [Test]
    public void AddEmptyDirectoryToTheRoot()
    {
        (tree, var expectedRoot) = CreateTree(rootFullName);
        
        DoWithDelay(tree, () => helper.CreateDirectory("3"));

        var newDir = new DirectoryEntry($"{rootFullName}\\3");
        expectedRoot.Subdirectories = expectedRoot.Subdirectories.Concat(new[] { newDir }).ToArray();
        
        tree.Root.Should().BeEquivalentTo(expectedRoot, options);
        tree.GetChangedNodes().Should().BeEquivalentTo(new[] { newDir, expectedRoot }, options);
    }

    [Test]
    public void AddEmptyDirectoryToSubdirectory()
    {
        (tree, var expectedRoot) = CreateTree(rootFullName);
        
        DoWithDelay(tree, () => helper.CreateDirectory("2\\3"));

        var newDir = new DirectoryEntry($"{rootFullName}\\2\\3");
        var parent = (DirectoryEntry) Find(expectedRoot, $"{rootFullName}\\2")!;
        parent.Subdirectories = new[] { newDir };
        
        tree.Root.Should().BeEquivalentTo(expectedRoot, options);
        tree.GetChangedNodes().Should().BeEquivalentTo(new [] { newDir, parent }, options);
    }

    [Test]
    public void AddNotEmptyDirectoryToTheRoot()
    {
        (tree, var expectedRoot) = CreateTree($"{rootFullName}\\TestData1");
        
        helper.CreateDirectoryWithFiles("3", 1000, "31f", "32f");
        DoWithDelay(tree, () => Directory.Move($"{rootFullName}\\3", $"{rootFullName}\\TestData1\\3"));
        
        var newFile1 = new FileEntry($"{rootFullName}\\TestData1\\3\\31f", 1000);
        var newFile2 = new FileEntry($"{rootFullName}\\TestData1\\3\\32f", 1000);
        var newDir = new DirectoryEntry($@"{rootFullName}\TestData1\3", 2000)
        {
            Files = new[] { newFile1, newFile2 }
        };

        expectedRoot = new DirectoryEntry(expectedRoot.FullName, expectedRoot.GetSize + 2000)
        {
            Files = expectedRoot.Files,
            Subdirectories = expectedRoot.Subdirectories.Concat(new []{ newDir }).ToArray()
        };
        
        tree.Root.Should().BeEquivalentTo(expectedRoot, options);
        tree.GetChangedNodes().Should().BeEquivalentTo(new FileSystemEntry[] {expectedRoot, newDir, newFile1, newFile2}, o => o.IgnoringCyclicReferences());
    }
    
    [Test]
    public void AddNotEmptyDirectoryToTheSubdirectory()
    {
        (tree, var expectedRoot) = CreateTree($"{rootFullName}\\TestData1");
        
        helper.CreateDirectoryWithFiles("3", 1000, "31f", "32f");
        DoWithDelay(tree, () => Directory.Move($"{rootFullName}\\3", $"{rootFullName}\\TestData1\\2\\3"));
        
        var newFile1 = new FileEntry($"{rootFullName}\\TestData1\\2\\3\\31f", 1000);
        var newFile2 = new FileEntry($"{rootFullName}\\TestData1\\2\\3\\32f", 1000);
        var newDir = new DirectoryEntry($@"{rootFullName}\TestData1\2\3", 2000)
        {
            Files = new[] { newFile1, newFile2 }
        };

        var oldDir2 = (DirectoryEntry)Find(expectedRoot, $"{rootFullName}\\TestData1\\2")!;
        var newDir2 = new DirectoryEntry($@"{rootFullName}\TestData1\2", 2000)
        {
            Subdirectories = new[] { newDir }
        };

        expectedRoot = new DirectoryEntry(expectedRoot.FullName, expectedRoot.GetSize + 2000)
        {
            Files = expectedRoot.Files,
            Subdirectories = expectedRoot.Subdirectories.Where(d => d != oldDir2).Concat(new []{ newDir2 }).ToArray()
        };
        
        tree.Root.Should().BeEquivalentTo(expectedRoot, options);
        tree.GetChangedNodes().Should().BeEquivalentTo(new FileSystemEntry[] {expectedRoot, newDir, newDir2, newFile1, newFile2}, o => o.IgnoringCyclicReferences());
    }

    [Test]
    public void AddFileToTheRoot()
    {
        (tree, var expectedRoot) = CreateTree($"{rootFullName}");
        
        DoWithDelay(tree, () => helper.CreateFile("3f", 1000));
        
        var newFile = new FileEntry($"{rootFullName}\\3f", 1000);

        expectedRoot = new DirectoryEntry(expectedRoot.FullName, expectedRoot.GetSize + 1000)
        {
            Files = new[]
            {
                expectedRoot.Files[0],
                expectedRoot.Files[1],
                newFile
            },
            Subdirectories = expectedRoot.Subdirectories
        };
        
        tree.Root.Should().BeEquivalentTo(expectedRoot, options);
        tree.GetChangedNodes().Should().BeEquivalentTo(new FileSystemEntry[] {expectedRoot, newFile}, o => o.IgnoringCyclicReferences());
    }

    [Test]
    public void AddEmptyFileToTheRoot()
    {
        (tree, var expectedRoot) = CreateTree($"{rootFullName}");
        
        DoWithDelay(tree, () => helper.CreateFile("3f", 0));
        
        var newFile = new FileEntry($"{rootFullName}\\3f", 0);

        expectedRoot.Files = new[]
        {
            expectedRoot.Files[0],
            expectedRoot.Files[1],
            newFile
        };
        
        tree.Root.Should().BeEquivalentTo(expectedRoot, options);
        tree.GetChangedNodes().Should().BeEquivalentTo(new FileSystemEntry[] {expectedRoot, newFile}, o => o.IgnoringCyclicReferences());
    }

    [Test]
    public void AddFileToSubdirectory()
    {
        (tree, var expectedRoot) = CreateTree($"{rootFullName}");
        
        DoWithDelay(tree, () => helper.CreateFile("2\\21f", 1000));
        
        var newFile = new FileEntry($"{rootFullName}\\2\\21f", 1000);
        var newDir2 = new DirectoryEntry($"{rootFullName}\\2", 1000)
        {
            Files = new[] { newFile }
        };
        
        var parent = Find(expectedRoot, $"{rootFullName}\\2");
        expectedRoot = new DirectoryEntry(expectedRoot.FullName, expectedRoot.GetSize + 1000)
        {
            Files = expectedRoot.Files,
            Subdirectories = expectedRoot.Subdirectories.Where(d => d != parent).Concat(new[] { newDir2 }).ToArray()
        };
        
        tree.Root.Should().BeEquivalentTo(expectedRoot, options);
        tree.GetChangedNodes().Should().BeEquivalentTo(new FileSystemEntry[] {expectedRoot, newDir2, newFile}, o => o.IgnoringCyclicReferences());
    }

    [Test]
    public void AddEmptyFileToSubdirectory()
    {
        (tree, var expectedRoot) = CreateTree($"{rootFullName}");
        
        DoWithDelay(tree, () => helper.CreateFile("2\\21f", 0));
        
        var newFile = new FileEntry($"{rootFullName}\\2\\21f", 0);
        var parent = (DirectoryEntry)Find(expectedRoot, $"{rootFullName}\\2")!;
        parent.Files = new[] { newFile };
        
        tree.Root.Should().BeEquivalentTo(expectedRoot, options);
        tree.GetChangedNodes().Should().BeEquivalentTo(new FileSystemEntry[] { parent, newFile }, o => o.IgnoringCyclicReferences());
    }
    
    [Test]
    public void ChangeFileSizeInRoot()
    {
        (tree, var expectedRoot) = CreateTree($"{rootFullName}");
        
        DoWithDelay(tree, () => helper.ChangeFile("1f", 500));
        
        var newFile = new FileEntry($"{rootFullName}\\1f", 500);
        var oldFile = (FileEntry)Find(expectedRoot, $"{rootFullName}\\1f")!;
        expectedRoot = new DirectoryEntry(expectedRoot.FullName, expectedRoot.GetSize - 500)
        {
            Files = expectedRoot.Files.Where(f => f != oldFile).Concat(new[] { newFile }).ToArray(),
            Subdirectories = expectedRoot.Subdirectories
        };
        
        tree.Root.Should().BeEquivalentTo(expectedRoot, options);
        tree.GetChangedNodes().Should().BeEquivalentTo(new FileSystemEntry[] {expectedRoot, newFile}, o => o.IgnoringCyclicReferences());
    }
    
    [Test]
    public void ClearFileInRoot()
    {
        (tree, var expectedRoot) = CreateTree($"{rootFullName}");
        
        DoWithDelay(tree, () => helper.ChangeFile("1f", 0));
        
        var newFile = new FileEntry($"{rootFullName}\\1f", 0);
        var oldFile = (FileEntry)Find(expectedRoot, $"{rootFullName}\\1f")!;
        expectedRoot = new DirectoryEntry(expectedRoot.FullName, expectedRoot.GetSize - 1000)
        {
            Files = expectedRoot.Files.Where(f => f != oldFile).Concat(new[] { newFile }).ToArray(),
            Subdirectories = expectedRoot.Subdirectories
        };
        
        tree.Root.Should().BeEquivalentTo(expectedRoot, options);
        tree.GetChangedNodes().Should().BeEquivalentTo(new FileSystemEntry[] {expectedRoot, newFile}, o => o.IgnoringCyclicReferences());
    }
    
    [Test]
    public void ChangeFileSizeInSubdirectory()
    {
        (tree, var expectedRoot) = CreateTree($"{rootFullName}");
        
        DoWithDelay(tree, () => helper.ChangeFile("1\\11f", 500));
        
        var newFile = new FileEntry($"{rootFullName}\\1\\11f", 500);
        var oldFile = (FileEntry)Find(expectedRoot, $"{rootFullName}\\1\\11f")!;
        var oldParent = (DirectoryEntry)Find(expectedRoot, $"{rootFullName}\\1")!;
        var newParent = new DirectoryEntry(oldParent.FullName, oldParent.GetSize - 500)
        {
            Subdirectories = oldParent.Subdirectories,
            Files = oldParent.Files.Where(f => f != oldFile).Concat(new[] { newFile }).ToArray()
        };

        expectedRoot = new DirectoryEntry(expectedRoot.FullName, expectedRoot.GetSize - 500)
        {
            Subdirectories = expectedRoot.Subdirectories.Where(d => d != oldParent).Concat(new[] { newParent })
                .ToArray(),
            Files = expectedRoot.Files
        };
        
        tree.Root.Should().BeEquivalentTo(expectedRoot, options);
        tree.GetChangedNodes().Should().BeEquivalentTo(new FileSystemEntry[] {expectedRoot, newParent, newFile}, o => o.IgnoringCyclicReferences());
    }
    
    [Test]
    public void ClearFileInSubdirectory()
    {
        (tree, var expectedRoot) = CreateTree($"{rootFullName}");
        
        DoWithDelay(tree, () => helper.ChangeFile("1\\11f", 0));
        
        var newFile = new FileEntry($"{rootFullName}\\1\\11f", 0);
        var oldFile = (FileEntry)Find(expectedRoot, $"{rootFullName}\\1\\11f")!;
        var oldParent = (DirectoryEntry)Find(expectedRoot, $"{rootFullName}\\1")!;
        var newParent = new DirectoryEntry(oldParent.FullName, oldParent.GetSize - 1000)
        {
            Subdirectories = oldParent.Subdirectories,
            Files = oldParent.Files.Where(f => f != oldFile).Concat(new[] { newFile }).ToArray()
        };

        expectedRoot = new DirectoryEntry(expectedRoot.FullName, expectedRoot.GetSize - 1000)
        {
            Subdirectories = expectedRoot.Subdirectories.Where(d => d != oldParent).Concat(new[] { newParent })
                .ToArray(),
            Files = expectedRoot.Files
        };
        
        tree.Root.Should().BeEquivalentTo(expectedRoot, options);
        tree.GetChangedNodes().Should().BeEquivalentTo(new FileSystemEntry[] {expectedRoot, newParent, newFile}, o => o.IgnoringCyclicReferences());
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
    private (SelfSustainableTree tree, DirectoryEntry expectedRoot) CreateTree(string root)
    {
        var fileSystemHelper = new FileSystemHelper(root);
        fileSystemHelper.CreateFiles(1000 , "1f", "2f");
        fileSystemHelper.CreateDirectoryWithFiles("1", 1000, "11f", "12f");
        fileSystemHelper.CreateDirectoryWithFiles("1\\11", 1000, "111f", "112f");
        fileSystemHelper.CreateDirectory(@"2");
        
        var sustainableTree = new SelfSustainableTree(root);
        WaitUntilLoaded(sustainableTree, TimeSpan.FromMinutes(1));
        sustainableTree.GetChangedNodes();

        return (sustainableTree, new DirectoryEntry(root, 6000)
        {
            Files = new[] { new FileEntry($"{root}\\1f", 1000), new FileEntry($"{root}\\2f", 1000) },
            Subdirectories = new[]
            {
                new DirectoryEntry($"{root}\\1", 4000)
                {
                    Files = new[] { new FileEntry($@"{root}\1\11f", 1000), new FileEntry($@"{root}\1\12f", 1000) },
                    Subdirectories = new[]
                    {
                        new DirectoryEntry($@"{root}\1\11", 2000)
                        {
                            Files = new[]
                                { new FileEntry($@"{root}\1\11\111f", 1000), new FileEntry($@"{root}\1\11\112f", 1000) }
                        }
                    }
                },
                new DirectoryEntry($"{root}\\2")
            }
        });
    }
    
    private void WaitUntilLoaded(SelfSustainableTree tree, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();
        while (!tree.Loaded && stopwatch.Elapsed < timeout)
        {
            Thread.Sleep(10);
        }
        if (!tree.Loaded)
            throw new Exception("Failed to load the tree");
        tree.GetChangedNodes();
    }

    private FileSystemEntry? Find(DirectoryEntry root, string path)
    {
        var queue = new Queue<FileSystemEntry>();
        queue.Enqueue(root);
        while (queue.TryDequeue(out var current))
        {
            if (current.FullName == path)
            {
                return current;
            }

            if (current is DirectoryEntry directory)
            {
                foreach (var file in directory.Files)
                {
                    queue.Enqueue(file);
                }

                foreach (var subdirectory in directory.Subdirectories)
                {
                    queue.Enqueue(subdirectory);
                }
            }
        }

        return null;
    }
    
    private void DoWithDelay(SelfSustainableTree t, Action action)
    {
        Thread.Sleep(500);
        action();
        Thread.Sleep(500);
        t.StopSynchronization();
    }
    
    [TearDown]
    public void TearDown()
    {
        tree?.StopSynchronization();
        helper.Clear();
    }
}