// See https://aka.ms/new-console-template for more information

using SpaceProfilerLogic.Tree;

var directory = Console.ReadLine();

var tree = FileSystemEntriesTreeBuilder.Build(directory);
Console.WriteLine(tree == null ? $"Can not open directory:{directory}" : $"Total size: {tree.Root.Size}");