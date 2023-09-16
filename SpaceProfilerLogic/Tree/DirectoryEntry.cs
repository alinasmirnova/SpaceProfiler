﻿using System.Collections.Concurrent;

namespace SpaceProfilerLogic.Tree;

public class DirectoryEntry : FileSystemEntry
{
    private const int max_items_count = 500;
    private ConcurrentDictionary<string, FileEntry> files = new();

    public int FilesCount => files.Count;
    public FileEntry[] Files
    {
        get => files.Values.Take(max_items_count).ToArray();
        set
        {
            files = new ConcurrentDictionary<string, FileEntry>();
            foreach (var file in value)
            {
                if (files.TryAdd(file.Name, file))
                    file.Parent = this;
            }
        }
    }

    public bool AddFile(FileEntry file)
    {
        file.Parent = this;
        if (files.TryAdd(file.Name, file))
        {
            AddSize(file.GetSize);
            return true;
        }

        return false;
    }
    
    public bool RemoveFile(FileSystemEntry file)
    {
        if (files.TryRemove(file.Name, out _))
        {
            file.Parent = null;
            AddSize(-file.GetSize);
            return true;
        }

        return false;
    }

    public int SubdirectoriesCount => subdirectories.Count;

    private ConcurrentDictionary<string, DirectoryEntry> subdirectories = new();
    public DirectoryEntry[] Subdirectories
    {
        get => subdirectories.Values.Take(max_items_count).ToArray();
        set
        {
            subdirectories = new ConcurrentDictionary<string, DirectoryEntry>();
            foreach (var subdirectory in value)
            {
                if (subdirectories.TryAdd(subdirectory.Name, subdirectory))
                    subdirectory.Parent = this;
            }
        }
    }

    public bool AddSubdirectory(DirectoryEntry directoryEntry)
    {
        directoryEntry.Parent = this;
        if (subdirectories.TryAdd(directoryEntry.Name, directoryEntry))
        {
            AddSize(directoryEntry.GetSize);
            return true;
        }
        return false;
    }
    
    public bool RemoveSubdirectory(FileSystemEntry directoryEntry)
    {
        if (subdirectories.TryRemove(directoryEntry.Name, out _))
        {
            directoryEntry.Parent = null;
            AddSize(-directoryEntry.GetSize);
            return true;
        }

        return false;
    }

    public bool ContainsFile(string fileName) => files.ContainsKey(fileName);
    public bool ContainsSubdirectory(string fileName) => subdirectories.ContainsKey(fileName);

   
    public DirectoryEntry(string fullName, long size, bool isAccessible, FileSystemEntry? parent) : base(fullName, isAccessible, parent)
    {
        Size = size;
    }

    public DirectoryEntry(string fullName, long size) : this(fullName, size, true, null)
    {
    }
}