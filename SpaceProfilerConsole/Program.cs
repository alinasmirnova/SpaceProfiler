// See https://aka.ms/new-console-template for more information

using SpaceProfilerLogic;

var directory = Console.ReadLine();

var treeWatcher = FileSystemEntriesTreeBuilder.Build(directory);
if (treeWatcher == null)
{
    Console.WriteLine("Can not open directory");
    return -1;
}

Console.WriteLine("Main thread goes to sleep");
Thread.Sleep(10000);

treeWatcher.Stop();

Console.WriteLine($"Total size: {treeWatcher.Tree.Root.Size}");
return 0;

