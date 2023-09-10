// See https://aka.ms/new-console-template for more information

using SpaceProfilerLogic;

var directory = Console.ReadLine();
if (!Directory.Exists(directory))
    return -1;

var tree = new SelfSustainableTree(directory);
tree.StartSynchronization();

Thread.Sleep(10000);

tree.StopSynchronization();

Console.WriteLine($"Total size: {tree.Root.GetSize}");
Console.WriteLine(tree.GetChangedNodes().Length);
return 0;

