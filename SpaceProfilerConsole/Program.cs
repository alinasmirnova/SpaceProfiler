// See https://aka.ms/new-console-template for more information

using SpaceProfilerLogic;

var directory = Console.ReadLine();

var treeWatcher = FileSystemEntriesTreeBuilder.Build(directory);
treeWatcher.Start();

Console.WriteLine("Main thread goes to sleep");
Thread.Sleep(10000);

treeWatcher.Stop();

Console.WriteLine($"Total size: {treeWatcher.Tree.Root.Size}");
Console.WriteLine(treeWatcher.FlushUpdated().Length);
return 0;

