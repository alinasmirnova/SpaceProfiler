// See https://aka.ms/new-console-template for more information

using SpaceProfilerLogic;

var directory = Console.ReadLine();
if (!Directory.Exists(directory))
    return -1;

using var tree = new SelfSustainableTree(directory);

Thread.Sleep(10000);

Console.WriteLine($"Total size: {tree.Root.GetSize()}");
Console.WriteLine(tree.GetChangedNodes().Count);
return 0;

