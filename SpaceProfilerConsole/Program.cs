// See https://aka.ms/new-console-template for more information

using SpaceProfilerLogic;

var directory = Console.ReadLine();

var treeWatcher = SelfSustainableTreeBuilder.Build(directory);
treeWatcher.StartSynchronization();

Console.WriteLine("Main thread goes to sleep");
Thread.Sleep(10000);

treeWatcher.StopSynchronization();

Console.WriteLine($"Total size: {treeWatcher.Root.Size}");
Console.WriteLine(treeWatcher.FlushUpdated().Length);
return 0;

