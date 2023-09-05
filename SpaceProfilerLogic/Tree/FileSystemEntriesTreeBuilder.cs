﻿namespace SpaceProfilerLogic.Tree;

public static class FileSystemEntriesTreeBuilder
{
    public static FileSystemEntry? Build(string? rootDirectory)
    {
        if (!Directory.Exists(rootDirectory))
            return null;

        var root = new FileSystemEntry(rootDirectory);
        Fill(root);
        return root;
    }

    private static void Fill(FileSystemEntry root)
    {
        var stack = new Stack<FileSystemEntry>();
        stack.Push(root);
        while (stack.TryPeek(out var current))
        {
            if (!current.Children.Any())
            {
                var subDirectories = AddDirectories(current).ToArray();
                foreach (var subDirectory in subDirectories)
                {
                    stack.Push(subDirectory);
                }
                
                if (subDirectories.Any())
                    continue;
            }

            current.Size = SumChildren(current);
            AddFiles(current);
            stack.Pop();
        }
    }

    private static IEnumerable<FileSystemEntry> AddDirectories(FileSystemEntry entry)
    {
        foreach (var directory in Directory.EnumerateDirectories(entry.FullName))
        {
            var child = new FileSystemEntry(directory);
            entry.Children.Add(child);
            yield return child;
        }
    }

    private static long SumChildren(FileSystemEntry entry)
    {
        return entry.Children.Sum(c => c.Size);
    }

    private static void AddFiles(FileSystemEntry entry)
    {
        foreach (var file in Directory.EnumerateFiles(entry.FullName))
        {
            var child = new FileSystemEntry(file, FileSizeCalculator.GetFileSize(file));
            entry.Children.Add(child);
            entry.Size += child.Size;
        }
    }
}